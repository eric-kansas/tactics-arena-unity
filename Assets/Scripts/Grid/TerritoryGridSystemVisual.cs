using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryGridSystemVisual : MonoBehaviour
{
    public enum TerritoryGridVisualType
    {
        CrimsonRed,
        DeepSkyBlue,
        ForestGreen,
        Gold,
        DarkViolet,
        OrangeRed,
        RoyalBlue,
        SaddleBrown,
        SeaGreen,
    }

    public static TerritoryGridSystemVisual Instance { get; private set; }

    [SerializeField] private Transform gridSystemVisualSinglePrefab;

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
        MakeZones();

        FogOfWarSystem.OnTeamVisibilityChanged += FogOfWarSystem_OnTeamVisibilityChanged;
        TerritorySystem.OnTerritoryOwnerChanged += TerritorySystem_OnTerritoryOwnerChanged;
    }

    private void FogOfWarSystem_OnTeamVisibilityChanged(Team team)
    {
        //ClearZones();
        UpdateZones();
    }

    private void TerritorySystem_OnTerritoryOwnerChanged(int zoneID, Team team)
    {
        //ClearZones();
        UpdateZones();
    }

    private void ClearZones()
    {
        foreach (GridSystemVisualSingle single in gridSystemVisualSingleArray) 
        {
            if (single != null) {
                Destroy(single.gameObject);
            }
        }
    }

    private void MakeZones()
    {
        Dictionary<int, ZoneInfo> zones = TerritorySystem.Instance.GetZones();
        int radius = LevelGrid.Instance.GetRadius(); // Assuming LevelGrid provides radius
        int diameter = radius * 2 + 1;

        // Ensure gridSystemVisualSingleArray is initialized with the correct dimensions
        gridSystemVisualSingleArray = new GridSystemVisualSingle[diameter, diameter];

        foreach (KeyValuePair<int, ZoneInfo> zoneEntry in zones)
        {
            Rect rect = zoneEntry.Value.rect;

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
    
    private void UpdateZones()
    {
        Dictionary<int, ZoneInfo> zones = TerritorySystem.Instance.GetZones();
        int radius = LevelGrid.Instance.GetRadius(); // Assuming LevelGrid provides radius
        int diameter = radius * 2 + 1;

        foreach (KeyValuePair<int, ZoneInfo> zoneEntry in zones) 
        {
            Rect rect = zoneEntry.Value.rect;

            int xStart = (int)rect.x;
            int yStart = (int)rect.y;

            for (int x = xStart; x < xStart + rect.width; x++)
            {
                for (int z = yStart; z < yStart + rect.height; z++)
                {
                    GridPosition gridPosition = new GridPosition(x, z);
                    if (LevelGrid.Instance.IsValidGridPosition(gridPosition)) 
                    {
                        GridSystemVisualSingle gridVisual = gridSystemVisualSingleArray[x, z];
                        gridVisual.transform.position = FogOfWarSystem.Instance.GetPerceivedWorldPosition(gridPosition);
                        gridVisual.Show(GetGridVisualTypeMaterial(zoneEntry.Key));
                    }

                }
            }

        }
    }


    private Material GetGridVisualTypeMaterial(int zone)
    {
        Team owner = TerritorySystem.Instance.GetTerritoryOwner(zone);
        if (owner != null) {
            return owner.GetTeamColorAsMaterial();
        }

        return null;
    }

}