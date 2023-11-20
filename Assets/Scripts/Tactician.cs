using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Tactician
{
    [SerializeField] private string name;
    [SerializeField] private List<Team> teams;

    public Tactician(string name)
    {
        this.name = name;
        teams = new List<Team>();
    }

    public void AddTeam(Team team)
    {
        teams.Add(team);
    }
}
