using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Team", menuName = "Game/Team")]
public class Team : ScriptableObject
{
    [SerializeField] private string teamName;
    [SerializeField] private List<Player> players;

    public void AddPlayer(Player player)
    {
        players.Add(player);
    }

    public List<Player> GetPlayers()
    {
        return players;
    }

}
