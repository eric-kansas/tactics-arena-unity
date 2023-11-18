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
    public static Action<int> OnAnyTeamFavorChange;

    private Dictionary<int, TeamFavor> teamFavors;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one FavorSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        teamFavors = new Dictionary<int, TeamFavor>{
                { 0, new(0, 10) },
                { 1, new(0, 10) },
            };
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


    public TeamFavor GetTeamFavor(int index)
    {
        return teamFavors[index];
    }

    public float GetTeamFavorNormalized(int index)
    {
        return teamFavors[index].GetFavorNormalized();
    }
}
