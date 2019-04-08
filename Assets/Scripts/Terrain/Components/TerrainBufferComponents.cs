using Unity.Entities;



[InternalBufferCapacity (10)]
public struct VoxelGeology : IBufferElementData
{
    public ushort ID;
}