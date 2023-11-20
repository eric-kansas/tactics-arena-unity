using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerritorySystemUI : MonoBehaviour
{

    [SerializeField] private GameObject iconsContainer;
    [SerializeField] private Transform iconPrefab;

    private TerritoryControlIcon[] territoryIcons;

    private void Start()
    {
        TerritorySystem.OnTerritoryOwnerChanged += TerritorySystem_OnTeamCapturedTerritory;

        var zones = TerritorySystem.Instance.GetZones();
        territoryIcons = new TerritoryControlIcon[zones.Count];

        int index = 0;
        foreach (KeyValuePair<int, Rect> zoneEntry in zones)
        {
            int zoneID = zoneEntry.Key;
            Rect zoneRect = zoneEntry.Value;

            // Calculate the world position for the icon based on the zone's Rect
            Vector3 worldPosition = LevelGrid.Instance.GetWorldPositionFromRect(zoneRect);

            // Instantiate the icon
            Transform iconTransform = Instantiate(iconPrefab, Vector3.zero, Quaternion.identity, iconsContainer.transform);
            TerritoryControlIcon territoryControlIconUI = iconTransform.GetComponent<TerritoryControlIcon>();
            territoryControlIconUI.GetButton().onClick.AddListener(() =>
            {
                CameraController.Instance.SetlookAtPoint(worldPosition);
            });

            // Store the icon in the array
            territoryIcons[index] = territoryControlIconUI;
            index++;
        }
    }

    private void TerritorySystem_OnTeamCapturedTerritory(int zone, Team team)
    {
        territoryIcons[zone].SetTeamControl(team);
    }
}
