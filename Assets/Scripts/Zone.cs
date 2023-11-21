using System.Collections.Generic;
using UnityEngine;

public class Zone
{
    private static GameObject borderVisualPrefab;

    private HashSet<GridPosition> gridPositionsSet;
    private List<GridPosition> gridPositions;

    private GameObject borderVisualInstance; // Instance of the prefab

    public Zone(List<GridPosition> gridPositions)
    {
        this.gridPositions = gridPositions;
        this.gridPositionsSet = new HashSet<GridPosition>(gridPositions);
        if (!VerifyZone())
        {
            Debug.LogWarning("Zone verification failed. The zone might not be closed.");
        }
    }

    public Zone(Rect rect)
    {
        gridPositions = ConvertRectToGridPositions(rect);
        gridPositionsSet = new HashSet<GridPosition>(gridPositions);
        // no need to verify -- built from rect
    }

    // Static method to set the prefab
    public static void SetBorderVisualPrefab(GameObject prefab)
    {
        borderVisualPrefab = prefab;
    }

    private List<GridPosition> ConvertRectToGridPositions(Rect rect)
    {
        List<GridPosition> positions = new List<GridPosition>();
        for (int x = Mathf.FloorToInt(rect.xMin); x < Mathf.CeilToInt(rect.xMax); x++)
        {
            for (int z = Mathf.FloorToInt(rect.yMin); z < Mathf.CeilToInt(rect.yMax); z++)
            {
                positions.Add(new GridPosition(x, z));
            }
        }
        return positions;
    }

    private bool VerifyZone()
    {
        if (gridPositions.Count == 0) return false;

        HashSet<GridPosition> filledPositions = new HashSet<GridPosition>();
        Queue<GridPosition> positionsToCheck = new Queue<GridPosition>();

        // Start flood fill from each boundary position
        foreach (var boundaryPosition in GetBoundaryPositions())
        {
            if (!filledPositions.Contains(boundaryPosition))
            {
                positionsToCheck.Enqueue(boundaryPosition);

                while (positionsToCheck.Count > 0)
                {
                    var currentPosition = positionsToCheck.Dequeue();
                    if (filledPositions.Add(currentPosition))
                    {
                        foreach (var neighbor in GetNeighbors(currentPosition))
                        {
                            if (gridPositionsSet.Contains(neighbor) && !filledPositions.Contains(neighbor))
                            {
                                positionsToCheck.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }
        }

        return filledPositions.Count == gridPositions.Count;
    }


    public List<GridPosition> GetBoundaryPositions()
    {
        var boundaryPositions = new List<GridPosition>();
        foreach (var position in gridPositions)
        {
            if (IsBoundaryPosition(position))
            {
                boundaryPositions.Add(position);
            }
        }
        return boundaryPositions;
    }

    private bool IsBoundaryPosition(GridPosition position)
    {
        foreach (var neighbor in GetNeighbors(position))
        {
            if (!gridPositionsSet.Contains(neighbor))
            {
                return true;
            }
        }
        return false;
    }

    private IEnumerable<GridPosition> GetNeighbors(GridPosition position)
    {
        return new List<GridPosition>
            {
                // Cardinal directions
                new GridPosition(position.x + 1, position.z),
                new GridPosition(position.x - 1, position.z),
                new GridPosition(position.x, position.z + 1),
                new GridPosition(position.x, position.z - 1),
                
                // Diagonal directions
                new GridPosition(position.x + 1, position.z + 1),
                new GridPosition(position.x + 1, position.z - 1),
                new GridPosition(position.x - 1, position.z - 1),
                new GridPosition(position.x - 1, position.z + 1)
            };
    }

    public List<GridPosition> GridPositions()
    {
        return gridPositions;
    }

    public bool InZone(GridPosition position)
    {
        return gridPositionsSet.Contains(position);
    }

    public void ShowBorder(Color color)
    {
        if (borderVisualInstance == null)
        {
            CreateBorderVisual();
        }

        var borderVisual = borderVisualInstance.GetComponent<ZoneBorderVisual>();
        borderVisual.Setup(this);
        borderVisual.ShowBorder(color);
    }

    public void HideBorder()
    {
        if (borderVisualInstance != null)
        {
            GameObject.Destroy(borderVisualInstance);
            borderVisualInstance = null;
        }
    }

    private void CreateBorderVisual()
    {
        if (borderVisualPrefab != null)
        {
            borderVisualInstance = GameObject.Instantiate(borderVisualPrefab);
            // Set parent or position based on the zone's grid positions
            // For example, you can set it at the center of the zone or align it with the grid
        }
        else
        {
            Debug.LogError("Zone border visual prefab is not assigned!");
        }
    }
}
