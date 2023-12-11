using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushAction : BaseAction
{
    // events
    public static event Action OnAnyPushHit;
    public event Action OnPushActionStarted;
    public event Action OnPushActionCompleted;
    public event Action OnPushMissed;

    // state
    private enum State
    {
        PushingBeforeHit,
        ChoosingPushDestination,
        PushingAfterHit,
    }

    private State state;
    private float stateTimer;
    private Unit targetUnit;

    private int maxPushTargetingDistance = 1;
    private float afterHitStateTime = 0.5f;


    private void Update()
    {
        if (!isActive) return;

        stateTimer -= Time.deltaTime;
        HandleState();
    }

    private void HandleState()
    {
        switch (state)
        {
            case State.PushingBeforeHit:
                HandlePushingBeforeHit();
                break;
            case State.ChoosingPushDestination:
                HandleChoosingPushDestination();
                break;
            case State.PushingAfterHit:
                HandlePushingAfterHit();
                break;
        }

        if (stateTimer <= 0f)
        {
            NextState();
        }
    }

    private void HandlePushingBeforeHit()
    {
        Vector3 aimDir = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotateSpeed);
    }

    private void HandleChoosingPushDestination()
    {
        throw new NotImplementedException();
    }


    private void HandlePushingAfterHit()
    {

    }

    private void NextState()
    {
        switch (state)
        {
            case State.PushingBeforeHit:

                if (DoesPushHit(targetUnit))
                {
                    state = State.ChoosingPushDestination;
                    OnAnyPushHit?.Invoke();
                }
                else
                {
                    stateTimer = afterHitStateTime;
                    state = State.PushingAfterHit;
                    OnPushMissed?.Invoke(); // Invoke the miss event
                }
                break;
            case State.ChoosingPushDestination:
                stateTimer = afterHitStateTime;
                state = State.PushingAfterHit;
                break;
            case State.PushingAfterHit:
                OnPushActionCompleted?.Invoke();
                ActionComplete(GameEvent.AttackHit);
                break;
        }
    }

    private bool DoesPushHit(Unit target)
    {
        System.Random random = new System.Random();
        int attackRoll = random.Next(1, 21);
        int armorCheck = ModifiersCalculator.PhysicalArmor(target);
        int modifier = ModifiersCalculator.PushModifer(unit);

        attackRoll += modifier;

        return attackRoll >= armorCheck;
    }

    public override string GetActionName()
    {
        return "Push";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 200,
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }

    public List<GridPosition> GetValidActionGridPositionList(GridPosition unitGridPosition)
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();
        if (!IsActionApplicable())
        {
            return validGridPositionList;
        }

        for (int x = -maxPushTargetingDistance; x <= maxPushTargetingDistance; x++)
        {
            for (int z = -maxPushTargetingDistance; z <= maxPushTargetingDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    // Grid Position is empty, no Unit
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(testGridPosition);

                if (targetUnit.GetTeam() == unit.GetTeam())
                {
                    // Both Units on same 'team'
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitAtGridPosition(gridPosition);

        state = State.PushingBeforeHit;
        float beforeHitStateTime = 0.7f;
        stateTimer = beforeHitStateTime;

        OnPushActionStarted?.Invoke();

        ActionStart(onActionComplete);
    }

    public int GetMaxPushTargetingDistance()
    {
        return maxPushTargetingDistance;
    }

    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }

}
