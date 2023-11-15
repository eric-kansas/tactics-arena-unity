using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{

    public static TurnSystem Instance { get; private set; }

    public static Color[] TeamColorList = new Color[]
    {
        Color.blue,
        Color.red,
        Color.green,
        Color.yellow
    };

    public event EventHandler OnTurnChanged;


    private int turnNumber = 1;
    private int currentTeamIndex = 0;
    private int totalTeams = 2;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one TurnSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetTotalTeams(int numberOfTeams)
    {
        if (numberOfTeams > 0)
        {
            totalTeams = numberOfTeams;
        }
    }


    public void NextTurn()
    {
        turnNumber++;
        currentTeamIndex = (currentTeamIndex + 1) % totalTeams;

        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetTurnNumber()
    {
        return turnNumber;
    }

    public int GetCurrentTeam()
    {
        return currentTeamIndex;
    }

    internal bool IsPlayerTurn()
    {
        return currentTeamIndex == 0;
    }
}
