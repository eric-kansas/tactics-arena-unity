using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerritorySystemUI : MonoBehaviour
{
    private void Start()
    {
        TerritorySystem.OnTerritoryOwnerChanged += TerritorySystem_OnTeamCapturedTerritory;

    }

    private void TerritorySystem_OnTeamCapturedTerritory(int obj)
    {
        throw new NotImplementedException();
    }
}
