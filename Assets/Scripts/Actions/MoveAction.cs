using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveAction : BaseAction
{
    // Events
    public static event Action OnAnyMoveActionStopped;
    public static event Action<Unit> OnAnyOpportunityAttackTriggered;
    public event Action OnStartMoving;
    public event Action OnStopMoving;

    // Movement
    private const float RotateSpeed = 10f;
    private const float MoveSpeed = 4f;
    private const float StoppingDistance = 0.05f;

    private List<Vector3> positionList;
    private int currentPositionIndex;
    private bool isWaitingForEventResponse;
    private bool rotateToUprightOnNextUpdate = false;
    private bool haltMoving;
    private bool opportunityAttackMissed;

    // UI and Visuals
    private static GameObject traversalPathVisualPrefab;
    private GameObject traversalPathVisualInstance;

    // Other fields
    private IOpportunityAttack opportunityAttack;

    private void Update()
    {
        if (CanUpdate())
        {
            UpdateMovement();
        }

        if (rotateToUprightOnNextUpdate)
        {
            RotateTowardsUpright();
        }
    }

    private bool CanUpdate()
    {
        return isActive && !isWaitingForEventResponse;
    }

    // Movement Handling
    private void UpdateMovement()
    {
        if (currentPositionIndex < positionList.Count && !haltMoving)
        {
            HandleMovement();
        }
        else if (currentPositionIndex >= positionList.Count || haltMoving)
        {
            FinalizeMove();
        }
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = positionList[currentPositionIndex];
        RotateTowardsTarget(targetPosition);
        MoveTowardsTarget(targetPosition);
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        if (Vector3.Distance(transform.position, targetPosition) > StoppingDistance)
        {
            transform.position += (targetPosition - transform.position).normalized * MoveSpeed * Time.deltaTime;
        }
        else
        {
            OnReachTargetPosition();
        }
    }

    private void RotateTowardsTarget(Vector3 targetPosition)
    {
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotateSpeed * Time.deltaTime);
    }

    private void RotateTowardsUpright()
    {
        Vector3 currentEulerAngles = transform.rotation.eulerAngles;
        Quaternion targetRotation = Quaternion.Euler(0, currentEulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * RotateSpeed);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f) // 1 degree tolerance
        {
            rotateToUprightOnNextUpdate = false;
        }
    }

    private void OnReachTargetPosition()
    {
        CheckForSpecialConditions(currentPositionIndex);
        currentPositionIndex++;
    }

    // Opportunity Attack Handling
    private void CheckForSpecialConditions(int positionIndex)
    {
        if (positionIndex < positionList.Count - 1)
        {
            GridPosition currentGridPosition = GetGridPosition(positionList[positionIndex]);
            GridPosition nextGridPosition = GetGridPosition(positionList[positionIndex + 1]);
            CheckOpportunityAttack(currentGridPosition, nextGridPosition);
        }
    }

    private void CheckOpportunityAttack(GridPosition current, GridPosition next)
    {
        (bool wouldTrigger, Unit enemy) = LevelGrid.Instance.TriggersOpportunityAttack(current, next);
        if (wouldTrigger)
        {
            TriggerOpportunityAttack(enemy, current);
        }
    }

    private void TriggerOpportunityAttack(Unit enemy, GridPosition position)
    {
        opportunityAttack = enemy.GetAction<IOpportunityAttack>();
        if (opportunityAttack == null)
        {
            return;
        }
        opportunityAttack.OnAttackMissed += OpportunityAttack_OnAttackMissed;
        opportunityAttack.TakeAction(position, opportunityAttackActionComplete);
        isWaitingForEventResponse = true;
    }

    private void OpportunityAttack_OnAttackMissed()
    {
        opportunityAttackMissed = true;
    }

    private void opportunityAttackActionComplete()
    {
        // If the player was hit, stop moving.
        haltMoving = !opportunityAttackMissed;

        // Reset the flag for future attacks.
        opportunityAttackMissed = false;

        // Unsubscribe from the event and reset the melee action.
        if (opportunityAttack != null)
        {
            opportunityAttack.OnAttackMissed -= OpportunityAttack_OnAttackMissed;
            opportunityAttack = null;
        }

        // Resume the update loop.
        isWaitingForEventResponse = false;
    }

    // Action Finalization
    private void FinalizeMove()
    {
        rotateToUprightOnNextUpdate = true;
        FireMoveActionStopped();
        ClearPreview();
        ActionComplete();
    }

    protected virtual void FireMoveActionStarted()
    {
        OnStartMoving?.Invoke();
    }

    protected virtual void FireMoveActionStopped()
    {
        OnStopMoving?.Invoke();
    }

    public override void PreviewAction(GridPosition gridPosition)
    {
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(unit.GetTeam(), unit.GetGridPosition(), gridPosition, out int pathLength, GetMoveValue());
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

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        if (!IsActionApplicable())
        {
            return validGridPositionList;
        }

        GridPosition unitGridPosition = unit.GetGridPosition();
        int moveDistance = GetMoveValue();
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

    // Utility Methods
    private GridPosition GetGridPosition(Vector3 position)
    {
        return LevelGrid.Instance.GetGridPosition(position);
    }

    private int GetMoveValue()
    {
        return unit.GetStats().MoveValue;
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

    public static void SetTraversalPathVisualPrefab(GameObject prefab)
    {
        traversalPathVisualPrefab = prefab;
    }

    // Overridden and Interface Methods
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        List<GridPosition> pathGridPositionList = Pathfinding.Instance.FindPath(unit.GetTeam(), unit.GetGridPosition(), gridPosition, out int pathLength, GetMoveValue());

        currentPositionIndex = 0;
        positionList = new List<Vector3>();

        foreach (GridPosition pathGridPosition in pathGridPositionList)
        {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }

        FireMoveActionStarted();
        ActionStart(onActionComplete);
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

}