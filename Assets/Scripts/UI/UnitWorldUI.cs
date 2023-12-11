using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitWorldUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI actionPointsText;
    [SerializeField] private Unit unit;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image favorBarImage;

    [SerializeField] private UnitEnergySystem unitEnergySystem;
    [SerializeField] private UnitFavor favorSystem;

    private float minFillAmount = 0.01f;


    private void Start()
    {
        Unit.OnAnyActionPointsChanged += Unit_OnAnyActionPointsChanged;
        unitEnergySystem.OnDamaged += UnitEnergySystem_OnDamaged;
        unitEnergySystem.OnHealed += UnitEnergySystem_OnHealed;

        favorSystem.OnAdded += FavorSystem_OnAdded;
        favorSystem.OnRemoved += FavorSystem_OnRemoved;

        UpdateActionPointsText();
        UpdateHealthBar();
        UpdateFavorBar();
    }

    private void FavorSystem_OnRemoved(object sender, EventArgs e)
    {
        UpdateFavorBar();
    }

    private void FavorSystem_OnAdded(object sender, EventArgs e)
    {
        UpdateFavorBar();
    }

    private void UpdateFavorBar()
    {
        favorBarImage.fillAmount = Mathf.Max(favorSystem.GetFavorNormalized(), minFillAmount);
    }

    private void UpdateActionPointsText()
    {
        actionPointsText.text = unit.GetActionPoints().ToString();
    }

    private void Unit_OnAnyActionPointsChanged()
    {
        UpdateActionPointsText();
    }

    private void UpdateHealthBar()
    {
        healthBarImage.fillAmount = Mathf.Max(unitEnergySystem.GetEnergyNormalized(), minFillAmount);
    }

    private void UnitEnergySystem_OnDamaged(object sender, EventArgs e)
    {
        UpdateHealthBar();
    }

    private void UnitEnergySystem_OnHealed(object sender, EventArgs e)
    {
        UpdateHealthBar();
    }

}
