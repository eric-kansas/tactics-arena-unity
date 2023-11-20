using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergySystem : MonoBehaviour
{

    [Serializable]
    public struct TeamEnergy
    {
        public int amount;
        public int max;

        public TeamEnergy(int v1, int v2)
        {
            amount = v1;
            max = v2;
        }

        public float GetEnergyNormalized()
        {
            return (float)amount / max;
        }
    }

    public static EnergySystem Instance { get; private set; }
    public static Action<Team> OnAnyTeamEnergyChange;
    private Dictionary<Team, TeamEnergy> teamEnergies;


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one EnergySystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        teamEnergies = new Dictionary<Team, TeamEnergy>();

    }

    private void Start()
    {
        // Assuming you have a way to get all teams, like from a GameManager or similar
        foreach (Team team in Match.Instance.GetAllTeams())
        {
            teamEnergies.Add(team, new TeamEnergy(1000, 1000));
        }

        Unit.OnAnyUnitOutOfEnergy += Unit_OnAnyUnitOutOfEnergy;
    }

    private void Unit_OnAnyUnitOutOfEnergy(Unit unit)
    {
        // subtract energy from team
        TeamEnergy teamEnergy = teamEnergies[unit.GetTeam()];
        teamEnergy.amount -= (int)unit.GetMaxHealth();

        if (teamEnergy.amount <= 0)
        {
            //game ends
        }

        teamEnergies[unit.GetTeam()] = teamEnergy;
        unit.Heal(unit.GetMaxHealth());

        OnAnyTeamEnergyChange?.Invoke(unit.GetTeam());
    }

    public TeamEnergy GetTeamEnergy(Team team)
    {
        return teamEnergies[team];
    }

    public float GetTeamEnergyNormalized(Team team)
    {
        return teamEnergies[team].GetEnergyNormalized();
    }

}
