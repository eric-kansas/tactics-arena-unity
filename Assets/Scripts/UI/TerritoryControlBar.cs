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

    private void TerritorySystem_OnTerritoryScoreChanged(int team, int points)
    {
        UpdateTerritoryBars();
    }

    private void UpdateTerritoryBars()
    {

        float minFillAmount = 0.01f; // Define a minimum fill amount

        team1BarImage.fillAmount = Mathf.Max(TerritorySystem.Instance.GetTeamTerritoryScoreNormalized(0), minFillAmount);
        team2BarImage.fillAmount = Mathf.Max(TerritorySystem.Instance.GetTeamTerritoryScoreNormalized(1), minFillAmount);
    }
}
