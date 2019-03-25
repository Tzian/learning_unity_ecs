using UnityEngine;
using static FastNoise;


public struct TopographyTypeStats
{
    public TopographyTypes topographyType;
    public int levelCount;
    public int levelHeight;
    public int minHeightOfTType;
    public int maxHeightOfTType;

    public float amplitude;

    public float frequency;
    public int fractalOctaves;
    public float fractalGain;
    public NoiseType noiseType;
    public FractalType fractalType;
    //Interp interp;
}

public struct TopographyTypeUtil
{
    public float AddNoise (TopographyTypeStats stats, int x, int z)
    {
        FastNoise noise = new FastNoise ();

        noise.SetFrequency (stats.frequency);
        noise.SetFractalOctaves (stats.fractalOctaves);
        noise.SetFractalGain (stats.fractalGain);
        noise.SetNoiseType (stats.noiseType);
        noise.SetFractalType (stats.fractalType);

        return noise.GetNoise01 (x, z) * stats.amplitude;
    }

    public TopographyTypeStats GetTerrainTypeStats (TopographyTypes type)
    {
        switch (type)
        {
            case TopographyTypes.Mountains:

                TopographyTypeStats mountains = new TopographyTypeStats
                {
                    topographyType = TopographyTypes.Mountains,
                    levelCount = 10,
                    levelHeight = 5,

                    minHeightOfTType = TerrainSettings.minMountainHeight,
                    maxHeightOfTType = TerrainSettings.maxMountainHeight,
                    amplitude = 50,
                    frequency = 0.003f,
                    fractalOctaves = 3,
                    fractalGain = 0.4f,
                    noiseType = NoiseType.CubicFractal,
                    fractalType = FractalType.RigidMulti,
                };
                return mountains;

            case TopographyTypes.Hills:

                TopographyTypeStats hills = new TopographyTypeStats
                {
                    topographyType = TopographyTypes.Hills,
                    levelCount = 5,
                    levelHeight = 2,

                    minHeightOfTType = TerrainSettings.minHillHeight,
                    maxHeightOfTType = TerrainSettings.maxHillHeight,
                    amplitude = 20,
                    frequency = 0.006f,
                    fractalOctaves = 3,
                    fractalGain = 0.2f,
                    noiseType = NoiseType.Cubic,
                    fractalType = FractalType.FBM,
                };
                return hills;

            case TopographyTypes.Flat:

                TopographyTypeStats flat = new TopographyTypeStats
                {
                    topographyType = TopographyTypes.Flat,
                    levelCount = 1,
                    levelHeight = 1,

                    minHeightOfTType = TerrainSettings.minFlatHeight,
                    maxHeightOfTType = TerrainSettings.maxFlatHeight,
                    amplitude = 0,
                    frequency = 0.01f,
                    fractalOctaves = 2,
                    fractalGain = 0.2f,
                    noiseType = NoiseType.CubicFractal,
                    fractalType = FractalType.FBM,
                };
                return flat;

            case TopographyTypes.Valleys:

                TopographyTypeStats valleys = new TopographyTypeStats
                {
                    topographyType = TopographyTypes.Valleys,
                    levelCount = 4,
                    levelHeight = 2,

                    minHeightOfTType = TerrainSettings.minValleyHeight,
                    maxHeightOfTType = TerrainSettings.maxValleyHeight,
                    amplitude = 10,
                    frequency = 0.003f,
                    fractalOctaves = 2,
                    fractalGain = 0.2f,
                    noiseType = NoiseType.CubicFractal,
                    fractalType = FractalType.FBM,
                };
                return valleys;

            case TopographyTypes.Oceanic:

                TopographyTypeStats oceans = new TopographyTypeStats
                {
                    topographyType = TopographyTypes.Oceanic,
                    levelCount = 8,
                    levelHeight = 5,

                    minHeightOfTType = TerrainSettings.minOceanHeight,
                    maxHeightOfTType = TerrainSettings.maxOceanHeight,
                    amplitude = 20,
                    frequency = 0.001f,
                    fractalOctaves = 2,
                    fractalGain = 0.1f,
                    noiseType = NoiseType.CubicFractal,
                    fractalType = FractalType.RigidMulti,
                };
                return oceans;

            default:
                TopographyTypeStats defaultFlat = new TopographyTypeStats
                {
                    topographyType = TopographyTypes.Flat,
                    levelCount = 1,
                    levelHeight = 1,

                    minHeightOfTType = TerrainSettings.minFlatHeight,
                    maxHeightOfTType = TerrainSettings.maxFlatHeight,
                    amplitude = 1,
                    frequency = 0.0001f,
                    fractalOctaves = 3,
                    fractalGain = 0.2f,
                    noiseType = NoiseType.SimplexFractal,
                    fractalType = FractalType.FBM,
                };
                return defaultFlat;
        }
    }

    

    public TopographyTypes TerrainType (float cellNoise)
    {
        if (cellNoise >= 0.9f)  // mountains
            return TopographyTypes.Mountains;
        else if (cellNoise >= 0.7f) // hills
            return TopographyTypes.Hills;
        else if (cellNoise >= 0.4f) // flat
            return TopographyTypes.Flat;
        else if (cellNoise >= 0.2f) // valleys
            return TopographyTypes.Valleys;
        else
            return TopographyTypes.Oceanic;  // ocean
    }
}

public enum TopographyTypes
{
    Mountains,  // Heights of more than 300 meters above surrounding terrain, 
    Hills,      // Heights of 150 to 300 meters, rolling to peak
    Flat,        // Flat or very gentle rolling - plains and plateaus
    Valleys,    // Natural depressions between hills or mountains, usually gradual slope down to body of water
    Oceanic,    // varied depths, shallow oceans ~200 meters deep are marginal or inland extensions of an ocean (coastal), average is ~3,600 meters
}

public enum TopographyFeatures
{
    Gradual,    // gradual changes
    Drastic,    // drastic change
    Gorge,      // narrow Valley between hills and mountains, typically with steep cliff walls and river running thru it
    Ravine,     // deep narrow gorge with steep cliff sides, narrower than Canyon, 
    Canyon,     // a deep gorge between cliffs, box canyons have cliffs on 3 sides, wider than ravine, up to 750km long, up to 5,500m deep, up to 30km wide
    Trench,     // Deep (up to thousands of meters) usally only occuring on seabed, deeper than width and narrow compared to length, generally parallel to volcanic island arcs and about 200 km away from them, oceanic trenches are typically 3 - 4 km below surround ocean floor
}