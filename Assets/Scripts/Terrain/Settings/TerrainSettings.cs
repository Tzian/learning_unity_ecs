using UnityEditor;
using UnityEngine;

public class TerrainSettings
{
    public static Material terrainMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TerrainMaterial.mat");


    public const int sectorSize = 16;
    public const int areaGenerationRange = 32;   // this is voxel/block range (unity units)

    public const int playerStartHeight = seaLevel;   //((maxWorldGenHeight - minWorldGenHeight) / 2) + minWorldGenHeight;
    public const int minWorldGenHeight = 50;
    public const int maxWorldGenHeight = 460;
    public const int seaLevel = 128;
    public const int dirtLayerThickness = 3;  // fixed atm, later randomise a bit

    // terrain surface cell noise settings, this noise determines topography type for the area
    public const int seed = 1337;
    public const float cellFrequency = 0.02f;  // affects region size in the cellular noise used to generate them
    public const float perterbAmp = 0.50f;
    public const float cellularJitter = 0.5f;
}