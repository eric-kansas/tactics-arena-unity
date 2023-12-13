using System;
using System.Collections.Generic;
using UnityEngine;

public enum VisibilityState
{
    Obscured,
    Visible
}

public class GridCell
{
    public int Elevation { get; set; }
    public TerrainType Terrain { get; set; }

    public VisibilityState Visibility { get; set; }
}

public class FogOfWarSystem : MonoBehaviour
{
    public static FogOfWarSystem Instance { get; private set; }
    public static event Action<Team> OnTeamVisibilityChanged;

    [SerializeField] private LayerMask terrainPlaneLayerMask;
    private GridCell[,] grid;
    private HashSet<GridPosition> currentlyVisibleCells = new HashSet<GridPosition>();

    private Dictionary<Team, HashSet<GridPosition>> visibilityByTeam = new Dictionary<Team, HashSet<GridPosition>>();
    private Dictionary<Team, Dictionary<GridPosition, TerrainType?>> knownTerrainByTeam = new Dictionary<Team, Dictionary<GridPosition, TerrainType?>>();

    private Dictionary<Team, Dictionary<GridPosition, int>> knownElevationByTeam = new Dictionary<Team, Dictionary<GridPosition, int>>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one FogOfWarSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPositionEventArgs;
    }

    public void Start()
    {
        visibilityByTeam[Match.Instance.GetClientTeam()] = new HashSet<GridPosition>();
        visibilityByTeam[Match.Instance.GetAwayTeam()] = new HashSet<GridPosition>();

        knownElevationByTeam[Match.Instance.GetClientTeam()] = new Dictionary<GridPosition, int>();
        knownElevationByTeam[Match.Instance.GetAwayTeam()] = new Dictionary<GridPosition, int>();

        knownTerrainByTeam[Match.Instance.GetClientTeam()] = new Dictionary<GridPosition, TerrainType?>();
        knownTerrainByTeam[Match.Instance.GetAwayTeam()] = new Dictionary<GridPosition, TerrainType?>();


        InitializeGrid();
    }

    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        UpdateVisibilityForTeam(Match.Instance.GetClientTeam());
    }

    private void LevelGrid_OnAnyUnitMovedGridPositionEventArgs(LevelGrid.OnAnyUnitMovedGridPositionEventArgs e)
    {
        UpdateVisibilityForTeam(Match.Instance.GetClientTeam());
    }

    public bool IsVisible(Team team, GridPosition pos) => visibilityByTeam[team].Contains(pos);

    public int GetKnownElevation(Team team, GridPosition gridPosition)
    {
        if (knownElevationByTeam[team].TryGetValue(gridPosition, out int knownElevation))
        {
            return knownElevation;
        }
        return -1; // Default elevation if not known
    }

    private void InitializeGrid()
    {
        int radius = LevelGrid.Instance.GetRadius();
        int diameter = LevelGrid.Instance.GetRadius() * 2 + 1;
        grid = new GridCell[diameter, diameter];

        for (int x = 0; x < diameter; x++)
        {
            for (int z = 0; z < diameter; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z); // Adjust for circle center
                if (LevelGrid.Instance.IsValidGridPosition(gridPosition))
                {
                    grid[x, z] = new GridCell { Elevation = 1, Visibility = VisibilityState.Obscured };
                }
            }
        }
    }

    private void UpdateVisibilityForTeam(Team team)
    {
        ClearPreviousVisibility(team);
        foreach (var unit in UnitManager.Instance.GetTeamArenaUnitList(team))
        {
            UpdateVisibilityForUnit(team, unit.GetGridPosition(), ModifiersCalculator.SightDistance(unit));
        }
        OnTeamVisibilityChanged?.Invoke(team);
    }

    private void ClearPreviousVisibility(Team team)
    {
        foreach (var pos in visibilityByTeam[team])
        {
            grid[pos.x, pos.z].Visibility = VisibilityState.Obscured;
        }
        visibilityByTeam[team].Clear();
    }



    private void UpdateVisibilityForUnit(Team team, GridPosition unitPosition, int visibilityRadius)
    {
        HashSet<GridPosition> currentlyVisibleCells = new HashSet<GridPosition>();
        for (int x = -visibilityRadius; x <= visibilityRadius; x++)
        {
            for (int z = -visibilityRadius; z <= visibilityRadius; z++)
            {
                GridPosition checkPos = new GridPosition(unitPosition.x + x, unitPosition.z + z);
                if (LevelGrid.Instance.IsValidGridPosition(checkPos) && IsWithinVisibilityRadius(unitPosition, checkPos, visibilityRadius))
                {
                    if (HasLineOfSight(unitPosition, checkPos))
                    {
                        UpdateCellVisibility(team, checkPos);
                        currentlyVisibleCells.Add(checkPos);
                    }
                }
            }
        }
        visibilityByTeam[team].UnionWith(currentlyVisibleCells);
    }

    private void UpdateCellVisibility(Team team, GridPosition checkPos)
    {
        grid[checkPos.x, checkPos.z].Visibility = VisibilityState.Visible;
        grid[checkPos.x, checkPos.z].Elevation = LevelGrid.Instance.GetElevationAtGridPosition(checkPos);
        grid[checkPos.x, checkPos.z].Terrain = LevelGrid.Instance.GetTerrainTypeAtGridPosition(checkPos);

        int currentElevation = LevelGrid.Instance.GetElevationAtGridPosition(checkPos);
        TerrainType currentTerrain = LevelGrid.Instance.GetTerrainTypeAtGridPosition(checkPos);

        knownElevationByTeam[team][checkPos] = currentElevation;
        knownTerrainByTeam[team][checkPos] = currentTerrain;
    }

    private bool IsWithinVisibilityRadius(GridPosition fromPos, GridPosition toPos, int radius)
    {
        int dx = fromPos.x - toPos.x;
        int dz = fromPos.z - toPos.z;
        return (dx * dx + dz * dz) <= (radius * radius);
    }

    private List<GridPosition> GetLine(GridPosition start, GridPosition end)
    {
        List<GridPosition> line = new List<GridPosition>();

        int x = start.x;
        int z = start.z;
        int dx = Math.Abs(end.x - start.x);
        int dz = Math.Abs(end.z - start.z);
        int sx = start.x < end.x ? 1 : -1;
        int sz = start.z < end.z ? 1 : -1;
        int err = dx - dz;

        while (true)
        {
            line.Add(new GridPosition(x, z));
            if (x == end.x && z == end.z) break;

            int e2 = 2 * err;
            if (e2 > -dz) { err -= dz; x += sx; }
            if (e2 < dx) { err += dx; z += sz; }
        }

        return line;
    }

    public bool HasLineOfSight(GridPosition fromPos, GridPosition toPos)
    {
        List<GridPosition> line = GetLine(fromPos, toPos);

        int startElevation = LevelGrid.Instance.GetElevationAtGridPosition(fromPos);
        int endElevation = LevelGrid.Instance.GetElevationAtGridPosition(toPos);

        foreach (GridPosition pos in line)
        {
            if (!LevelGrid.Instance.IsValidGridPosition(pos))
            {
                continue;
            }
            int cellElevation = LevelGrid.Instance.GetElevationAtGridPosition(pos);

            if (DoesElevationBlockSight(startElevation, endElevation, cellElevation, pos, fromPos, toPos))
            {
                return false;
            }
        }

        return true;
    }

    private bool DoesElevationBlockSight(int startElev, int endElev, int cellElev, GridPosition cellPos, GridPosition startPos, GridPosition endPos)
    {
        // Existing calculations
        float distanceStartToEnd = Vector2.Distance(new Vector2(startPos.x, startPos.z), new Vector2(endPos.x, endPos.z));
        float distanceStartToCell = Vector2.Distance(new Vector2(startPos.x, startPos.z), new Vector2(cellPos.x, cellPos.z));
        float linearInterpolatedElevation = Mathf.Lerp(startElev, endElev, distanceStartToCell / distanceStartToEnd);
        float elevationThreshold = LevelGrid.Instance.elevationScaleFactor * 4;

        // Direct obstruction
        if (cellElev > linearInterpolatedElevation + elevationThreshold)
        {
            return true;
        }

        // Additional check for diagonal obstruction
        if (IsDiagonal(startPos, endPos))
        {
            if (IsDiagonalObstructed(startPos, cellPos, startElev))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsDiagonal(GridPosition fromPos, GridPosition toPos)
    {
        return fromPos.x != toPos.x && fromPos.z != toPos.z;
    }

    private bool IsDiagonalObstructed(GridPosition startPos, GridPosition diagPos, int observerElev)
    {
        int diagElev = LevelGrid.Instance.GetElevationAtGridPosition(diagPos);

        // Get the elevations of the adjacent positions
        // Initialize adjacent elevations to a value representing an invalid state
        int adjElev1 = int.MinValue;
        int adjElev2 = int.MinValue;

        // Get the elevations of the adjacent positions if they are valid
        if (LevelGrid.Instance.IsValidGridPosition(new GridPosition(diagPos.x, startPos.z)))
        {
            adjElev1 = LevelGrid.Instance.GetElevationAtGridPosition(new GridPosition(diagPos.x, startPos.z));
        }

        if (LevelGrid.Instance.IsValidGridPosition(new GridPosition(startPos.x, diagPos.z)))
        {
            adjElev2 = LevelGrid.Instance.GetElevationAtGridPosition(new GridPosition(startPos.x, diagPos.z));
        }

        // Define an elevation threshold
        int elevationThreshold = 2; // Adjust this based on game design

        // Check if both adjacent elevations are higher than the diagonal elevation
        bool bothAdjHigherThanDiag = adjElev1 > diagElev + elevationThreshold && adjElev2 > diagElev + elevationThreshold;

        // Check if observer elevation is not sufficiently higher than adjacent elevations
        bool observerNotHighEnough = observerElev + elevationThreshold < adjElev1 && observerElev + elevationThreshold < adjElev2;

        if (bothAdjHigherThanDiag && observerNotHighEnough)
        {
            // Both adjacent positions are higher and the observer is not high enough to see over them
            return true;
        }

        return false; // Line of sight is not obstructed
    }

    internal Vector3 GetPerceivedWorldPosition(GridPosition gridPosition)
    {
        Vector3 worldPos = LevelGrid.Instance.GetWorldPosition(gridPosition);
        worldPos.y = grid[gridPosition.x, gridPosition.z].Elevation * LevelGrid.Instance.elevationScaleFactor;
        return worldPos;
    }

    public TerrainType? GetKnownTerrain(Team team, GridPosition gridPosition)
    {
        if (knownTerrainByTeam[team].TryGetValue(gridPosition, out TerrainType? knownTerrain))
        {
            return knownTerrain;
        }
        return null;
    }

}
