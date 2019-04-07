using Unity.Entities;
using Unity.Mathematics;



public struct Voxel : IComponentData
{
    public float3 WorldPosition;
}

public struct SurfaceTopography : IComponentData
{
    public int Type;
}

public struct VoxelTexture : IComponentData
{
    public ushort ID;
}

public struct VoxelNeighbours : IComponentData
{
    public Entity north;
    public Entity south;
    public Entity east;
    public Entity west;
    public Entity up;
    public Entity down;
}

public struct WorleySurfaceNoise : IComponentData, System.IComparable<WorleySurfaceNoise>
{
    public int CompareTo(WorleySurfaceNoise other)
    {
        return currentSurfaceCellValue.CompareTo(other.currentSurfaceCellValue);
    }

    public float3 currentCellPosition;
    public int2 currentCellIndexInMap;
    public float currentSurfaceCellValue;
    public float distance2Edge;
    public float adjacentSurfaceCellValue;
}