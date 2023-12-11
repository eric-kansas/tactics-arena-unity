using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int radius = 15;
    [SerializeField] private float cellSize;
    [SerializeField] public float elevationScaleFactor = 0.5f;

    [SerializeField] private List<BiomeConfig> biomeConfigs;

    private GridSystem<GridObject> gridSystem;
    private Dictionary<GridPosition, BiomeConfig> regionConfigMap = new Dictionary<GridPosition, BiomeConfig>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one LevelGrid! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeGrid();
    }

    private void InitializeGrid()
    {
        CreateTerrainRegions();

        gridSystem = new GridSystem<GridObject>(radius, cellSize, Vector3.zero,
            (GridSystem<GridObject> g, GridPosition gridPosition) =>
            {
                return CreateGridCell(g, gridPosition);
            });

        gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
    }

    private GridObject CreateGridCell(GridSystem<GridObject> gridsystem, GridPosition gridPosition)
    {
        BiomeConfig biomeConfig = ChooseBiomeConfigForPosition(gridPosition);

        TerrainProbability chosenTerrainProbability = ChooseTerrainProbability(biomeConfig);
        TerrainType terrainType = chosenTerrainProbability.TerrainType;
        int elevation = UnityEngine.Random.Range(
            chosenTerrainProbability.ElevationRange.MinElevation,
            chosenTerrainProbability.ElevationRange.MaxElevation + 1
        );

        return new GridObject(gridsystem, gridPosition, terrainType, elevation);
    }

    private TerrainProbability ChooseTerrainProbability(BiomeConfig biomeConfig)
    {
        float randomPoint = UnityEngine.Random.Range(0f, 1f);
        float cumulativeProbability = 0f;

        foreach (var terrainProbability in biomeConfig.terrainProbabilities)
        {
            cumulativeProbability += terrainProbability.Probability;
            if (randomPoint <= cumulativeProbability)
            {
                return terrainProbability;
            }
        }

        // Fallback to the last terrain probability
        return biomeConfig.terrainProbabilities.Last();
    }

    private BiomeConfig ChooseBiomeConfigForPosition(GridPosition gridPosition)
    {
        if (regionConfigMap.TryGetValue(gridPosition, out BiomeConfig biomeConfig))
        {
            return biomeConfig;
        }
        else
        {
            // Fallback if no specific region config is found
            return biomeConfigs[UnityEngine.Random.Range(0, biomeConfigs.Count)];
        }
    }

    private void CreateTerrainRegions()
    {
        // Divide the grid into regions and assign a biome to each
        int numRegionsX = 3;
        int numRegionsZ = 3;
        int regionWidth = width / numRegionsX;
        int regionHeight = height / numRegionsZ;

        for (int x = 0; x < numRegionsX; x++)
        {
            for (int z = 0; z < numRegionsZ; z++)
            {
                int startX = x * regionWidth;
                int startZ = z * regionHeight;
                BiomeConfig regionConfig = ChooseRandomRegionConfig();
                ApplyRegionConfigToGrid(startX, startZ, regionWidth, regionHeight, regionConfig);
            }
        }
    }

    private void ApplyRegionConfigToGrid(int startX, int startZ, int regionWidth, int regionHeight, BiomeConfig regionConfig)
    {
        for (int x = startX; x < startX + regionWidth; x++)
        {
            for (int z = startZ; z < startZ + regionHeight; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                regionConfigMap[gridPosition] = regionConfig;
            }
        }
    }

    private BiomeConfig ChooseRandomRegionConfig()
    {
        return biomeConfigs[UnityEngine.Random.Range(0, biomeConfigs.Count)];
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
        if (!IsValidGridPosition(gridPosition))
        {
            return;
        }

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

    public int GetRadius() => gridSystem.GetRadius();

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