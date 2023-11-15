using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerritorySystem : MonoBehaviour
{
    public static TerritorySystem Instance { get; private set; }

    public static Action<int> OnTerritoryOwnerChanged;

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
            { 1, new(10,10,3,3) },
        };

        territoryOwners = new Dictionary<int, int>();
        foreach (var zone in zones.Keys)
        {
            territoryOwners[zone] = -1; // -1 could signify uncontrolled or neutral territory
        }
    }

    private void Start()
    {
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition;

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

    // private void LevelGrid_OnAnyUnitMovedGridPosition(object sender, LevelGrid.OnAnyUnitMovedGridPositionEventArgs args)
    // {

    //     int zone = -1;
    //     // check if unit is in a territory
    //     foreach (KeyValuePair<int, Rect> zoneEntry in zones)
    //     {
    //         int zoneID = zoneEntry.Key;
    //         Rect zoneRect = zoneEntry.Value;

    //         if (zoneRect.Contains(args.toGridPosition.ToVector2()))
    //         {
    //             zone = zoneID;
    //             break;
    //         }
    //     }

    //     if (zone == -1)
    //     {
    //         // no zone found
    //         return;
    //     }

    //     UpdateTerritoryControl(zone, args.unit);
    // }

    private void UpdateTerritoryControl(int zoneID, Unit unit)
    {
        int newOwner = CalculateNewOwnerForTerritory(zoneID);
        if (territoryOwners[zoneID] != newOwner)
        {
            territoryOwners[zoneID] = newOwner;
            Debug.Log("[TERRITORY CHANGE] zone:" + zoneID + " -- " + newOwner);
            OnTerritoryOwnerChanged?.Invoke(zoneID);
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

    // private int CalculateNewOwnerForTerritory(int zoneID, Unit unit)
    // {

    //     HashSet<int> teamsInZone = new HashSet<int>();
    //     Rect zoneRect = zones[zoneID];
    //     int currentOwner = territoryOwners[zoneID]; // Assuming you track current owners in territoryOwners

    //     foreach (Unit tempUnit in UnitManager.Instance.GetUnitList())
    //     {
    //         if (zoneRect.Contains(tempUnit.GetGridPosition().ToVector2()))
    //         {
    //             teamsInZone.Add(tempUnit.GetTeam());
    //         }
    //     }

    //     if (teamsInZone.Count == 0)
    //     {
    //         // No units in the zone
    //         return currentOwner; // Might remain neutral or with current owner
    //     }
    //     else if (teamsInZone.Count == 1)
    //     {
    //         // Only one team's units are in the zone
    //         int teamInZone = teamsInZone.First();
    //         if (teamInZone != currentOwner)
    //         {
    //             // If different from current owner, either change ownership or make neutral
    //             return teamInZone != currentOwner ? teamInZone : -1;
    //         }
    //     }

    //     // Multiple teams in the zone or a different team has entered
    //     return -1; // Neutral

    // }
}
