using UnityEngine;
using System.Collections.Generic;

public class TraveralPathVisual : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private MoveCostDisplayer moveCostDisplayer;
    private List<GridPosition> path;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        moveCostDisplayer = GetComponent<MoveCostDisplayer>();
    }

    public void Setup(List<GridPosition> path)
    {
        this.path = path;
        UpdatePathPoints();
    }

    public void ShowPath(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.enabled = true;
    }

    public void HidePath()
    {
        lineRenderer.enabled = false;
    }

    public void Clear()
    {
        moveCostDisplayer.DestroyMoveCosts();
    }

    private void UpdatePathPoints()
    {
        float yOffset = 0.1f; // Define the Y-axis offset
        int cost = 0;

        GridPosition? lastlocation = new GridPosition?();
        List<Vector3> points = new List<Vector3>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 startPos = LevelGrid.Instance.GetWorldPosition(path[i]) + new Vector3(0, yOffset, 0);
            Vector3 endPos = LevelGrid.Instance.GetWorldPosition(path[i + 1]) + new Vector3(0, yOffset, 0);

            // Add start position
            points.Add(startPos);

            // If there is a height difference, break the movement into smaller steps
            if (startPos.y != endPos.y)
            {
                if (startPos.y > endPos.y)
                {
                    Vector3 modifiedEndPos = endPos;
                    modifiedEndPos.y = startPos.y;
                    // Horizontal step - 1/4 into the next position
                    Vector3 horizontalStep = Vector3.MoveTowards(startPos, modifiedEndPos, LevelGrid.Instance.GetCellSize() * 0.75f);
                    points.Add(horizontalStep);

                    // Vertical step - match the height of the next position
                    Vector3 verticalStep = new Vector3(horizontalStep.x, endPos.y, horizontalStep.z);
                    points.Add(verticalStep);
                }
                else
                {
                    // Horizontal step - 1/4 towards the next position
                    Vector3 horizontalStep = Vector3.MoveTowards(startPos, endPos, LevelGrid.Instance.GetCellSize() / 4);
                    horizontalStep.y = startPos.y; // Keep the same height as the start position
                    points.Add(horizontalStep);

                    // Vertical step - match the height of the next position
                    Vector3 verticalStep = new Vector3(horizontalStep.x, endPos.y, horizontalStep.z);
                    points.Add(verticalStep);
                }

            }

            // Add end position
            points.Add(endPos);

            // display cost
            if (lastlocation.HasValue)
            {
                Pathfinding.Instance.FindPath(lastlocation.Value, path[i], out cost);
                moveCostDisplayer.CreateMoveCost(transform, LevelGrid.Instance.GetWorldPosition(path[i]), cost);
            }

            lastlocation = path[i];
        }

        // add for last stop
        Pathfinding.Instance.FindPath(lastlocation.Value, path[path.Count - 1], out cost);
        moveCostDisplayer.CreateMoveCost(transform, LevelGrid.Instance.GetWorldPosition(path[path.Count - 1]), cost);

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }


}
