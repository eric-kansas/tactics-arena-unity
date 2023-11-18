using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PathfindingGridDebugObject : GridDebugObject
{

    [SerializeField] private TextMeshPro terrainText;
    [SerializeField] private TextMeshPro elevationText;
    [SerializeField] private Transform elevationPrefabTransform;
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
        elevationText.text = gridObject.GetElevation().ToString();

        // Update the prefab's material based on terrain type
        int terrainIndex = (int)gridObject.GetTerrainType();
        prefabMeshRenderer.material = terrainMaterials[terrainIndex];

        float elevationScaleFactor = 0.5f; // Adjust this factor as needed

        transform.localScale = new Vector3(
            transform.localScale.x,
            gridObject.GetElevation() * elevationScaleFactor,
            transform.localScale.z
        );
    }

}
