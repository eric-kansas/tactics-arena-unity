using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevationAction : BaseAction
{

    private int maxElevationActionRange = 1;
    private GridPosition targetGridPosition;

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        int elevationChangeRange = 1; // Define the range of the effect
        int elevationChangeAmount = 1; // Define how much to change the elevation

        for (int x = -elevationChangeRange; x <= elevationChangeRange; x++)
        {
            for (int z = -elevationChangeRange; z <= elevationChangeRange; z++)
            {
                GridPosition offsetPosition = new GridPosition(x, z);
                GridPosition targetPosition = targetGridPosition + offsetPosition;

                if (LevelGrid.Instance.IsValidGridPosition(targetPosition))
                {
                    LevelGrid.Instance.ChangeElevation(targetPosition, elevationChangeAmount);
                }
            }
        }

        ActionComplete();

    }

    public override string GetActionName()
    {
        return "Terrain";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 10,
        };
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        if (!IsActionApplicable())
        {
            return validGridPositionList;
        }

        GridPosition unitGridPosition = unit.GetGridPosition();

        for (int x = -maxElevationActionRange; x <= maxElevationActionRange; x++)
        {
            for (int z = -maxElevationActionRange; z <= maxElevationActionRange; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        ActionStart(onActionComplete);
        targetGridPosition = gridPosition;
        isActive = true;
    }

    public int GetMaxElevationActionRange()
    {
        return maxElevationActionRange;
    }

}
