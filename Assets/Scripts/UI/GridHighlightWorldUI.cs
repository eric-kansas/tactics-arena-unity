using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridHighlightWorldUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI gridPositionTerrainType;
    [SerializeField] private TextMeshProUGUI gridPositionHeight;

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
        GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());
        if (LevelGrid.Instance.IsValidGridPosition(mouseGridPosition))
        {
            TerrainType? terrainType = FogOfWarSystem.Instance.GetKnownTerrain(Match.Instance.GetClientTeam(), mouseGridPosition);
            String terrainText;
            if (terrainType.HasValue)
            {
                terrainText = terrainType.Value.ToString();
            }
            else
            {
                terrainText = "???";
            }
            int elevation = FogOfWarSystem.Instance.GetKnownElevation(Match.Instance.GetClientTeam(), mouseGridPosition);

            String elevationText;
            if (elevation > 0)
            {
                elevationText = elevation.ToString();
            }
            else
            {
                elevationText = "???";
            }
            gridPositionTerrainType.text = "Terrain Type: " + terrainText;
            gridPositionHeight.text = "Terrain Height: " + elevationText;
        }
        else
        {
            gridPositionTerrainType.text = "Terrain Type: ???";
            gridPositionHeight.text = "Terrain Height: ???";
        }


    }

}
