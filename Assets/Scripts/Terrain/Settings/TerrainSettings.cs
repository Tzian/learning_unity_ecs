using UnityEditor;
using UnityEngine;

public class TerrainSettings
{
    //public static Material terrainMaterial = AssetDatabase.LoadAssetAtPath<Material> ("Assets/Materials/TerrainMaterial.mat");
    public static Material terrainMaterial = AssetDatabase.LoadAssetAtPath<Material> ("Assets/Materials/TestMaterial.mat");

    public const int sectorSize = 16;
    public const int seaLevel = 128;
    public const int sectorGenerationRange = 8;

    // TerrainGen
    public const int minWorldHeight = 50;
    public const int maxWorldHeight = 460;
    public const int playerStartHeight = ((maxWorldHeight - minWorldHeight) / 2) + minWorldHeight;

    // terrain type adjustment values
    public const int minMountainHeight = 50;
    public const int maxMountainHeight = 250;
    public const int minHillHeight = 5;
    public const int maxHillHeight = 50;
    public const int minFlatHeight = 0;
    public const int maxFlatHeight = 5;
    public const int minValleyHeight = -50;
    public const int maxValleyHeight = 0;
    public const int minOceanHeight = -50;
    public const int maxOceanHeight = -250;

    // terrain surface cell noise settings (first noise that terrain gets)
    public const int seed = 1337;
    public const float cellFrequency = 0.002f;  // affects region size in the cellular noise used to generate them
    public const float perterbAmp = 0.50f;
    public const float cellularJitter = 0.5f;
}

enum SectorDrawType { NONE, INRANGE, OUTOFRANGE }
