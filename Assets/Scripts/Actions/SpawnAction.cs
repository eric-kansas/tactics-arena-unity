using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAction : BaseAction
{
    public static event Action<Unit> OnAnyStartSpawn;
    public event Action<Unit> OnStopSpawn;

    private GridPosition targetGridPosition;

    protected override void Awake()
    {
        base.Awake();
        actionUsage = ActionUsage.Reserves;
    }

    private void Update()
    {
        if (!isActive)
            return;

        HandleSpawning();
    }

    private void HandleSpawning()
    {
        transform.position = LevelGrid.Instance.GetWorldPosition(targetGridPosition);
        Debug.Log(targetGridPosition);
        unit.SetInArena(true);
        ActionComplete(GameEvent.UnitSpawn);
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetGridPosition = gridPosition;
        ActionStart(onActionComplete);
        OnAnyStartSpawn?.Invoke(unit);
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        List<GridPosition> validPositions = new List<GridPosition>();
        foreach (Zone zone in Match.Instance.GetSpawnZonesForTeam(unit.GetTeam()))
        {
            validPositions.AddRange(GetValidPositionsInZone(zone));
        }
        return validPositions;
    }

    private IEnumerable<GridPosition> GetValidPositionsInZone(Zone zone)
    {
        foreach (GridPosition pos in zone.GridPositions())
        {
            if (LevelGrid.Instance.IsValidGridPosition(pos) && !LevelGrid.Instance.HasAnyUnitOnGridPosition(pos))
                yield return pos;
        }
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
