using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player", menuName = "Game/Player")]
public class Player : ScriptableObject
{
    [SerializeField] private string playerName;
    [SerializeField] private int level;
    [SerializeField] private PlayerClass playerClass;

    [SerializeField] private FavorRole favorRole;

    [SerializeField] private CombatRole combatRole;

    [SerializeField] private Stats stats;
    [SerializeField] private Gear gear;
    [SerializeField] private List<PerkData> perks;

    [SerializeField] private List<AbilityData> abilities;


    public List<AbilityData> GetAbilities()
    {
        return abilities;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public int GetLevel()
    {
        return level;
    }

    public PlayerClass GetPlayerClass()
    {
        return playerClass;
    }

    public FavorRole GetFavorRole()
    {
        return favorRole;
    }

    public CombatRole GetCombatRole()
    {
        return combatRole;
    }

    public Stats GetStats()
    {
        return stats;
    }
    public string GetPlayerClassDisplayText()
    {
        switch (playerClass)
        {
            case PlayerClass.Druid:
                return "Druid";
            case PlayerClass.Monk:
                return "Monk";
            case PlayerClass.Wizard:
                return "Wizard";
            case PlayerClass.Fighter:
                return "Fighter";
            case PlayerClass.Cleric:
                return "Cleric";
            case PlayerClass.Bard:
                return "Bard";
            default:
                return playerClass.ToString();
        }
    }

    public string GetCombatRoleDisplayText()
    {
        switch (combatRole)
        {
            case CombatRole.Striker:
                return "Striker";
            case CombatRole.Leader:
                return "Leader";
            case CombatRole.Defender:
                return "Defender";
            case CombatRole.Controller:
                return "Controller";
            default:
                return combatRole.ToString();
        }
    }

    public string GetFavorRoleDisplayText()
    {
        switch (favorRole)
        {
            case FavorRole.Spectacular:
                return "Spectacular";
            case FavorRole.Empowering:
                return "Empowering";
            case FavorRole.Disruptive:
                return "Disruptive";
            case FavorRole.Strategic:
                return "Strategic";
            default:
                return favorRole.ToString();
        }
    }

    internal Gear GetGear()
    {
        return gear;
    }

    internal List<PerkData> GetPerks()
    {
        return perks;
    }
}

public enum PlayerClass
{
    Druid,
    Monk,
    Wizard,
    Fighter,
    Cleric,
    Bard,
}

public enum CombatRole
{
    Striker,
    Leader,
    Defender,
    Controller,
}

public enum FavorRole
{
    Spectacular,
    Empowering,
    Disruptive,
    Strategic,
}