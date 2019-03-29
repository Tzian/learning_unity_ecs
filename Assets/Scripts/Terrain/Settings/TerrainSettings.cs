using UnityEditor;
using UnityEngine;

public class TerrainSettings
{
    public static Material terrainMaterial = AssetDatabase.LoadAssetAtPath<Material> ("Assets/Materials/TerrainMaterial.mat");
    //public static Material terrainMaterial = AssetDatabase.LoadAssetAtPath<Material> ("Assets/Materials/TestMaterial.mat");

    public const int sectorSize = 16;
    public const int seaLevel = 128;
    public const int sectorGenerationRange = 4;

    // TerrainGen
    public const int minWorldHeight = 30;
    public const int maxWorldHeight = 480;


    // terrain type adjustment values
    public const int minMountainHeight = 50;
    public const int maxMountainHeight = 250;
    public const int minHillHeight = 5;
    public const int maxHillHeight = 50;
    public const int minFlatHeight = -5;
    public const int maxFlatHeight = 5;
    public const int minValleyHeight = -50;
    public const int maxValleyHeight = -5;
    public const int minOceanHeight = -250;
    public const int maxOceanHeight = -50;

    // terrain surface cell noise settings (first noise that terrain gets)
    public const int seed = 1337;
    public const float cellFrequency = 0.015f;  // affects region size in the cellular noise used to generate them
    public const float perterbAmp = 0.5f;
    public const float cellularJitter = 0.15f;
}

enum SectorDrawType { NONE, INRANGE, OUTOFRANGE }
