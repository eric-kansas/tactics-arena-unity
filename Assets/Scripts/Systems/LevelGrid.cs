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

    private const int BULLSEYE_SECTOR = -1; // Special value for the bullseye sector
    private const float BULLSEYE_RADIUS = 4; // Radius of the bullseye area


    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int radius = 15;
    [SerializeField] private float cellSize;
    [SerializeField] public float elevationScaleFactor = 0.5f;
    [SerializeField] public int numberOfSections;

    [SerializeField] private List<BiomeConfig> biomeConfigs;

    private GridSystem<GridObject> gridSystem;
    private Dictionary<GridPosition, BiomeConfig> regionConfigMap = new Dictionary<GridPosition, BiomeConfig>();

    private Dictionary<int, BiomeConfig> sectorBiomes = new Dictionary<int, BiomeConfig>();

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
        // Step 1: Generate Biome Configuration for Each Sector
        GenerateBiomesForSectors();

        foreach (var region in sectorBiomes)
        {
            Debug.Log(region);
        }

        // Step 2: Fill regionConfigMap Based on Sectors    
        for (int x = 0; x < 2 * radius + 1; x++)
        {
            for (int z = 0; z < 2 * radius + 1; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                if (IsWithinCircle(radius, gridPosition.x, gridPosition.z))
                {
                    int sector = GetSector(gridPosition);
                    BiomeConfig regionConfig = sectorBiomes[sector];
                    regionConfigMap[gridPosition] = regionConfig;
                }
            }
        }
    }

    public bool IsWithinCircle(int radius, int x, int z)
    {
        // Convert grid coordinates to circle coordinates by offsetting by radius
        int circleX = x - radius;
        int circleZ = z - radius;
        return circleX * circleX + circleZ * circleZ <= (radius + 0.5f) * (radius + 0.5f);
    }


    private void GenerateBiomesForSectors()
    {
        for (int sector = -1; sector < numberOfSections; sector++)
        {
            BiomeConfig biome = biomeConfigs[UnityEngine.Random.Range(0, biomeConfigs.Count)];
            sectorBiomes[sector] = biome;
        }
    }


    public int GetSector(GridPosition gridPosition)
    {
        float relativeX = gridPosition.x - radius;
        float relativeZ = gridPosition.z - radius;

        // Check if the position is within the bullseye radius
        if (relativeX * relativeX + relativeZ * relativeZ <= BULLSEYE_RADIUS * BULLSEYE_RADIUS)
        {
            return BULLSEYE_SECTOR; // Return special value for bullseye
        }

        // Calculate the angle and normalize it
        float angle = Mathf.Atan2(relativeZ, relativeX) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360;
        float sectorSize = 360f / numberOfSections;
        angle = (angle + sectorSize / 2) % 360; // Adjust angle to distribute sectors evenly

        // Calculate the sector and ensure it is within bounds
        int sector = Mathf.FloorToInt(angle / sectorSize);
        return Mathf.Min(sector, numberOfSections - 1); // Ensure sector does not exceed max
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


    public int GetNumberOfSections() => numberOfSections;

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