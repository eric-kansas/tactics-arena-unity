using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ZoneInfo
{
    public Rect rect;
    public int sectionID;
    public Card card;

    public ZoneInfo(Rect rect, int sectionID, Card card)
    {
        this.rect = rect;
        this.sectionID = sectionID;
        this.card = card;
    }
}


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

public class TerritorySystem : MonoBehaviour
{
    public static TerritorySystem Instance { get; private set; }
    public static Action<int, Team> OnTerritoryOwnerChanged; // what zone to what team
    public static Action<Team, int> OnTerritoryScoreChanged; // what team to how many points

    private Dictionary<Team, TerritoryScore> territoryScores;

    private Dictionary<int, ZoneInfo> zones;
    private Dictionary<int, Team> territoryOwners; // Maps zone ID to owner team ID

    private TerritoryDeck territoryDeck;
    int totalGridPositionsPerZone = 9; // Example value

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one TerritorySystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        territoryDeck = new TerritoryDeck(); 
        
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition;
        Unit.OnAnyUnitInitialized += Unit_OnAnyUnitSpawned;

        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;

        InitializeTerritoryZones();
        territoryScores = new Dictionary<Team, TerritoryScore>{
            { Match.Instance.GetClientTeam(), new(0, 10) },
            { Match.Instance.GetAwayTeam(), new(0, 10) },
        };

        territoryOwners = new Dictionary<int, Team>();
        foreach (var zone in zones.Keys)
        {
            territoryOwners[zone] = null;
        }
    }

    private void InitializeTerritoryZones()
    {
        zones = new Dictionary<int, ZoneInfo>();

        // Generate and zones for the sectors
        Dictionary<int, List<Rect>> sectorToZones = GenerateZones();

        // Loop through sector to zones and their lists and add to zones
        int zoneID = 0;
        foreach (var sectorPair in sectorToZones)
        {
            foreach (var rect in sectorPair.Value)
            {
                zones.Add(zoneID, new ZoneInfo(rect, sectorPair.Key, territoryDeck.DrawCard()));
                zoneID++;
            }
        }
    }

    private Dictionary<int, List<Rect>> GenerateZones()
    {
        Dictionary<int, List<Rect>> gridPositions = new Dictionary<int, List<Rect>>(); // dictionary of sector to territory zones 
        for (int i = 0; i < LevelGrid.Instance.GetNumberOfSections(); i++)
        {
            gridPositions[i] = GenerateZoneGridPositions(i);
        }
        return gridPositions;
    }

    private List<Rect> GenerateZoneGridPositions(int zone)
    {
        List<Rect> zonePositions = new List<Rect>();
        List<GridPosition> potentialPositions = GetPotentialPositionsForSector(zone);
        if (potentialPositions.Count > 0)
        {
            for (int i = 0; i < totalGridPositionsPerZone; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, potentialPositions.Count);
                GridPosition chosenPosition = potentialPositions[randomIndex];
                Rect rect = new Rect(chosenPosition.x, chosenPosition.z, 1, 1); // for now -- just point as rect
                zonePositions.Add(rect);
            }
        }
        return zonePositions;
    }

    private List<GridPosition> GetPotentialPositionsForSector(int sectorID)
    {
        List<GridPosition> potentialPositions = new List<GridPosition>();

        for (int x = 0; x < 2 * LevelGrid.Instance.GetRadius() + 1; x++)
        {
            for (int z = 0; z < 2 * LevelGrid.Instance.GetRadius() + 1; z++)
            {
                GridPosition position = new GridPosition(x, z);
                if (LevelGrid.Instance.IsWithinCircle(LevelGrid.Instance.GetRadius(), x, z) &&
                    LevelGrid.Instance.GetSector(position) == sectorID)
                {
                    potentialPositions.Add(position);
                }
            }
        }

        return potentialPositions;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        // Here we will calculate and award points based on territory control

        // Assuming you have a method or system to award points
        foreach (KeyValuePair<int, Team> territoryEntry in territoryOwners)
        {
            int zoneID = territoryEntry.Key;
            Team owningTeamID = territoryEntry.Value;

            if (owningTeamID != null) // Check if the territory is not neutral
            {
                // Award points to the team that controls this territory
                AwardPointsToTeam(owningTeamID, CalculatePointsForTerritory(zoneID));
            }
        }
    }

    private void AwardPointsToTeam(Team team, int points)
    {
        // Implement logic to award points to the given team.
        // This might involve updating a score variable, notifying other systems, etc.
        Debug.Log($"Team {team.name} awarded {points} points for controlling territory.");
        TerritoryScore score = territoryScores[team];
        score.amount += points;
        territoryScores[team] = score;
        OnTerritoryScoreChanged?.Invoke(team, points);
    }

    private int CalculatePointsForTerritory(int zoneID)
    {
        // Define how many points a territory is worth. 
        // This could be a fixed value, or vary based on the territory's properties or game dynamics.
        return 1; // Example: Each territory is worth 1 point per turn
    }

    private void Unit_OnAnyUnitSpawned(Unit unit)
    {
        UpdateTerritoryControlBasedOnUnitMovement(unit.GetGridPosition(), unit);
    }

    private void LevelGrid_OnAnyUnitMovedGridPosition(LevelGrid.OnAnyUnitMovedGridPositionEventArgs args)
    {
        UpdateTerritoryControlBasedOnUnitMovement(args.fromGridPosition, args.unit);
        UpdateTerritoryControlBasedOnUnitMovement(args.toGridPosition, args.unit);
    }

    private void UpdateTerritoryControlBasedOnUnitMovement(GridPosition gridPosition, Unit unit)
    {
        foreach (KeyValuePair<int, ZoneInfo> zoneEntry in zones)
        {
            if (zoneEntry.Value.rect.Contains(gridPosition.ToVector2()))
            {
                UpdateTerritoryControl(zoneEntry.Key, unit);
                break;
            }
        }
    }

    private void UpdateTerritoryControl(int zoneID, Unit unit)
    {
        Team newOwner = CalculateNewOwnerForTerritory(zoneID);

        if (territoryOwners[zoneID] != newOwner)
        {
            territoryOwners[zoneID] = newOwner;
            OnTerritoryOwnerChanged?.Invoke(zoneID, newOwner);
        }
    }

    private Team CalculateNewOwnerForTerritory(int zoneID)
    {
        HashSet<Team> teamsInZone = new HashSet<Team>();
        Rect zoneRect = zones[zoneID].rect;

        foreach (Unit tempUnit in UnitManager.Instance.GetArenaUnitList())
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

        return null; // Territory becomes neutral if empty or contested
    }

    public Dictionary<int, ZoneInfo> GetZones()
    {
        return zones;
    }

    public float GetTeamTerritoryScoreNormalized(Team team)
    {
        return territoryScores[team].GetScoreNormalized();
    }

    public Team GetTerritoryOwner(int zoneID) {
        return territoryOwners[zoneID];
    }

}
