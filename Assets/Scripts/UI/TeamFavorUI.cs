using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeamFavorUI : MonoBehaviour
{

    [SerializeField] private Image favorBarImage1;
    [SerializeField] private Image favorBarImage2;

    private float minFillAmount = 0.001f; // Define a minimum fill amount

    private void Start()
    {
        FavorSystem.OnAnyTeamFavorChange += FavorSystem_OnAnyTeamFavorChange;
        UpdateFavorBars();
    }

    private void FavorSystem_OnAnyTeamFavorChange(int team)
    {
        UpdateFavorBar(team);
    }

    private void UpdateFavorBars()
    {
        favorBarImage1.fillAmount = Mathf.Max(FavorSystem.Instance.GetTeamFavorNormalized(0), minFillAmount);
        favorBarImage2.fillAmount = Mathf.Max(FavorSystem.Instance.GetTeamFavorNormalized(1), minFillAmount);
    }

    private void UpdateFavorBar(int team)
    {

        if (team == 0)
        {
            favorBarImage1.fillAmount = Mathf.Max(FavorSystem.Instance.GetTeamFavorNormalized(team), minFillAmount);
        }
        else
        {
            favorBarImage2.fillAmount = Mathf.Max(FavorSystem.Instance.GetTeamFavorNormalized(team), minFillAmount);
        }
    }

}
