using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerritoryCardWorldUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI gridPositionCardSuit;
    [SerializeField] private TextMeshProUGUI gridPositionCardValue;

    private void Start()
    {
        UpdateGridInfoText();
    }

    private void Update()
    {
        UpdateGridInfoText();
    }


    private void UpdateGridInfoText()
    {
        gridPositionCardSuit.text = "Suit: ";
        gridPositionCardValue.text = "Value: ";
    }

}
