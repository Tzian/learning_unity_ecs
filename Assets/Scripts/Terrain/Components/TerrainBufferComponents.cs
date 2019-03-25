using Unity.Entities;
using Unity.Mathematics;


[InternalBufferCapacity (0)]
public struct Block : IBufferElementData
{
    public int blockIndex;
    public float3 localPosition;
    public float3 worldPosition;

    public Entity parentSectorEntity;
    public float3 parentSectorWorldPosition;

    public ushort atlasID;

    public int debug;
}

[InternalBufferCapacity (0)]
public struct Topography : IBufferElementData
{
    public float surfaceHeight;
    public float adjSurfaceHeight;
    public float dist2Edge;
}

[InternalBufferCapacity (0)]
public struct WorleySurfaceNoise : IBufferElementData, System.IComparable<WorleySurfaceNoise>
{
    public int CompareTo (WorleySurfaceNoise other)
    {
        return currentSurfaceCellValue.CompareTo (other.currentSurfaceCellValue);
    }

    public float3 currentCellPosition;
    public int2 currentCellIndexInMap;
    public float currentSurfaceCellValue;
    public float distance2Edge;
    public float adjacentSurfaceCellValue;
}

[InternalBufferCapacity (10)]
public struct SurfaceCell : IBufferElementData
{
    public float surfaceGenValue;
    public int2 indexInBuffer;

    public float3 position;  // ignore the Y value it is always 0
}