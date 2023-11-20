using System;
using System.Collections.Generic;
using UnityEngine;

public class Match : MonoBehaviour
{
    public static Match Instance { get; private set; }

    [SerializeField] private Team homeTeam;
    [SerializeField] private Team awayTeam;
    [SerializeField] private GameObject unitPrefab; // Reference to the Unit prefab

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitActionSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SetupTeam(homeTeam);
        SetupTeam(awayTeam);
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
}