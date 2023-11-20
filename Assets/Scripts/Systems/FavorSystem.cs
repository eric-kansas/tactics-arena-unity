using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FavorSystem : MonoBehaviour
{

    [Serializable]
    public struct TeamFavor
    {
        public int amount;
        public int max;

        public TeamFavor(int v1, int v2)
        {
            amount = v1;
            max = v2;
        }

        public float GetFavorNormalized()
        {
            return (float)amount / max;
        }
    }

    public static FavorSystem Instance { get; private set; }
    public static Action<Team> OnAnyTeamFavorChange;

    private Dictionary<Team, TeamFavor> teamFavors;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one FavorSystem!");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        teamFavors = new Dictionary<Team, TeamFavor>();

    }

    private void Start()
    {
        // Assuming you have a way to get all teams, like from a GameManager or similar
        foreach (Team team in Match.Instance.GetAllTeams())
        {
            teamFavors.Add(team, new TeamFavor(0, 10));
        }

        Unit.OnAnyUnitExpendFavor += Unit_OnAnyUnitExpendFavor;
    }


    private void Unit_OnAnyUnitExpendFavor(Unit unit)
    {
        TeamFavor teamFavor = teamFavors[unit.GetTeam()];
        teamFavor.amount += 1;

        if (teamFavor.amount >= teamFavor.max)
        {
            //game ends
        }

        teamFavors[unit.GetTeam()] = teamFavor;

        OnAnyTeamFavorChange?.Invoke(unit.GetTeam());
    }


    public TeamFavor GetTeamFavor(Team team)
    {
        return teamFavors[team];
    }

    public float GetTeamFavorNormalized(Team index)
    {
        return teamFavors[index].GetFavorNormalized();
    }
}
