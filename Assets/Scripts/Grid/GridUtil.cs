using System;
using System.Collections;
using System.Collections.Generic;

public class GridUtil
{
    public static List<GridPosition> GetAllNeighborPositions(GridPosition gridPosition)
    {
        List<GridPosition> neighborPositions = new List<GridPosition>();

        // Add all 8 possible neighbors (including diagonals)
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0) continue; // Skip the current position

                neighborPositions.Add(new GridPosition(gridPosition.x + x, gridPosition.z + z));
            }
        }

        return neighborPositions;
    }

    public static bool IsAdjacent(GridPosition pos1, GridPosition pos2)
    {
        // Check if pos2 is within 1 grid position away from pos1 in any direction
        return Math.Abs(pos1.x - pos2.x) <= 1 && Math.Abs(pos1.z - pos2.z) <= 1;
    }
}
