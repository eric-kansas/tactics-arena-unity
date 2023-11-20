using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TerritoryControlBar : MonoBehaviour
{

    [SerializeField] private Image team1BarImage;
    [SerializeField] private Image team2BarImage;

    private void Start()
    {
        TerritorySystem.OnTerritoryScoreChanged += TerritorySystem_OnTerritoryScoreChanged;
        UpdateTerritoryBars();
    }

    private void TerritorySystem_OnTerritoryScoreChanged(Team team, int points)
    {
        UpdateTerritoryBars();
    }

    private void UpdateTerritoryBars()
    {

        float minFillAmount = 0.01f; // Define a minimum fill amount

        team1BarImage.fillAmount = Mathf.Max(TerritorySystem.Instance.GetTeamTerritoryScoreNormalized(Match.Instance.GetClientTeam()), minFillAmount);
        team2BarImage.fillAmount = Mathf.Max(TerritorySystem.Instance.GetTeamTerritoryScoreNormalized(Match.Instance.GetAwayTeam()), minFillAmount);
    }
}
