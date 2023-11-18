using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class TerritorySystem : MonoBehaviour
{
    [Serializable]
    public struct TerritoryScore
    {
        public int amount;
        public int max;

        public TerritoryScore(int v1, int v2)
        {
            amount = v1;
            max = v2;
        }

        public float GetScoreNormalized()
        {
            return (float)amount / max;
        }
    }

    public static TerritorySystem Instance { get; private set; }
    public static Action<int, int> OnTerritoryOwnerChanged; // what zone to what team
    public static Action<int, int> OnTerritoryScoreChanged; // what team to how many points

    private Dictionary<int, TerritoryScore> territoryScores;

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
            { 0, new(2,2,3,3) },
            { 1, new(10,2,3,3) },
            { 2, new(2,10,3,3) },
            { 3, new(10,10,3,3) },
            { 4 , new(6,6,3,3) },
        };

        territoryScores = new Dictionary<int, TerritoryScore>{
            { 0, new(0, 10) },
            { 1, new(0, 10) },
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

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        // Here we will calculate and award points based on territory control

        // Assuming you have a method or system to award points
        foreach (KeyValuePair<int, int> territoryEntry in territoryOwners)
        {
            int zoneID = territoryEntry.Key;
            int owningTeamID = territoryEntry.Value;

            if (owningTeamID != -1) // Check if the territory is not neutral
            {
                // Award points to the team that controls this territory
                AwardPointsToTeam(owningTeamID, CalculatePointsForTerritory(zoneID));
            }
        }
    }

    private void AwardPointsToTeam(int teamID, int points)
    {
        // Implement logic to award points to the given team.
        // This might involve updating a score variable, notifying other systems, etc.
        Debug.Log($"Team {teamID} awarded {points} points for controlling territory.");
        TerritoryScore score = territoryScores[teamID];
        score.amount += points;
        territoryScores[teamID] = score;
        OnTerritoryScoreChanged?.Invoke(teamID, points);
    }

    private int CalculatePointsForTerritory(int zoneID)
    {
        // Define how many points a territory is worth. 
        // This could be a fixed value, or vary based on the territory's properties or game dynamics.
        return 1; // Example: Each territory is worth 1 point per turn
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

    public float GetTeamTerritoryScoreNormalized(int index)
    {
        return territoryScores[index].GetScoreNormalized();
    }


}
