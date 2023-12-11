using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAction : BaseAction
{
    private float totalSpinAmount;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        float spinAddAmount = 360f * Time.deltaTime;
        transform.eulerAngles += new Vector3(0, spinAddAmount, 0);

        totalSpinAmount += spinAddAmount;
        if (totalSpinAmount >= 360f)
        {
            unit.UseFavor();
            ActionComplete(GameEvent.TeamFavorChanged);
        }
    }

    public override bool MeetsRequirements(GridPosition gridPosition)
    {
        if (unit.GetFavorNormalized() != 1.0)
        {
            return false;
        }
        return true;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        totalSpinAmount = 0f;

        ActionStart(onActionComplete);
    }


    public override string GetActionName()
    {
        return "special";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        if (!unit.IsInArena())
        {
            return new List<GridPosition>();
        }

        GridPosition unitGridPosition = unit.GetGridPosition();

        return new List<GridPosition>
        {
            unitGridPosition
        };
    }

    public override int GetActionPointsCost()
    {
        return 1;
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0,
        };
    }

}