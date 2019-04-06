using UnityEditor;
using UnityEngine;

public class TerrainSettings
{
    public static Material terrainMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TerrainMaterial.mat");


    public const int sectorSize = 16;
    public const int areaGenerationRange = 32;   // this is voxel/block range (unity units)

    public const int playerStartHeight = ((maxWorldGenHeight - minWorldGenHeight) / 2) + minWorldGenHeight;
    public const int minWorldGenHeight = 50;
    public const int maxWorldGenHeight = 460;







}