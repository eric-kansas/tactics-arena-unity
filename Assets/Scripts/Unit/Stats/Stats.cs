using System;
using UnityEngine;

[Serializable]
public class Stats
{
    [SerializeField] private int might;
    [SerializeField] private int endurance;
    [SerializeField] private int agility;
    [SerializeField] private int intelligence;
    [SerializeField] private int perception;
    [SerializeField] private int charisma;

    public int Might => might;
    public int Endurance => endurance;
    public int Agility => agility;
    public int Intelligence => intelligence;
    public int Perception => perception;
    public int Charisma => charisma;

    public int GetPushStrength()
    {
        return 5 + might;
    }

    public int GetMaxEnergy()
    {
        return 5 + endurance;
    }

    public int GetMoveSpeed()
    {
        return 5 + agility;
    }

    public int GetSightDistance()
    {
        return 5 + perception;
    }

    public int GetXPGain()
    {
        return 5 + intelligence;
    }

    public int GetMaxFavor()
    {
        return 10 - charisma;
    }

    public int GetPhysicalArmour()
    {
        int higherStat = endurance > agility ? endurance : agility;
        return 10 + higherStat;
    }

    public int GetMagicArmor()
    {
        int higherStat = intelligence > perception ? intelligence : perception;
        return 10 + higherStat;
    }

}
