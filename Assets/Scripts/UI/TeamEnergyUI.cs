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

    private void EnergySystem_OnAnyTeamEnergyChange(int team)
    {
        UpdateEnergyBar(team);
    }

    private void UpdateEnergyBars()
    {
        energyBarImage1.fillAmount = EnergySystem.Instance.GetTeamEnergyNormalized(0);
        energyBarImage2.fillAmount = EnergySystem.Instance.GetTeamEnergyNormalized(1);

    }

    private void UpdateEnergyBar(int team)
    {
        if (team == 0)
        {
            energyBarImage1.fillAmount = EnergySystem.Instance.GetTeamEnergyNormalized(team);
        }
        else
        {
            energyBarImage2.fillAmount = EnergySystem.Instance.GetTeamEnergyNormalized(team);

        }
    }

}
