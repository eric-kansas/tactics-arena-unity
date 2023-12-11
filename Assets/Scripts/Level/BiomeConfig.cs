
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainProbability
{
    public TerrainType TerrainType;
    public float Probability; // 0 to 1
    public ElevationRange ElevationRange;

}

[System.Serializable]
public class ElevationRange
{
    public int MinElevation;
    public int MaxElevation;
}


[CreateAssetMenu(fileName = "NewBiomeConfig", menuName = "Game/Biome Configuration")]
public class BiomeConfig : ScriptableObject
{
    public string biomeName;
    public List<TerrainProbability> terrainProbabilities;
}

[System.Serializable]
public class TerrainColor
{
    public TerrainType terrainType;
    public Color color;
    public Material material;
}