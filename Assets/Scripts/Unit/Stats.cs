using System;
using UnityEngine;

[Serializable]
public class Stats
{
    [SerializeField] private int moveValue;
    [SerializeField] private int strength;
    [SerializeField] private int endurance;
    [SerializeField] private int agility;
    [SerializeField] private int intelligence;
    [SerializeField] private int perception;
    [SerializeField] private int charisma;

    public int MoveValue => moveValue;
    public int Strength => strength;
    public int Endurance => endurance;
    public int Agility => agility;
    public int Intelligence => intelligence;
    public int Perception => perception;
    public int Charisma => charisma;

}
