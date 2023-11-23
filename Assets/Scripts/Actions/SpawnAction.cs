using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAction : BaseAction
{
    public event EventHandler OnStartSpawn;
    public event EventHandler OnStopSpawn;
    private GridPosition targetGridPosition;

    protected override void Awake()
    {
        base.Awake();
        actionUsage = ActionUsage.Reserves;
    }

    private void Update()
    {
        if (!isActive)
        {
            return;
        }

        transform.position = LevelGrid.Instance.GetWorldPosition(targetGridPosition);

        GridPosition gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitAtGridPosition(gridPosition, unit);
        unit.SetInArena(true);
        ActionComplete();
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetGridPosition = gridPosition;
        ActionStart(onActionComplete);
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> result = new List<GridPosition>();
        foreach (Zone zone in Match.Instance.GetSpawnZonesForTeam(unit.GetTeam()))
        {
            foreach (GridPosition pos in zone.GridPositions())
            {

                if (!LevelGrid.Instance.IsValidGridPosition(pos))
                {
                    continue;
                }

                if (LevelGrid.Instance.HasAnyUnitOnGridPosition(pos))
                {
                    // Grid Position is empty, no Unit
                    continue;
                }

                result.Add(pos);
            }

        }
        return result;
    }


    public override string GetActionName()
    {
        return "Spawn";
    }

    public override EnemyAIAction GetEnemyAIAction(GridPosition gridPosition)
    {

        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 100,
        };
    }

}