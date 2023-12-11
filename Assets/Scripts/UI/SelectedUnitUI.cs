using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SelectedUnitUI : MonoBehaviour
{

    [SerializeField] private GameObject container;

    [SerializeField] private TextMeshProUGUI playerNameText;

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI classText;

    [SerializeField] private TextMeshProUGUI favorRoleText;

    [SerializeField] private TextMeshProUGUI combatRoleText;


    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;

        UpdateSelectedUnitText();
    }

    private void UnitActionSystem_OnSelectedUnitChanged()
    {
        UpdateSelectedUnitText();
    }

    private void UpdateSelectedUnitText()
    {
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();
        if (selectedUnit == null)
        {
            container.SetActive(false);
            playerNameText.text = "";
            levelText.text = "";
            classText.text = "";
            favorRoleText.text = "";
            combatRoleText.text = "";
            return;
        }

        Player playerData = selectedUnit.GetPlayerData();
        playerNameText.text = playerData.GetPlayerName();
        levelText.text = "Level: " + playerData.GetLevel();
        classText.text = "Class: " + playerData.GetPlayerClassDisplayText();
        favorRoleText.text = "Favor: " + playerData.GetFavorRoleDisplayText();
        combatRoleText.text = "Combat: " + playerData.GetCombatRoleDisplayText();
        container.SetActive(true);

    }

}
