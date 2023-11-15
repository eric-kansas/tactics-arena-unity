using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerritoryControlIcon : MonoBehaviour
{

    [SerializeField] private Image foregroundImage;

    private void Start()
    {
        // EnergySystem.OnAnyTeamEnergyChange += EnergySystem_OnAnyTeamEnergyChange;
        // UpdateEnergyBars();
    }

    public void SetTeamControl(int teamID)
    {
        if (teamID < 0)
        {
            foregroundImage.color = Color.grey;
            return;
        }

        foregroundImage.color = TurnSystem.TeamColorList[teamID];
    }
}
