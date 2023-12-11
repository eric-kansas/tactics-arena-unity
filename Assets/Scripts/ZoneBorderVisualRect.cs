using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class ZoneBorderVisualRect : MonoBehaviour
{
    private Zone zone;
    private float width;
    [SerializeField] private GameObject borderSegmentPrefab; // Reference to your 3D rectangle model prefab
    private List<GameObject> borderSegments = new List<GameObject>(); // To keep track of created segments

    public void Setup(Zone zone, float width = 0.1f)
    {
        this.zone = zone;
        this.width = width;
        UpdateBorderPoints();
    }
    private void UpdateBorderPoints()
    {
        ClearBorderSegments(); // Clear existing border segments

        List<Vector3> borderPoints = CalculateBorderPoints(zone.GetBoundaryPositions());

        // Create a border segment between each pair of points
        for (int i = 0; i < borderPoints.Count; i += 2)
        {
            CreateBorderSegment(borderPoints[i], borderPoints[i + 1]);
        }
    }

    public void ClearBorderSegments()
    {
        foreach (var segment in borderSegments)
        {
            Destroy(segment);
        }
        borderSegments.Clear();
    }

    private void CreateBorderSegment(Vector3 startPoint, Vector3 endPoint)
    {
        GameObject segment = Instantiate(borderSegmentPrefab, startPoint, Quaternion.identity);
        Vector3 direction = endPoint - startPoint;
        segment.transform.rotation = Quaternion.LookRotation(direction);

        Transform prefabVisual = segment.transform.GetChild(0);
        prefabVisual.localScale = new Vector3(width, width, prefabVisual.localScale.z);

        float length = direction.magnitude + width / 2;
        segment.transform.localScale = new Vector3(segment.transform.localScale.x, segment.transform.localScale.y, length);

        borderSegments.Add(segment);
    }

    public void ShowBorder(Color color)
    {
        foreach (var segment in borderSegments)
        {
            var renderer = segment.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color; // Change the color of the segment
            }
            segment.SetActive(true); // Activate the segment
        }
    }

    public void HideBorder()
    {
        foreach (var segment in borderSegments)
        {
            segment.SetActive(false); // Deactivate the segment
        }
    }

    private List<Vector3> CalculateBorderPoints(List<GridPosition> boundaryPositions)
    {
        if (boundaryPositions.Count == 0)
            return new List<Vector3>();

        List<Vector3> points = new List<Vector3>();
        GridPosition currentPos = boundaryPositions[0];
        Direction startDirection = FindFirstEdgeDirection(currentPos);
        Direction tracingDirection = startDirection;
        do
        {
            GridPosition? possibleNextPos = GetNeighbor(currentPos, tracingDirection);
            List<Vector3> newPoints = new List<Vector3>();

            if (possibleNextPos == null || !boundaryPositions.Contains(possibleNextPos.Value))
            {
                // Turn right and add edge
                newPoints = CalculateEdgePoints(currentPos, GetClockwiseDirection(tracingDirection), tracingDirection);
                tracingDirection = GetClockwiseDirection(tracingDirection);
            }
            else
            {
                GridPosition nextPos = possibleNextPos.Value;


                // Check for turn
                Direction counterClockwiseDir = GetCounterClockwiseDirection(tracingDirection);
                GridPosition? potentialTurnPos = GetNeighbor(nextPos, counterClockwiseDir);

                if (potentialTurnPos.HasValue && boundaryPositions.Contains(potentialTurnPos.Value))
                {
                    // Turn left and add edge
                    currentPos = potentialTurnPos.Value;
                    tracingDirection = counterClockwiseDir;
                    newPoints = CalculateEdgePoints(currentPos, counterClockwiseDir, GetCounterClockwiseDirection(tracingDirection));
                }
                else
                {
                    // Continue straight and add edge
                    currentPos = nextPos;
                    newPoints = CalculateEdgePoints(currentPos, tracingDirection, GetCounterClockwiseDirection(tracingDirection));
                }
            }

            //check out if this new points needs a vertical segment
            if (points.Count >= 2)
            {
                Vector3 bottom = points[points.Count - 2];
                Vector3 top = newPoints[1];
                if (Mathf.Abs(bottom.y - top.y) > 0.1)
                {

                    //insert vertical setment
                    points.Add(bottom);
                    points.Add(top);
                }
            }

            points.AddRange(newPoints);
        }
        while (!(currentPos == boundaryPositions[0] && tracingDirection == startDirection));

        return points;
    }

    private List<Vector3> CalculateEdgePoints(GridPosition pos, Direction directionOfTrace, Direction directionToAddEdge)
    {
        Vector3 worldPos = FogOfWarSystem.Instance.GetPerceivedWorldPosition(pos);
        float cellSize = LevelGrid.Instance.GetCellSize();
        float halfCellSize = cellSize / 2f;

        Vector3 point1 = Vector3.zero;
        Vector3 point2 = Vector3.zero;
        List<Vector3> calculatedPoints = new List<Vector3>();

        // Calculate the points based on the direction to add edge
        switch (directionToAddEdge)
        {
            case Direction.North:
                point1 = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z + halfCellSize);
                point2 = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z + halfCellSize);
                break;

            case Direction.East:
                point1 = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z + halfCellSize);
                point2 = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z - halfCellSize);
                break;

            case Direction.South:
                point1 = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z - halfCellSize);
                point2 = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z - halfCellSize);
                break;

            case Direction.West:
                point1 = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z - halfCellSize);
                point2 = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z + halfCellSize);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(directionToAddEdge), $"Unsupported direction: {directionToAddEdge}");
        }

        // Add points in order based on the direction of trace
        if (ShouldReverseOrder(directionOfTrace, directionToAddEdge))
        {
            calculatedPoints.Add(point2);
            calculatedPoints.Add(point1);
        }
        else
        {
            calculatedPoints.Add(point1);
            calculatedPoints.Add(point2);
        }

        return calculatedPoints;
    }

    private void AddEdgePoints(GridPosition pos, Direction directionOfTrace, Direction directionToAddEdge, List<Vector3> points)
    {
        Vector3 worldPos = FogOfWarSystem.Instance.GetPerceivedWorldPosition(pos);
        float cellSize = LevelGrid.Instance.GetCellSize();
        float halfCellSize = cellSize / 2f;

        Vector3 point1 = Vector3.zero;
        Vector3 point2 = Vector3.zero;

        // Calculate the points based on the direction to add edge
        switch (directionToAddEdge)
        {
            case Direction.North:
                point1 = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z + halfCellSize);
                point2 = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z + halfCellSize);
                break;

            case Direction.East:
                point1 = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z + halfCellSize);
                point2 = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z - halfCellSize);
                break;

            case Direction.South:
                point1 = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z - halfCellSize);
                point2 = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z - halfCellSize);
                break;

            case Direction.West:
                point1 = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z - halfCellSize);
                point2 = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z + halfCellSize);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(directionToAddEdge), $"Unsupported direction: {directionToAddEdge}");
        }

        // Add points in order based on the direction of trace
        if (ShouldReverseOrder(directionOfTrace, directionToAddEdge))
        {
            points.Add(point2);
            points.Add(point1);
        }
        else
        {
            points.Add(point1);
            points.Add(point2);
        }
    }

    private bool ShouldReverseOrder(Direction directionOfTrace, Direction directionToAddEdge)
    {
        // Add logic to determine if the order of points should be reversed
        // based on the direction of trace and the direction to add edge
        // Example:
        return (directionOfTrace == Direction.East && directionToAddEdge == Direction.North) ||
               (directionOfTrace == Direction.South && directionToAddEdge == Direction.East) ||
               (directionOfTrace == Direction.West && directionToAddEdge == Direction.South) ||
               (directionOfTrace == Direction.North && directionToAddEdge == Direction.West);
    }


    private Direction FindFirstEdgeDirection(GridPosition pos)
    {
        // Check North
        GridPosition? northNeighbor = GetNeighbor(pos, Direction.North);
        if (northNeighbor == null || !zone.InZone((GridPosition)northNeighbor))
        {
            return Direction.East; // If North is outside, start tracing East
        }
        // Check East
        GridPosition? eastNeighbor = GetNeighbor(pos, Direction.East);
        if (eastNeighbor == null || !zone.InZone((GridPosition)eastNeighbor))
        {
            return Direction.South; // If East is outside, start tracing South
        }
        // Check South
        GridPosition? southNeighbor = GetNeighbor(pos, Direction.South);
        if (southNeighbor == null || !zone.InZone((GridPosition)southNeighbor))
        {
            return Direction.West; // If South is outside, start tracing West
        }
        // Check West
        GridPosition? westNeighbor = GetNeighbor(pos, Direction.West);
        if (westNeighbor == null || !zone.InZone((GridPosition)westNeighbor))
        {
            return Direction.North; // If West is outside, start tracing North
        }

        throw new ArgumentOutOfRangeException("No edge found at this position!!!");
    }

    private GridPosition? GetNeighbor(GridPosition pos, Direction dir)
    {
        GridPosition neighborPos;
        switch (dir)
        {
            case Direction.North:
                neighborPos = new GridPosition(pos.x, pos.z + 1);
                break;
            case Direction.East:
                neighborPos = new GridPosition(pos.x + 1, pos.z);
                break;
            case Direction.South:
                neighborPos = new GridPosition(pos.x, pos.z - 1);
                break;
            case Direction.West:
                neighborPos = new GridPosition(pos.x - 1, pos.z);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dir), $"Unsupported direction: {dir}");
        }

        if (LevelGrid.Instance.IsValidGridPosition(neighborPos))
        {
            return neighborPos;
        }

        return null; // Return null if the neighbor is outside the grid bounds
    }

    private Direction GetClockwiseDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North:
                return Direction.East;
            case Direction.East:
                return Direction.South;
            case Direction.South:
                return Direction.West;
            case Direction.West:
                return Direction.North;
            default:
                throw new ArgumentOutOfRangeException(nameof(dir), $"Unsupported direction: {dir}");
        }
    }

    private Direction GetCounterClockwiseDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.North:
                return Direction.West;
            case Direction.East:
                return Direction.North;
            case Direction.South:
                return Direction.East;
            case Direction.West:
                return Direction.South;
            default:
                throw new ArgumentOutOfRangeException(nameof(dir), $"Unsupported direction: {dir}");
        }
    }
}
