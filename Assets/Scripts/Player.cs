using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player", menuName = "Game/Player")]
public class Player : ScriptableObject
{
    [SerializeField] private string playerName;

    [SerializeField] private List<AbilityData> abilities;

    public List<AbilityData> GetAbilities()
    {
        return abilities;
    }

}
