using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReserveGridSystem : MonoBehaviour
{
    public static ReserveGridSystem Instance { get; private set; }

    [SerializeField] private Transform reserveUnitPrefab;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float cellSize;

    private GridSystem<GridObject> reserveGrid;

    void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one ReserveGridSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        reserveGrid = new GridSystem<GridObject>(width, height, cellSize,
            (GridSystem<GridObject> g, GridPosition gridPosition) =>
            {
                return new GridObject(g, gridPosition);
            });
    }

    public void AddUnit(Unit unit)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                GridObject gridObject = reserveGrid.GetGridObject(gridPosition);

                if (!gridObject.HasAnyUnit())
                {
                    // Found an empty position, add the unit here
                    gridObject.AddUnit(unit);
                    return; // Exit the method after adding the unit
                }
            }
        }

        Debug.LogWarning("ReserveGridSystem: No empty position found to add the unit.");
    }

    public void RemoveUnit(Unit unit)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                GridObject gridObject = reserveGrid.GetGridObject(gridPosition);

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


    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = reserveGrid.GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }

    public void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = reserveGrid.GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }

}
