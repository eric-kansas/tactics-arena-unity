
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TerrainColorMapping", menuName = "Game/Terrain Color Mapping")]
public class TerrainColorMapping : ScriptableObject
{
    public List<TerrainColor> terrainColors;
}