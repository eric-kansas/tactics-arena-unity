using System.Collections.Generic;
using UnityEngine;

public class Zone
{
    private static GameObject borderVisualPrefab;
    public static void SetBorderVisualPrefab(GameObject prefab)
    {
        borderVisualPrefab = prefab;
    }

    private HashSet<GridPosition> gridPositionsSet;
    private List<GridPosition> gridPositions;

    private GameObject borderVisualInstance; // Instance of the prefab

    public Zone(List<GridPosition> gridPositions)
    {
        this.gridPositions = gridPositions;
        this.gridPositionsSet = new HashSet<GridPosition>(gridPositions);
        // if (!VerifyAndCleanZone())
        // {
        //     Debug.LogWarning("Zone verification failed. The zone might not be closed.");
        // }
    }

    public Zone(Rect rect)
    {
        gridPositions = ConvertRectToGridPositions(rect);
        gridPositionsSet = new HashSet<GridPosition>(gridPositions);
        // no need to verify -- built from rect
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

    private bool VerifyAndCleanZone()
    {
        if (gridPositions.Count == 0) return false;

        HashSet<GridPosition> mainSegment = new HashSet<GridPosition>();
        HashSet<GridPosition> visited = new HashSet<GridPosition>();
        Queue<GridPosition> toVisit = new Queue<GridPosition>();

        // Start from the first grid position
        toVisit.Enqueue(gridPositions[0]);

        while (toVisit.Count > 0)
        {
            var current = toVisit.Dequeue();
            if (!visited.Add(current)) continue;

            mainSegment.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (gridPositionsSet.Contains(neighbor) && !visited.Contains(neighbor))
                {
                    toVisit.Enqueue(neighbor);
                }
            }
        }

        // Remove isolated segments
        gridPositions.RemoveAll(pos => !mainSegment.Contains(pos));
        gridPositionsSet.IntersectWith(mainSegment);

        return mainSegment.Count == gridPositions.Count;
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
