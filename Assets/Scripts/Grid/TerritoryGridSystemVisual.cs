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

        FogOfWarSystem.OnTeamVisbilityChanged += FogOfWarSystem_OnTeamVisbilityChanged;
    }

    private void RenderZones()
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

                    Vector3 pos = FogOfWarSystem.Instance.GetPerceivedWorldPosition(gridPosition);

                    Transform gridSystemVisualSingleTransform = Instantiate(gridSystemVisualSinglePrefab, pos, Quaternion.identity);

                    gridSystemVisualSingleArray[x, z] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();

                    gridSystemVisualSingleArray[x, z].Show(GetGridVisualTypeMaterial(zoneEntry.Key));
                }
            }
        }
    }

    private void FogOfWarSystem_OnTeamVisbilityChanged(Team team)
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