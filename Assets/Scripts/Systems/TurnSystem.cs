using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    public static TurnSystem Instance { get; private set; }

    public static Color[] TeamColorList = new Color[]
    {
        Color.blue,
        Color.red,
    };

    public event EventHandler OnTurnChanged;

    private int turnNumber = 1;
    private List<Team> teams;
    private Team currentTeam;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one TurnSystem!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        teams = new List<Team>();
    }

    private void Start()
    {
        foreach (Team team in Match.Instance.GetAllTeams())
        {
            teams.Add(team);
        }

        currentTeam = Match.Instance.GetClientTeam();
    }


    public void NextTurn()
    {
        turnNumber++;
        int currentTeamIndex = teams.IndexOf(currentTeam);
        currentTeam = teams[(currentTeamIndex + 1) % teams.Count];

        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetTurnNumber()
    {
        return turnNumber;
    }

    public Team GetCurrentTeam()
    {
        return currentTeam;
    }

    public bool IsPlayerTurn()
    {
        return currentTeam == Match.Instance.GetClientTeam();
    }
}
