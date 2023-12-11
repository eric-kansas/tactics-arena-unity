using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAction : BaseAction, IOpportunityAttack
{
    // events
    public static event Action OnAnySwordHit;
    public event Action OnSwordActionStarted;
    public event Action OnSwordActionCompleted;
    public event Action OnAttackMissed;

    // state
    private enum State
    {
        SwingingSwordBeforeHit,
        SwingingSwordAfterHit,
    }

    private State state;
    private float stateTimer;
    private Unit targetUnit;

    private int maxSwordDistance = 1;

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
            case State.SwingingSwordBeforeHit:
                HandleSwingingSwordBeforeHit();
                break;
            case State.SwingingSwordAfterHit:
                HandleSwingingSwordAfterHit();
                break;
        }

        if (stateTimer <= 0f)
        {
            NextState();
        }
    }

    private void HandleSwingingSwordBeforeHit()
    {
        Vector3 aimDir = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotateSpeed);
    }

    private void HandleSwingingSwordAfterHit()
    {

    }

    private void NextState()
    {
        switch (state)
        {
            case State.SwingingSwordBeforeHit:
                state = State.SwingingSwordAfterHit;
                float afterHitStateTime = 0.5f;
                stateTimer = afterHitStateTime;
                if (DoesAttackHit(targetUnit))
                {
                    targetUnit.Damage(CalculateDamage()); // Damage the target if the attack hits
                    OnAnySwordHit?.Invoke();
                }
                else
                {
                    OnAttackMissed?.Invoke(); // Invoke the miss event
                }
                break;
            case State.SwingingSwordAfterHit:
                OnSwordActionCompleted?.Invoke();
                ActionComplete(GameEvent.AttackHit);
                break;
        }
    }

    private int CalculateDamage()
    {
        return 30;
    }

    private bool DoesAttackHit(Unit target)
    {
        System.Random random = new System.Random();
        int attackRoll = random.Next(1, 21);
        int armorCheck = ModifiersCalculator.PhysicalArmor(target);
        int modifier = ModifiersCalculator.PhysicalHitModifier(unit, target);

        attackRoll += modifier;

        return attackRoll >= armorCheck;
    }

    public override string GetActionName()
    {
        return "Melee";
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

        for (int x = -maxSwordDistance; x <= maxSwordDistance; x++)
        {
            for (int z = -maxSwordDistance; z <= maxSwordDistance; z++)
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

        state = State.SwingingSwordBeforeHit;
        float beforeHitStateTime = 0.7f;
        stateTimer = beforeHitStateTime;

        OnSwordActionStarted?.Invoke();

        ActionStart(onActionComplete);
    }

    public int GetMaxSwordDistance()
    {
        return maxSwordDistance;
    }

    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }

}
