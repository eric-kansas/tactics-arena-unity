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
    public VisibilityState Visibility { get; set; }
}

public class FogOfWarSystem : MonoBehaviour
{
    public static FogOfWarSystem Instance { get; private set; }

    public static Action<Team> OnTeamVisbilityChanged;

    [SerializeField] private LayerMask terrainPlaneLayerMask;
    private GridCell[,] grid;
    private HashSet<GridPosition> currentlyVisibleCells;
    private Dictionary<Team, HashSet<GridPosition>> visibilityByTeam;
    private Dictionary<Team, Dictionary<GridPosition, int>> knownElevationByTeam;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitManager! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPositionEventArgs;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;

    }

    private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    {
        UpdateVisibilityForTeam(Match.Instance.GetClientTeam());
    }

    private void LevelGrid_OnAnyUnitMovedGridPositionEventArgs(object sender, LevelGrid.OnAnyUnitMovedGridPositionEventArgs e)
    {
        UpdateVisibilityForTeam(Match.Instance.GetClientTeam());
    }

    public void Start()
    {
        visibilityByTeam = new Dictionary<Team, HashSet<GridPosition>>();
        visibilityByTeam[Match.Instance.GetClientTeam()] = new HashSet<GridPosition>();
        visibilityByTeam[Match.Instance.GetAwayTeam()] = new HashSet<GridPosition>();

        knownElevationByTeam = new Dictionary<Team, Dictionary<GridPosition, int>>();
        knownElevationByTeam[Match.Instance.GetClientTeam()] = new Dictionary<GridPosition, int>();
        knownElevationByTeam[Match.Instance.GetAwayTeam()] = new Dictionary<GridPosition, int>();

        currentlyVisibleCells = new HashSet<GridPosition>();
        grid = new GridCell[LevelGrid.Instance.GetWidth(), LevelGrid.Instance.GetHeight()];
        InitializeGrid();
    }
    public bool IsVisible(Team team, GridPosition pos)
    {
        return visibilityByTeam[team].Contains(pos);
    }

    public int GetKnownElevation(Team team, GridPosition gridPosition)
    {
        if (knownElevationByTeam[team].TryGetValue(gridPosition, out int knownElevation))
        {
            return knownElevation;
        }
        return 1; // Default elevation if not known
    }

    private void InitializeGrid()
    {
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int z = 0; z < grid.GetLength(1); z++)
            {
                grid[x, z] = new GridCell
                {
                    Elevation = 1,
                    Visibility = VisibilityState.Obscured
                };
            }
        }
    }

    private void UpdateVisibilityForTeam(Team team)
    {
        Debug.Log("UpdateVisibilityForTeam");
        // Clear previous visibility for the team
        foreach (var pos in visibilityByTeam[team])
        {
            grid[pos.x, pos.z].Visibility = VisibilityState.Obscured;
        }
        visibilityByTeam[team].Clear();

        // Update visibility based on positions of all units on this team
        foreach (var unit in UnitManager.Instance.GetTeamArenaUnitList(team))
        {
            UpdateVisibilityForUnit(team, unit.GetGridPosition(), unit.GetPlayerData().GetStats().Perception + 2);
        }
        OnTeamVisbilityChanged?.Invoke(team);
    }

    private void UpdateVisibilityForUnit(Team team, GridPosition unitPosition, int visibilityRadius)
    {
        // Set previously visible cells to obscured
        foreach (var pos in currentlyVisibleCells)
        {
            grid[pos.x, pos.z].Visibility = VisibilityState.Obscured;
        }

        currentlyVisibleCells.Clear();

        // Calculate new visibility
        for (int x = -visibilityRadius; x <= visibilityRadius; x++)
        {
            for (int z = -visibilityRadius; z <= visibilityRadius; z++)
            {
                int gridX = unitPosition.x + x;
                int gridZ = unitPosition.z + z;
                GridPosition checkPos = new GridPosition(gridX, gridZ);
                if (LevelGrid.Instance.IsValidGridPosition(checkPos) && IsWithinVisibilityRadius(unitPosition, checkPos, visibilityRadius))
                {
                    if (HasLineOfSight(unitPosition, checkPos))
                    {
                        grid[gridX, gridZ].Visibility = VisibilityState.Visible;
                        grid[gridX, gridZ].Elevation = LevelGrid.Instance.GetElevationAtGridPosition(checkPos);
                        currentlyVisibleCells.Add(checkPos);
                    }
                }
            }
        }
        // Add visible cells to the team's visibility set
        visibilityByTeam[team].UnionWith(currentlyVisibleCells);
        foreach (var pos in currentlyVisibleCells)
        {
            int currentElevation = LevelGrid.Instance.GetElevationAtGridPosition(pos);
            if (!knownElevationByTeam[team].TryGetValue(pos, out int knownElevation) || currentElevation > knownElevation)
            {
                knownElevationByTeam[team][pos] = currentElevation;
            }
        }
    }

    private bool IsWithinVisibilityRadius(GridPosition unitPosition, GridPosition pos, int visibilityRadius)
    {
        int dx = unitPosition.x - pos.x;
        int dz = unitPosition.z - pos.z;
        return (dx * dx + dz * dz) <= (visibilityRadius * visibilityRadius);
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

    private bool HasLineOfSight(GridPosition fromPos, GridPosition toPos)
    {
        List<GridPosition> line = GetLine(fromPos, toPos);

        int startElevation = LevelGrid.Instance.GetElevationAtGridPosition(fromPos);
        int endElevation = LevelGrid.Instance.GetElevationAtGridPosition(toPos);

        foreach (GridPosition pos in line)
        {
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
        float distanceStartToEnd = Vector2.Distance(new Vector2(startPos.x, startPos.z), new Vector2(endPos.x, endPos.z));
        float distanceStartToCell = Vector2.Distance(new Vector2(startPos.x, startPos.z), new Vector2(cellPos.x, cellPos.z));

        // Calculate the expected elevation at the cell position if there were no elevation changes
        float linearInterpolatedElevation = Mathf.Lerp(startElev, endElev, distanceStartToCell / distanceStartToEnd);

        // Consider some threshold for elevation differences
        float elevationThreshold = LevelGrid.Instance.ElevationScaleFactor * 4; // This value depends on your game's scale

        // Check if the cell's elevation is significantly higher than the linear interpolated elevation
        if (cellElev > linearInterpolatedElevation + elevationThreshold)
        {
            return true; // Elevation blocks sight
        }

        // Additional complexity: You could add checks for specific types of terrain or objects that might be at the cell

        return false; // Elevation does not block sight
    }

    internal Vector3 GetPerceivedWorldPosition(GridPosition gridPosition)
    {
        Vector3 worldPos = LevelGrid.Instance.GetWorldPosition(gridPosition);
        worldPos.y = grid[gridPosition.x, gridPosition.z].Elevation * LevelGrid.Instance.ElevationScaleFactor;
        return worldPos;
    }
}
