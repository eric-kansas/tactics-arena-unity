using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{

    public static Pathfinding Instance { get; private set; }


    private const int MOVE_STRAIGHT_COST = 1;
    private const int ELEVATION_CHANGE_COST = 1; // or any other value based on your game design


    [SerializeField] private Transform gridDebugObjectPrefab;
    [SerializeField] private LayerMask obstaclesLayerMask;

    private int width;
    private int height;
    private int radius;
    private float cellSize;
    private GridSystem<PathNode> gridSystem;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one Pathfinding! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    public void Start()
    {
        Setup(LevelGrid.Instance.GetRadius(), LevelGrid.Instance.GetCellSize());
    }

    public void Setup(int radius, float cellSize)
    {
        this.radius = radius;
        this.cellSize = cellSize;
        int diameter = radius * 2 + 1;

        gridSystem = new GridSystem<PathNode>(radius, cellSize, Vector3.zero,
            (GridSystem<PathNode> g, GridPosition gridPosition) => new PathNode(gridPosition));

        // Update obstacle checking for circular grid
        for (int x = 0; x < diameter; x++)
        {
            for (int z = 0; z < diameter; z++)
            {
                GridPosition gridPosition = new GridPosition(x - radius, z - radius); // Adjust for circle center
                if (LevelGrid.Instance.IsValidGridPosition(gridPosition))
                {
                    Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                    float raycastOffsetDistance = 5f;
                    if (Physics.Raycast(
                        worldPosition + Vector3.down * raycastOffsetDistance,
                        Vector3.up,
                        raycastOffsetDistance * 2,
                        obstaclesLayerMask))
                    {
                        GetNode(x, z).SetIsWalkable(false);
                    }
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


    private int CalculatePerceivedRisk(PathNode fromNode, PathNode toNode)
    {
        (bool didTrigger, Unit enemey) = LevelGrid.Instance.TriggersOpportunityAttack(fromNode.GetGridPosition(), toNode.GetGridPosition());
        if (didTrigger)
        {
            return 100; // High perceived risk for opportunity attack moves
        }
        return 0; // No extra perceived risk
    }

    public List<GridPosition> FindPath(Team team, GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength, int maxMoveDistance)
    {
        List<PathNode> openList = new List<PathNode>();
        List<PathNode> closedList = new List<PathNode>();

        PathNode startNode = gridSystem.GetGridObject(startGridPosition);
        PathNode endNode = gridSystem.GetGridObject(endGridPosition);
        openList.Add(startNode);

        for (int x = 0; x < gridSystem.GetWidth(); x++)
        {
            for (int z = 0; z < gridSystem.GetHeight(); z++)
            {
                GridPosition gridPosition = new GridPosition(x, z);
                if (!LevelGrid.Instance.IsValidGridPosition(gridPosition))
                {
                    continue;
                }
                PathNode pathNode = gridSystem.GetGridObject(gridPosition);

                pathNode.SetGCost(int.MaxValue);
                pathNode.SetHCost(0);
                pathNode.SetPerceivedRisk(0);
                pathNode.CalculateFCost();
                pathNode.ResetCameFromPathNode();
                if (LevelGrid.Instance.GetUnitAtGridPosition(gridPosition))
                {
                    pathNode.SetIsWalkable(false);
                }
                else
                {
                    pathNode.SetIsWalkable(true);

                }
            }
        }

        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();

        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);

            if (currentNode == endNode)
            {
                // Reached final node
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (PathNode neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode))
                {
                    continue;
                }

                if (!neighbourNode.IsWalkable())
                {
                    closedList.Add(neighbourNode);
                    continue;
                }

                if (!FogOfWarSystem.Instance.IsVisible(team, neighbourNode.GetGridPosition()))
                {
                    closedList.Add(neighbourNode);
                    continue;
                }
                int neighbourElevation = LevelGrid.Instance.GetElevationAtGridPosition(neighbourNode.GetGridPosition());
                TerrainType neighbourTerrainType = LevelGrid.Instance.GetTerrainTypeAtGridPosition(neighbourNode.GetGridPosition());


                // Calculate costs using the GridObject's data
                int terrainMovementCost = GetTerrainMovementCost(neighbourTerrainType);
                int elevationChangeCost = GetElevationChangeCost(currentNode, neighbourElevation);

                int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighbourNode.GetGridPosition()) + terrainMovementCost + elevationChangeCost;
                if (tentativeGCost <= maxMoveDistance)
                {
                    int perceivedRisk = CalculatePerceivedRisk(currentNode, neighbourNode);
                    int totalCost = tentativeGCost + perceivedRisk; // Combine actual cost and perceived risk

                    if (totalCost < neighbourNode.GetGCost() + neighbourNode.GetPerceivedRisk())
                    {
                        neighbourNode.SetCameFromPathNode(currentNode);
                        neighbourNode.SetGCost(tentativeGCost); // Actual movement cost
                        neighbourNode.SetPerceivedRisk(perceivedRisk); // Set perceived risk
                        neighbourNode.SetHCost(CalculateDistance(neighbourNode.GetGridPosition(), endGridPosition));
                        neighbourNode.CalculateFCost();

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }
        }

        // No path found
        pathLength = 0;
        return null;
    }

    public int CalculateDistance(GridPosition gridPositionA, GridPosition gridPositionB)
    {
        GridPosition gridPositionDistance = gridPositionA - gridPositionB;
        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int zDistance = Mathf.Abs(gridPositionDistance.z);
        return MOVE_STRAIGHT_COST * (xDistance + zDistance);
    }

    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostPathNode = pathNodeList[0];
        for (int i = 0; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost())
            {
                lowestFCostPathNode = pathNodeList[i];
            }
        }
        return lowestFCostPathNode;
    }

    private int GetTerrainMovementCost(TerrainType terrainType)
    {
        return 0;
        switch (terrainType)
        {
            case TerrainType.Forest:
                return 15; // Higher cost for forest
            case TerrainType.Mountain:
                return 20; // Even higher cost for mountains
                           // ... other cases
            default:
                return 10; // Default cost for plain terrain
        }
    }

    private int GetElevationChangeCost(PathNode currentNode, int targetElevation)
    {
        int currentElevation = LevelGrid.Instance.GetElevationAtGridPosition(currentNode.GetGridPosition());
        if (targetElevation - currentElevation <= 0)
        {
            return 0;
        }
        return Mathf.Abs(targetElevation - currentElevation) * ELEVATION_CHANGE_COST;
    }

    private PathNode GetNode(int x, int z)
    {
        return gridSystem.GetGridObject(new GridPosition(x, z));
    }
    private List<PathNode> GetNeighbourList(PathNode currentNode)
    {
        List<PathNode> neighbourList = new List<PathNode>();

        GridPosition gridPosition = currentNode.GetGridPosition();
        AddNeighbour(neighbourList, gridPosition.x - 1, gridPosition.z); // Left
        AddNeighbour(neighbourList, gridPosition.x + 1, gridPosition.z); // Right
        AddNeighbour(neighbourList, gridPosition.x, gridPosition.z - 1); // Down
        AddNeighbour(neighbourList, gridPosition.x, gridPosition.z + 1); // Up

        return neighbourList;
    }

    private void AddNeighbour(List<PathNode> neighbourList, int x, int z)
    {
        if (IsWithinCircle(radius, x, z))
        {
            neighbourList.Add(GetNode(x, z));
        }
    }

    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathNodeList = new List<PathNode>();
        pathNodeList.Add(endNode);
        PathNode currentNode = endNode;
        while (currentNode.GetCameFromPathNode() != null)
        {
            pathNodeList.Add(currentNode.GetCameFromPathNode());
            currentNode = currentNode.GetCameFromPathNode();
        }

        pathNodeList.Reverse();

        List<GridPosition> gridPositionList = new List<GridPosition>();
        foreach (PathNode pathNode in pathNodeList)
        {
            gridPositionList.Add(pathNode.GetGridPosition());
        }

        return gridPositionList;
    }

    public void SetIsWalkableGridPosition(GridPosition gridPosition, bool isWalkable)
    {
        gridSystem.GetGridObject(gridPosition).SetIsWalkable(isWalkable);
    }

    public bool IsWalkableGridPosition(GridPosition gridPosition)
    {
        return gridSystem.GetGridObject(gridPosition).IsWalkable();
    }

    public bool HasPath(Team team, GridPosition startGridPosition, GridPosition endGridPosition, int maxMoveDistance)
    {
        return FindPath(team, startGridPosition, endGridPosition, out int pathLength, maxMoveDistance) != null;
    }

    public int GetPathLength(Team team, GridPosition startGridPosition, GridPosition endGridPosition, int maxMoveDistance)
    {
        Debug.Log("find path!");
        List<GridPosition> path = FindPath(team, startGridPosition, endGridPosition, out int pathLength, maxMoveDistance);
        return pathLength;
    }

}
