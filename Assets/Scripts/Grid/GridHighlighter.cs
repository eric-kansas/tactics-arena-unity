using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GridHighlighter : MonoBehaviour
{
    [SerializeField] private GameObject highlightPrefab;
    [SerializeField] private GameObject gridHighlightWorldUI;

    private GameObject currentHighlight;
    private ZoneBorderVisualRect zoneBorderVisual;
    private float cellSize;

    void Awake()
    {
        if (highlightPrefab != null)
        {
            currentHighlight = Instantiate(highlightPrefab, Vector3.zero, Quaternion.identity);
            zoneBorderVisual = currentHighlight.GetComponent<ZoneBorderVisualRect>();
        }
        else
        {
            Debug.LogError("Highlight prefab is not assigned!");
        }
        gridHighlightWorldUI = Instantiate(gridHighlightWorldUI, Vector3.zero, Quaternion.identity);

    }

    void Update()
    {
        GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());
        if (!LevelGrid.Instance.IsValidGridPosition(mouseGridPosition))
        {
            zoneBorderVisual.HideBorder();
            gridHighlightWorldUI.SetActive(false);
            return;
        }
        Vector3 textPos = FogOfWarSystem.Instance.GetPerceivedWorldPosition(mouseGridPosition);
        textPos.y += 1.5f;
        gridHighlightWorldUI.transform.position = textPos;
        gridHighlightWorldUI.SetActive(true);
        zoneBorderVisual.Setup(new Zone(new List<GridPosition>() { mouseGridPosition }), 0.05f);
        zoneBorderVisual.ShowBorder(Color.green);
    }
}
