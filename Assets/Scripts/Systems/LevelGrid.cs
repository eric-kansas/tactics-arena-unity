using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGrid : MonoBehaviour
{

    public static LevelGrid Instance { get; private set; }

    public Action<OnAnyUnitMovedGridPositionEventArgs> OnAnyUnitMovedGridPosition;
    public class OnAnyUnitMovedGridPositionEventArgs : EventArgs
    {
        public Unit unit;
        public GridPosition fromGridPosition;
        public GridPosition toGridPosition;
    }

    public event Action<GridPosition, int> OnElevationChanged;

    private float elevationScaleFactor = 0.5f;

    public float ElevationScaleFactor => elevationScaleFactor;

    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;

    private GridSystem<GridObject> gridSystem;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one LevelGrid! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gridSystem = new GridSystem<GridObject>(width, height, cellSize, Vector3.zero,
            (GridSystem<GridObject> g, GridPosition gridPosition) =>
            {
                var (terrain, elevation) = GetRandomTerrainAndElevation();
                return new GridObject(g, gridPosition, terrain, elevation);
            });

        gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
    }

    private (TerrainType, int) GetRandomTerrainAndElevation()
    {
        Array terrainTypes = Enum.GetValues(typeof(TerrainType));
        TerrainType randomTerrain = TerrainType.Snow;//(TerrainType)terrainTypes.GetValue(UnityEngine.Random.Range(0, terrainTypes.Length));

        int randomElevation = UnityEngine.Random.Range(1, 8);

        return (randomTerrain, randomElevation);
    }

    private void Start()
    {
        //Pathfinding.Instance.Setup(width, height, cellSize);
    }

    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }

    public List<Unit> GetUnitListAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetUnitList();
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }

    public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitAtGridPosition(fromGridPosition, unit);

        AddUnitAtGridPosition(toGridPosition, unit);

        OnAnyUnitMovedGridPosition?.Invoke(new OnAnyUnitMovedGridPositionEventArgs
        {
            unit = unit,
            fromGridPosition = fromGridPosition,
            toGridPosition = toGridPosition,
        });
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);

    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        Vector3 basePosition = gridSystem.GetWorldPosition(gridPosition);

        int elevation = GetElevationAtGridPosition(gridPosition);

        basePosition.y += elevation * elevationScaleFactor;

        return basePosition;
    }

    public bool IsValidGridPosition(GridPosition gridPosition) => gridSystem.IsValidGridPosition(gridPosition);

    public int GetWidth() => gridSystem.GetWidth();

    public int GetHeight() => gridSystem.GetHeight();

    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public Unit GetUnitAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }

    public void SetInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.SetInteractable(interactable);
    }

    public void ClearInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        gridObject.ClearInteractable();
    }

    public int GetElevationAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetElevation();
    }

    public TerrainType GetTerrainTypeAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = gridSystem.GetGridObject(gridPosition);
        return gridObject.GetTerrainType();
    }

    public void ChangeElevation(GridPosition position, int amount)
    {
        // Change the elevation logic
        GridObject gridObject = gridSystem.GetGridObject(position);
        int newElevation = gridObject.GetElevation() + amount;
        gridObject.SetElevation(newElevation);

        OnElevationChanged?.Invoke(position, newElevation);
    }

    public Vector3 GetWorldPositionFromRect(Rect rect)
    {
        Vector3 worldPosition = new Vector3(rect.center.x * cellSize, 1, rect.center.y * cellSize);
        return worldPosition;
    }

    internal float GetCellSize()
    {
        return cellSize;
    }

    public (bool, Unit) TriggersOpportunityAttack(GridPosition fromPos, GridPosition toPos)
    {
        // Get all neighbor positions of the fromNode, including diagonals
        List<GridPosition> neighborPositions = GridUtil.GetAllNeighborPositions(fromPos);
        Unit enemy = null;
        foreach (var neighborPos in neighborPositions)
        {
            // Check if there's an enemy unit at the neighbor position
            if (HasEnemyUnitAtGridPosition(neighborPos))
            {
                // Check if the toNode is not adjacent to this enemy unit
                if (!GridUtil.IsAdjacent(neighborPos, toPos))
                {
                    enemy = GetUnitAtGridPosition(neighborPos);
                    return (true, enemy); // Moving from fromNode to toNode triggers an opportunity attack
                }
            }
        }

        return (false, enemy); // No opportunity attack triggered
    }

    internal bool HasEnemyUnitAtGridPosition(GridPosition pos)
    {
        if (!IsValidGridPosition(pos))
        {
            return false;
        }

        Unit unit = GetUnitAtGridPosition(pos);
        if (unit != null && unit.GetTeam() != TurnSystem.Instance.GetCurrentTeam())
        {
            return true;
        }
        return false;
    }
}