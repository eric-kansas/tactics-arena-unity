using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamEnergyUI : MonoBehaviour
{

    [SerializeField] private Image energyBarImage1;
    [SerializeField] private Image energyBarImage2;
    private void Start()
    {
        EnergySystem.OnAnyTeamEnergyChange += EnergySystem_OnAnyTeamEnergyChange;
        UpdateEnergyBars();
    }

    private void EnergySystem_OnAnyTeamEnergyChange(Team team)
    {
        UpdateEnergyBar(team);
    }

    private void UpdateEnergyBars()
    {
        energyBarImage1.fillAmount = EnergySystem.Instance.GetTeamEnergyNormalized(Match.Instance.GetClientTeam());
        energyBarImage2.fillAmount = EnergySystem.Instance.GetTeamEnergyNormalized(Match.Instance.GetAwayTeam());
    }

    private void UpdateEnergyBar(Team team)
    {
        if (team == Match.Instance.GetClientTeam())
        {
            energyBarImage1.fillAmount = EnergySystem.Instance.GetTeamEnergyNormalized(team);
        }
        else
        {
            energyBarImage2.fillAmount = EnergySystem.Instance.GetTeamEnergyNormalized(team);

        }
    }

}
