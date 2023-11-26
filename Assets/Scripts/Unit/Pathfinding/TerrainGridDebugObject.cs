using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TerrainGridDebugObject : GridDebugObject
{

    [SerializeField] private TextMeshPro terrainText;
    [SerializeField] private TextMeshPro elevationText;
    [SerializeField] private GameObject fogOfWarObject;
    [SerializeField] private MeshRenderer prefabMeshRenderer;
    [SerializeField] private Material[] terrainMaterials; // Assuming you have materials for each terrain type

    private GridObject gridObject;

    public override void SetGridObject(object gridObject)
    {
        base.SetGridObject(gridObject);
        this.gridObject = (GridObject)gridObject;
    }

    protected override void Update()
    {
        base.Update();
        terrainText.text = gridObject.GetTerrainType().ToString();

        bool seePosition = FogOfWarSystem.Instance.IsVisible(Match.Instance.GetClientTeam(), gridObject.GetGridPosition());
        int elevation = FogOfWarSystem.Instance.GetKnownElevation(Match.Instance.GetClientTeam(), gridObject.GetGridPosition());
        fogOfWarObject.SetActive(!seePosition);

        elevationText.text = elevation.ToString();

        // Update the prefab's material based on terrain type
        int terrainIndex = (int)gridObject.GetTerrainType();
        prefabMeshRenderer.material = terrainMaterials[terrainIndex];

        transform.localScale = new Vector3(
            transform.localScale.x,
            elevation * LevelGrid.Instance.ElevationScaleFactor,
            transform.localScale.z
        );
    }

}
