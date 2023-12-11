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
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                GridPosition position = new GridPosition(x, z);
                UpdateCoverForPosition(position);
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
        switch (direction)
        {
            case Direction.North:
                return new GridPosition(currentPosition.x, currentPosition.z + 1);

            case Direction.East:
                return new GridPosition(currentPosition.x + 1, currentPosition.z);

            case Direction.South:
                return new GridPosition(currentPosition.x, currentPosition.z - 1);

            case Direction.West:
                return new GridPosition(currentPosition.x - 1, currentPosition.z);

            case Direction.NorthEast:
                return new GridPosition(currentPosition.x + 1, currentPosition.z + 1);

            case Direction.SouthEast:
                return new GridPosition(currentPosition.x + 1, currentPosition.z - 1);

            case Direction.SouthWest:
                return new GridPosition(currentPosition.x - 1, currentPosition.z - 1);

            case Direction.NorthWest:
                return new GridPosition(currentPosition.x - 1, currentPosition.z + 1);

            default:
                // If the direction is not recognized, return the current position
                return currentPosition;
        }
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
