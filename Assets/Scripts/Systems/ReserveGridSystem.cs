using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReserveGridSystem : MonoBehaviour
{
    public static ReserveGridSystem Instance { get; private set; }

    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;

    private Dictionary<Team, GridSystem<GridObject>> reserveGrids;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one ReserveGridSystem!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        reserveGrids = new Dictionary<Team, GridSystem<GridObject>>();
    }

    public void RegisterTeam(Team team)
    {
        Vector3 offset = CalculateOffsetForTeam();
        GridSystem<GridObject> reserveGrid = CreateReserveGrid(offset);
        reserveGrid.CreateDebugObjects(gridDebugObjectPrefab);
        reserveGrids[team] = reserveGrid;
    }

    private Vector3 CalculateOffsetForTeam()
    {
        if (reserveGrids.Count == 0)
        {
            return new Vector3(-2 * cellSize, 0, 0);
        }
        else
        {
            return new Vector3((LevelGrid.Instance.GetWidth() * cellSize) + 2, 0, 0);
        }
    }

    private GridSystem<GridObject> CreateReserveGrid(Vector3 offset)
    {
        return new GridSystem<GridObject>(width, height, cellSize, offset,
                 (GridSystem<GridObject> g, GridPosition gridPosition) =>
                 {
                     return new GridObject(g, gridPosition);
                 });
    }

    public GridPosition AddUnit(Team team, Unit unit)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                GridObject gridObject = reserveGrids[team].GetGridObject(gridPosition);

                if (!gridObject.HasAnyUnit())
                {
                    // Found an empty position, add the unit here
                    gridObject.AddUnit(unit);
                    return gridPosition; // Exit the method after adding the unit
                }
            }
        }

        Debug.LogWarning("ReserveGridSystem: No empty position found to add the unit.");
        return new GridPosition(0, 0);
    }

    public void RemoveUnit(Team team, Unit unit)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                GridObject gridObject = reserveGrids[team].GetGridObject(gridPosition);

                if (gridObject.GetUnit() == unit)
                {
                    // Found the unit, remove it
                    gridObject.RemoveUnit(unit);
                    return; // Exit the method after removing the unit
                }
            }
        }

        Debug.LogWarning("ReserveGridSystem: Unit not found in the reserve grid.");
    }


    public void AddUnitAtGridPosition(GridPosition gridPosition, Team team, Unit unit)
    {
        GridObject gridObject = reserveGrids[team].GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Team team, Unit unit)
    {
        GridObject gridObject = reserveGrids[team].GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }

    public Vector3 GetWorldPosition(Team team, GridPosition gridPosition)
    {
        return reserveGrids[team].GetWorldPosition(gridPosition);
    }

}
