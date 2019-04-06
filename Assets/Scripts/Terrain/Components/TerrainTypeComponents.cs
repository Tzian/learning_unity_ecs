using Unity.Entities;
using Unity.Mathematics;



public struct Voxel : IComponentData
{
    public float3 WorldPosition;
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