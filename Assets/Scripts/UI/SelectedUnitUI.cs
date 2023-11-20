using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class SelectedUnitUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI playNameText;

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;

        UpdateTurnText();
    }

    private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs e)
    {
        UpdateTurnText();
    }

    private void UpdateTurnText()
    {
        Unit selectedUnitUI = UnitActionSystem.Instance.GetSelectedUnit();
        if (selectedUnitUI == null)
        {
            playNameText.text = "";
            return;
        }
        playNameText.text = selectedUnitUI.name;
    }
}
