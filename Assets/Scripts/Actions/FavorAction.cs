using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FavorAction : BaseAction
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
            unit.AddFavor(CalculateFavor());
            ActionComplete(GameEvent.FavorRoll);
        }
    }

    private int CalculateFavor()
    {
        System.Random random = new System.Random();
        int favorRoll = random.Next(1, 21);

        favorRoll = favorRoll + ModifiersCalculator.FavorModifier(unit);
        return favorRoll;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        totalSpinAmount = 0f;

        ActionStart(onActionComplete);
    }


    public override string GetActionName()
    {
        return "Favor";
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