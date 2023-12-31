using System;
using System.Collections.Generic;
using UnityEngine;

public enum CoverLevel
{
    None,
    Partial,
    Full
}


public class CoverSystem : MonoBehaviour
{
    public static CoverSystem Instance { get; private set; }

    private Dictionary<GridPosition, Dictionary<Direction, CoverLevel>> coverMap;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one CoverSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Start()
    {
        coverMap = new Dictionary<GridPosition, Dictionary<Direction, CoverLevel>>();
        LevelGrid.Instance.OnElevationChanged += HandleElevationChange;
        InitializeCoverMap();
    }

    private void InitializeCoverMap()
    {
        int radius = LevelGrid.Instance.GetRadius(); // Assuming LevelGrid provides radius
        int diameter = radius * 2 + 1;

        for (int x = 0; x < diameter; x++)
        {
            for (int z = 0; z < diameter; z++)
            {
                if (LevelGrid.Instance.IsValidGridPosition(new GridPosition(x - radius, z - radius)))
                {
                    GridPosition position = new GridPosition(x - radius, z - radius); // Adjust for circle center
                    UpdateCoverForPosition(position);
                }
            }
        }
    }

    private void HandleElevationChange(GridPosition position, int newElevation)
    {
        // Update the cover for the position with the elevation change
        UpdateCoverForPosition(position);

        // Update the cover for adjacent positions
        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
        {
            GridPosition adjacentPosition = GetPositionInDirection(position, direction);
            if (LevelGrid.Instance.IsValidGridPosition(adjacentPosition))
            {
                UpdateCoverForPosition(adjacentPosition);
            }
        }
    }
    private void UpdateCoverForPosition(GridPosition position)
    {
        Dictionary<Direction, CoverLevel> positionCover = new Dictionary<Direction, CoverLevel>();

        foreach (Direction direction in Enum.GetValues(typeof(Direction)))
        {
            CoverLevel coverLevel = CalculateCoverForDirection(position, direction);
            positionCover[direction] = coverLevel;
        }

        coverMap[position] = positionCover;
    }

    private CoverLevel CalculateCoverForDirection(GridPosition currentPosition, Direction direction)
    {
        GridPosition checkPosition = GetPositionInDirection(currentPosition, direction);

        if (!LevelGrid.Instance.IsValidGridPosition(checkPosition))
        {
            return CoverLevel.None;
        }

        int currentElevation = LevelGrid.Instance.GetElevationAtGridPosition(currentPosition);
        int checkElevation = LevelGrid.Instance.GetElevationAtGridPosition(checkPosition);

        int elevationDifference = checkElevation - currentElevation;
        if (elevationDifference >= 4) return CoverLevel.Full;
        if (elevationDifference >= 2) return CoverLevel.Partial;
        return CoverLevel.None;
    }

    private GridPosition GetPositionInDirection(GridPosition currentPosition, Direction direction)
    {
        GridPosition adjacentPosition;
        switch (direction)
        {
            case Direction.North:
                adjacentPosition = new GridPosition(currentPosition.x, currentPosition.z + 1);
                break;
            case Direction.East:
                adjacentPosition = new GridPosition(currentPosition.x + 1, currentPosition.z);
                break;
            case Direction.South:
                adjacentPosition = new GridPosition(currentPosition.x, currentPosition.z - 1);
                break;
            case Direction.West:
                adjacentPosition = new GridPosition(currentPosition.x - 1, currentPosition.z);
                break;
            case Direction.NorthEast:
                adjacentPosition = new GridPosition(currentPosition.x + 1, currentPosition.z + 1);
                break;
            case Direction.SouthEast:
                adjacentPosition = new GridPosition(currentPosition.x + 1, currentPosition.z - 1);
                break;
            case Direction.SouthWest:
                adjacentPosition = new GridPosition(currentPosition.x - 1, currentPosition.z - 1);
                break;
            case Direction.NorthWest:
                adjacentPosition = new GridPosition(currentPosition.x - 1, currentPosition.z + 1);
                break;
            default:
                // If the direction is not recognized, return the current position
                adjacentPosition = currentPosition;
                break;
        }

        // Check if the adjacent position is within the circular grid
        if (!LevelGrid.Instance.IsValidGridPosition(adjacentPosition))
        {
            return currentPosition; // Return the current position if adjacent is invalid
        }

        return adjacentPosition;
    }

    public CoverLevel GetCoverAtPosition(GridPosition position, Direction attackDirection)
    {
        if (coverMap.TryGetValue(position, out var positionCover))
        {
            if (positionCover.TryGetValue(attackDirection, out CoverLevel coverLevel))
            {
                return coverLevel;
            }
        }

        return CoverLevel.None;
    }
}
