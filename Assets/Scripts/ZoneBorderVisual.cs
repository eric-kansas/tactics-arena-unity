using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public enum Direction
{
    North,
    East,
    South,
    West
}

public class ZoneBorderVisual : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private Zone zone;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false; // Initially disable the LineRenderer
    }

    public void Setup(Zone zone)
    {
        this.zone = zone;
        UpdateBorderPoints();
    }

    public void ShowBorder(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.enabled = true;
    }

    public void HideBorder()
    {
        lineRenderer.enabled = false;
    }

    private void UpdateBorderPoints()
    {
        List<Vector3> borderPoints = CalculateBorderPoints(zone.GetBoundaryPositions());
        lineRenderer.positionCount = borderPoints.Count;
        lineRenderer.SetPositions(borderPoints.ToArray());
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

            if (possibleNextPos == null || !boundaryPositions.Contains(possibleNextPos.Value))
            {
                // Turn right and add edge
                AddEdgePoints(currentPos, GetClockwiseDirection(tracingDirection), tracingDirection, points);
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
                    AddEdgePoints(currentPos, counterClockwiseDir, GetCounterClockwiseDirection(tracingDirection), points);
                }
                else
                {
                    // Continue straight and add edge
                    currentPos = nextPos;
                    AddEdgePoints(currentPos, tracingDirection, GetCounterClockwiseDirection(tracingDirection), points);

                }

            }
        }
        while (!(currentPos == boundaryPositions[0] && tracingDirection == startDirection));

        return points;
    }

    private void AddEdgePoints(GridPosition pos, Direction directionOfTrace, Direction directionToAddEdge, List<Vector3> points)
    {
        Vector3 worldPos = LevelGrid.Instance.GetWorldPosition(pos);
        float cellSize = LevelGrid.Instance.GetCellSize();
        float halfCellSize = cellSize / 2f;

        switch (directionToAddEdge)
        {
            case Direction.North:
                // Add North-West and North-East points in order based on direction of trace
                Vector3 pointNW = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z + halfCellSize);
                Vector3 pointNE = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z + halfCellSize);

                if (directionOfTrace == Direction.East)
                {
                    points.Add(pointNW);
                    points.Add(pointNE);
                }
                else // directionOfTrace is West
                {
                    points.Add(pointNE);
                    points.Add(pointNW);
                }
                break;

            case Direction.East:
                // Add North-East and South-East points in order based on direction of trace
                pointNE = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z + halfCellSize);
                Vector3 pointSE = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z - halfCellSize);

                if (directionOfTrace == Direction.South)
                {
                    points.Add(pointNE);
                    points.Add(pointSE);
                }
                else // directionOfTrace is North
                {
                    points.Add(pointSE);
                    points.Add(pointNE);
                }
                break;

            case Direction.South:
                // Add South-East and South-West points in order based on direction of trace
                pointSE = new Vector3(worldPos.x + halfCellSize, worldPos.y, worldPos.z - halfCellSize);
                Vector3 pointSW = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z - halfCellSize);

                if (directionOfTrace == Direction.West)
                {
                    points.Add(pointSE);
                    points.Add(pointSW);
                }
                else // directionOfTrace is East
                {
                    points.Add(pointSW);
                    points.Add(pointSE);
                }
                break;

            case Direction.West:
                // Add South-West and North-West points in order based on direction of trace
                pointSW = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z - halfCellSize);
                pointNW = new Vector3(worldPos.x - halfCellSize, worldPos.y, worldPos.z + halfCellSize);

                if (directionOfTrace == Direction.North)
                {
                    points.Add(pointSW);
                    points.Add(pointNW);
                }
                else // directionOfTrace is South
                {
                    points.Add(pointNW);
                    points.Add(pointSW);
                }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(directionToAddEdge), $"Unsupported direction: {directionToAddEdge}");
        }
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
