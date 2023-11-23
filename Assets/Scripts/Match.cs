using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SpawnZone
{
    public Rect zoneRect; // Defines the area of the spawn zone
    // Add other properties if needed, like team ID or specific spawn points within the zone
}

public class Match : MonoBehaviour
{
    public static Match Instance { get; private set; }
    [SerializeField] private GameObject unitPrefab; // Reference to the Unit prefab

    [SerializeField] private Team homeTeam;
    [SerializeField] private Team awayTeam;
    [SerializeField] private List<SpawnZone> homeTeamSpawnZones;
    [SerializeField] private List<SpawnZone> awayTeamSpawnZones;

    // You can define the spawn zones here or load them from Scriptable Objects
    private Dictionary<Team, List<Zone>> teamSpawnZones;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one Match! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        teamSpawnZones = new Dictionary<Team, List<Zone>>();
    }

    private void Start()
    {

        SetupTeam(homeTeam);
        SetupTeam(awayTeam);

        PopulateTeamSpawnZones(homeTeam, homeTeamSpawnZones);
        PopulateTeamSpawnZones(awayTeam, awayTeamSpawnZones);
    }

    private void PopulateTeamSpawnZones(Team team, List<SpawnZone> spawnZones)
    {
        List<Zone> teamZonePositions = new List<Zone>();
        foreach (SpawnZone zone in spawnZones)
        {
            teamZonePositions.Add(new Zone(zone.zoneRect));
        }

        teamSpawnZones[team] = teamZonePositions;
    }

    private void SetupTeam(Team team)
    {
        ReserveGridSystem.Instance.RegisterTeam(team);
        foreach (Player player in team.GetPlayers())
        {
            // Instantiate a unit for each player
            GameObject unitGO = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity);
            unitGO.SetActive(false);
            Unit unit = unitGO.GetComponent<Unit>();

            // Setup the unit with the player's abilities
            unit.Setup(team, player);
            unit.SetInArena(false);
            GridPosition gridPosition = ReserveGridSystem.Instance.AddUnit(team, unit);
            unitGO.transform.position = ReserveGridSystem.Instance.GetWorldPosition(team, gridPosition);
            unitGO.SetActive(true);

            // Additional unit setup code...
        }
    }

    internal IEnumerable<Team> GetAllTeams()
    {
        // Create a list or array containing all teams
        List<Team> teams = new List<Team>();

        if (homeTeam != null)
        {
            teams.Add(homeTeam);
        }

        if (awayTeam != null)
        {
            teams.Add(awayTeam);
        }

        return teams;
    }

    internal Team GetClientTeam()
    {
        return homeTeam;
    }

    internal Team GetAwayTeam()
    {
        return awayTeam;
    }

    internal Color GetTeamColor(Team team)
    {
        if (team == homeTeam)
        {
            return Color.red;
        }
        else
        {
            return Color.blue;
        }
    }

    public static List<GridPosition> GetGridPositionsFromRect(Rect rect)
    {
        List<GridPosition> gridPositions = new List<GridPosition>();

        // Iterate over the rectangle area
        for (int x = Mathf.FloorToInt(rect.xMin); x < Mathf.CeilToInt(rect.xMax); x++)
        {
            for (int z = Mathf.FloorToInt(rect.yMin); z < Mathf.CeilToInt(rect.yMax); z++)
            {
                gridPositions.Add(new GridPosition(x, z));
            }
        }

        return gridPositions;
    }

    public List<Zone> GetSpawnZonesForTeam(Team team)
    {
        if (teamSpawnZones.TryGetValue(team, out var spawnPositions))
        {
            return spawnPositions;
        }
        return new List<Zone>(); // Return an empty list if no spawn zones are found
    }
}