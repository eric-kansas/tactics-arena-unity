using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryGridSystemVisual : MonoBehaviour
{

    public static TerritoryGridSystemVisual Instance { get; private set; }

    [SerializeField] private Transform gridSystemVisualSinglePrefab;
    [SerializeField] private List<GridVisualTypeMaterial> gridVisualTypeMaterialList;

    private GridSystemVisualSingle[,] gridSystemVisualSingleArray;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one TerritoryGridSystemVisual! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Dictionary<int, Rect> zones = TerritorySystem.Instance.GetZones();

        foreach (KeyValuePair<int, Rect> zoneEntry in zones)
        {
            Rect rect = zoneEntry.Value;
            gridSystemVisualSingleArray = new GridSystemVisualSingle[
                (int)rect.width,
                (int)rect.height
            ];

            for (int x = 0; x < (int)rect.width; x++)
            {
                for (int z = 0; z < (int)rect.height; z++)
                {
                    GridPosition gridPosition = new GridPosition((int)rect.x + x, (int)rect.y + z);

                    Transform gridSystemVisualSingleTransform = Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);

                    gridSystemVisualSingleArray[x, z] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();

                    gridSystemVisualSingleArray[x, z].Show(GetGridVisualTypeMaterial(GridVisualType.Blue));
                }
            }


        }
    }

    private Material GetGridVisualTypeMaterial(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        {
            if (gridVisualTypeMaterial.gridVisualType == gridVisualType)
            {
                return gridVisualTypeMaterial.material;
            }
        }

        Debug.LogError("Could not find GridVisualTypeMaterial for GridVisualType " + gridVisualType);
        return null;
    }

}