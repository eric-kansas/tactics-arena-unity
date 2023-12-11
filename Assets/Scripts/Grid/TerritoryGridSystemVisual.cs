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
        RenderZones();

        FogOfWarSystem.OnTeamVisibilityChanged += FogOfWarSystem_OnTeamVisibilityChanged;
    }

    private void RenderZones()
    {
        Dictionary<int, Rect> zones = TerritorySystem.Instance.GetZones();
        int radius = LevelGrid.Instance.GetRadius(); // Assuming LevelGrid provides radius
        int diameter = radius * 2 + 1;

        foreach (KeyValuePair<int, Rect> zoneEntry in zones)
        {
            Rect rect = zoneEntry.Value;

            // Ensure gridSystemVisualSingleArray is initialized with the correct dimensions
            gridSystemVisualSingleArray = new GridSystemVisualSingle[diameter, diameter];

            for (int x = 0; x < diameter; x++)
            {
                for (int z = 0; z < diameter; z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);

                    // Check if the position is valid within both the zone and the circular grid
                    if (rect.Contains(new Vector2(gridPosition.x, gridPosition.z)) &&
                        LevelGrid.Instance.IsValidGridPosition(gridPosition))
                    {
                        Vector3 pos = FogOfWarSystem.Instance.GetPerceivedWorldPosition(gridPosition);
                        Transform gridSystemVisualSingleTransform = Instantiate(gridSystemVisualSinglePrefab, pos, Quaternion.identity);
                        gridSystemVisualSingleArray[x, z] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
                        gridSystemVisualSingleArray[x, z].Show(GetGridVisualTypeMaterial(zoneEntry.Key));
                    }
                }
            }
        }
    }

    private void FogOfWarSystem_OnTeamVisibilityChanged(Team team)
    {
        RenderZones();
    }

    private Material GetGridVisualTypeMaterial(int zone)
    {
        return gridVisualTypeMaterialList[zone].material;

        // foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        // {
        //     if (gridVisualTypeMaterial.gridVisualType == gridVisualType)
        //     {
        //         return gridVisualTypeMaterial.material;
        //     }
        // }

        // Debug.LogError("Could not find GridVisualTypeMaterial for GridVisualType " + gridVisualType);
        // return null;
    }

}