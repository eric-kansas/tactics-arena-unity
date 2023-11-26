using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAction : BaseAction
{

    public static Action OnAnyStopMoving;

    [SerializeField] private int maxMoveDistance = 5;

    private static GameObject traversalPathVisualPrefab;
    private List<Vector3> positionList;
    private GameObject traversalPathVisualInstance;
    private int currentPositionIndex;

    private bool isWaitingForEventResponse = false;
    private bool shouldContinueMoving = true;
    private bool attackMissed;
    private MeleeAction meleeAction;

    public static event Action<Unit> OnOpportunityAttack;
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;

    protected void Start()
    {
        maxMoveDistance = unit.GetStats().MoveValue;
    }

    public static void SetTraversalPathVisualPrefab(GameObject prefab)
    {
        traversalPathVisualPrefab = prefab;
    }

    private void Update()
    {
        if (!isActive || isWaitingForEventResponse)
        {
            return;
        }

        if (currentPositionIndex < positionList.Count && shouldContinueMoving)
        {
            HandleMovement();
        }

        if (currentPositionIndex >= positionList.Count || !shouldContinueMoving)
        {
            FinalizeMove();
        }
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = positionList[currentPositionIndex];
        Vector3 moveDirection = (targetPosition - transform.position).normalized;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);

        float stoppingDistance = .1f;
        if (Vector3.Distance(transform.position, targetPosition) > stoppingDistance)
        {
            float moveSpeed = 4f;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
        else
        {
            CheckForSpecialConditions(currentPositionIndex);
            currentPositionIndex++;
        }
    }

    private void FinalizeMove()
    {
        OnStopMoving?.Invoke(this, EventArgs.Empty);
        ClearPreview();
        ActionComplete();
    }

    private void CheckForSpecialConditions(int positionIndex)
    {
        if (positionIndex < positionList.Count - 1)
        {
            GridPosition currentGridPosition = LevelGrid.Instance.GetGridPosition(positionList[positionIndex]);
            GridPosition nextGridPosition = LevelGrid.Instance.GetGridPosition(positionList[positionIndex + 1]);

            // Check if moving from currentGridPosition to nextGridPosition triggers any special condition
            (bool wouldTrigger, Unit enemey) = LevelGrid.Instance.TriggersOpportunityAttack(currentGridPosition, nextGridPosition);

            if (wouldTrigger)
            {
                meleeAction = enemey.GetAction<MeleeAction>();
                meleeAction.OnAttackMissed += MeleeAction_OnAttackMissed;
                enemey.GetAction<MeleeAction>().TakeAction(currentGridPosition, meleeActionComplete);
                isWaitingForEventResponse = true;
            }
        }
    }

    private void MeleeAction_OnAttackMissed(object sender, EventArgs e)
    {
        attackMissed = true;
    }

    private void meleeActionComplete()
    {
        // If the player was hit, stop moving.
        shouldContinueMoving = attackMissed;

        // Reset the flag for future attacks.
        attackMissed = false;

        // Unsubscribe from the event and reset the melee action.
        if (meleeAction != null)
        {
            meleeAction.OnAttackMissed -= MeleeAction_OnAttackMissed;
            meleeAction = null;
        }

        // Resume the update loop.
        isWaitingForEventResponse = false;
    }

    public override void PreviewAction(GridPosition gridPosition)
    {
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(unit.GetTeam(), unit.GetGridPosition(), gridPosition, out int pathLength, maxMoveDistance);
        if (traversalPathVisualInstance == null)
        {
            CreateTraveralPathVisual();
        }

        var traveralPathVisual = traversalPathVisualInstance.GetComponent<TraveralPathVisual>();
        traveralPathVisual.Setup(unit.GetTeam(), pathGridPositionList);
        traveralPathVisual.ShowPath(Color.green);
    }

    public override void ClearPreview()
    {
        if (traversalPathVisualInstance != null)
        {
            GameObject.Destroy(traversalPathVisualInstance);
            traversalPathVisualInstance = null;
        }
    }

    private void CreateTraveralPathVisual()
    {
        if (traversalPathVisualPrefab != null)
        {
            traversalPathVisualInstance = GameObject.Instantiate(traversalPathVisualPrefab);
        }
        else
        {
            Debug.LogError("Traversal visual prefab is not assigned!");
        }
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(unit.GetTeam(), unit.GetGridPosition(), gridPosition, out int pathLength, maxMoveDistance);

        currentPositionIndex = 0;
        positionList = new List<Vector3>();

        foreach (GridPosition pathGridPosition in pathGridPositionList)
        {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }

        OnStartMoving?.Invoke(this, EventArgs.Empty);

        ActionStart(onActionComplete);
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        if (!IsActionApplicable())
        {
            return validGridPositionList;
        }

        GridPosition unitGridPosition = unit.GetGridPosition();
        int moveDistance = maxMoveDistance;
        if (unit.IsProne())
        {
            moveDistance /= 2;
        }

        for (int x = -moveDistance; x <= moveDistance; x++)
        {
            for (int z = -moveDistance; z <= moveDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                if (unitGridPosition == testGridPosition)
                {
                    // Same Grid Position where the unit is already at
                    continue;
                }

                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    // Grid Position already occupied with another Unit
                    continue;
                }

                if (!Pathfinding.Instance.IsWalkableGridPosition(testGridPosition))
                {
                    continue;
                }

                if (!Pathfinding.Instance.HasPath(unit.GetTeam(), unitGridPosition, testGridPosition, moveDistance))
                {
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }


    public override string GetActionName()
    {
        return "Move";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = unit.GetAction<MeleeAction>().GetTargetCountAtPosition(gridPosition);

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = (targetCountAtGridPosition * 2) + 10,
        };
    }

    public void SetMaxMove(int max)
    {
        maxMoveDistance = max;
    }

}