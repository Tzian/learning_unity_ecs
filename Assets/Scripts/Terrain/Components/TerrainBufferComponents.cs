using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity (10)]
public struct VoxelGeology : IBufferElementData
{
    public ushort ID;
}

[InternalBufferCapacity (0)]
public struct Verts : IBufferElementData
{
    public float3 vertex;
}

[InternalBufferCapacity(0)]
public struct Tris : IBufferElementData
{
    public int triangle;
}

[InternalBufferCapacity(0)]
public struct Uvs : IBufferElementData
{
    public float2 uv;
}