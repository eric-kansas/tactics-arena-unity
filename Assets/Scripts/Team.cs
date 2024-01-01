using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Team", menuName = "Game/Team")]
public class Team : ScriptableObject
{
    [SerializeField] private string teamName;
    [SerializeField] private Material teamMaterial;
    [SerializeField] private Material teamColor;
    [SerializeField] private List<Player> players;

    public void AddPlayer(Player player)
    {
        players.Add(player);
    }

    public List<Player> GetPlayers()
    {
        return players;
    }

    public Material GetMaterial()
    {
        return teamMaterial;
    }

    public Color GetColor()
    {
        return teamColor.color;
    }

    public Material GetTeamColorAsMaterial()
    {
       return teamColor;
    }

}
