using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem<TGridObject>
{
    private int radius = 0;
    private int width;
    private int height;
    private float cellSize;
    private Vector3 worldOffset;

    private TGridObject[,] gridObjectArray;

    public GridSystem(int width, int height, float cellSize, Vector3 worldOffset, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject)
    {
        InitializeGrid(width, height, cellSize, worldOffset, createGridObject);
    }

    // Constructor for circular grid
    public GridSystem(int radius, float cellSize, Vector3 worldOffset, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject)
    {
        this.radius = radius;
        int diameter = radius * 2 + 1; // Adjust for the center cell
        InitializeGrid(diameter, diameter, cellSize, worldOffset, createGridObject);
    }

    private void InitializeGrid(int width, int height, float cellSize, Vector3 worldOffset, Func<GridSystem<TGridObject>, GridPosition, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.worldOffset = worldOffset;

        gridObjectArray = new TGridObject[width, height];

        if (radius > 0)
        {
            // Iterate over the grid and fill cells within the circle's radius
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    if (IsWithinCircle(radius, x, z))
                    {
                        GridPosition gridPosition = new GridPosition(x, z);
                        gridObjectArray[x, z] = createGridObject(this, gridPosition);
                    }
                }
            }
        }
        else
        {
            // Standard grid initialization for non-circular grids
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);
                    gridObjectArray[x, z] = createGridObject(this, gridPosition);
                }
            }
        }
    }

    private bool IsWithinCircle(int radius, int x, int z)
    {
        // Convert grid coordinates to circle coordinates by offsetting by radius
        int circleX = x - radius;
        int circleZ = z - radius;
        return circleX * circleX + circleZ * circleZ <= (radius + 0.5f) * (radius + 0.5f);
    }


    public Vector3 GetWorldPosition(GridPosition gridPosition)
    {
        return new Vector3(gridPosition.x, 0, gridPosition.z) * cellSize + worldOffset;
    }

    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        return new GridPosition(
            Mathf.RoundToInt(worldPosition.x / cellSize),
            Mathf.RoundToInt(worldPosition.z / cellSize)
        );
    }

    public void CreateDebugObjects(Transform debugPrefab)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (radius == 0 || IsWithinCircle(radius, x, z))
                {
                    GridPosition gridPosition = new GridPosition(x, z);

                    Transform debugTransform = GameObject.Instantiate(debugPrefab, GetWorldPosition(gridPosition), Quaternion.identity);
                    GridDebugObject gridDebugObject = debugTransform.GetComponent<GridDebugObject>();

                    if (gridDebugObject != null)
                    {
                        gridDebugObject.SetGridObject(GetGridObject(gridPosition));
                    }
                }
            }
        }
    }

    public TGridObject GetGridObject(GridPosition gridPosition)
    {
        return gridObjectArray[gridPosition.x, gridPosition.z];
    }

    public bool IsValidGridPosition(GridPosition gridPosition)
    {
        if (radius != 0)
        {
            return gridPosition.x >= 0 &&
                   gridPosition.z >= 0 &&
                   gridPosition.x < width &&
                   gridPosition.z < height &&
                   IsWithinCircle(radius, gridPosition.x, gridPosition.z);
        }

        return gridPosition.x >= 0 &&
               gridPosition.z >= 0 &&
               gridPosition.x < width &&
               gridPosition.z < height;
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    public int GetRadius()
    {
        return radius;
    }

}