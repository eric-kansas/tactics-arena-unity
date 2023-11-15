using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class TerritorySystem : MonoBehaviour
{
    public static TerritorySystem Instance { get; private set; }
    public static Action<int, int> OnTerritoryOwnerChanged; // what zone to what team

    public UnityEngine.Color[] TerritoryColorList = new UnityEngine.Color[]
    {
        UnityEngine.Color.blue,
        UnityEngine.Color.red,
        UnityEngine.Color.green,
        UnityEngine.Color.yellow
    };

    private Dictionary<int, Rect> zones;
    private Dictionary<int, int> territoryOwners; // Maps zone ID to owner team ID

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one TerritorySystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        zones = new Dictionary<int, Rect>{
            { 0, new(0,0,3,3) },
            { 1, new(10,0,3,3) },
            { 2, new(10,10,3,3) },
        };

        territoryOwners = new Dictionary<int, int>();
        foreach (var zone in zones.Keys)
        {
            territoryOwners[zone] = -1;
        }
    }

    private void Start()
    {
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition;
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
    }

    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;

        UpdateTerritoryControlBasedOnUnitMovement(unit.GetGridPosition(), unit);
    }

    private void LevelGrid_OnAnyUnitMovedGridPosition(object sender, LevelGrid.OnAnyUnitMovedGridPositionEventArgs args)
    {
        UpdateTerritoryControlBasedOnUnitMovement(args.fromGridPosition, args.unit);
        UpdateTerritoryControlBasedOnUnitMovement(args.toGridPosition, args.unit);
    }

    private void UpdateTerritoryControlBasedOnUnitMovement(GridPosition gridPosition, Unit unit)
    {
        foreach (KeyValuePair<int, Rect> zoneEntry in zones)
        {
            if (zoneEntry.Value.Contains(gridPosition.ToVector2()))
            {
                UpdateTerritoryControl(zoneEntry.Key, unit);
                break;
            }
        }
    }

    private void UpdateTerritoryControl(int zoneID, Unit unit)
    {
        int newOwner = CalculateNewOwnerForTerritory(zoneID);

        if (territoryOwners[zoneID] != newOwner)
        {
            territoryOwners[zoneID] = newOwner;
            OnTerritoryOwnerChanged?.Invoke(zoneID, newOwner);
        }
    }

    private int CalculateNewOwnerForTerritory(int zoneID)
    {
        HashSet<int> teamsInZone = new HashSet<int>();
        Rect zoneRect = zones[zoneID];

        foreach (Unit tempUnit in UnitManager.Instance.GetUnitList())
        {
            if (zoneRect.Contains(tempUnit.GetGridPosition().ToVector2()))
            {
                teamsInZone.Add(tempUnit.GetTeam());
            }
        }

        if (teamsInZone.Count == 1)
        {
            // Only one team's units are in the zone
            return teamsInZone.First();
        }

        return -1; // Territory becomes neutral if empty or contested
    }

    public Dictionary<int, Rect> GetZones()
    {
        return zones;
    }

}
