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
}

[InternalBufferCapacity (0)]
public struct Topography : IBufferElementData
{
    public float surfaceHeight;
    public int topopgraphyType;
    public float adjSurfaceHeight;
    public int adjTopographyType;
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

[InternalBufferCapacity(0)]
public struct BlockFaces : IBufferElementData
{
    public int north, south, east, west, up, down;
    public int count;
    public int faceIndex, triIndex, vertIndex, uvIndex;
    public ushort atlasID;
    
    public void SetCount()
    {
        if (north > 0) count++;
        if (south > 0) count++;
        if (east  > 0) count++;
        if (west  > 0) count++;
        if (up    > 0) count++;
        if (down  > 0) count++;
    }

    public int this[int side]
    {
        get
        {
            switch (side)
            {
                case 0: return north;
                case 1: return south;
                case 2: return east;
                case 3: return west;
                case 4: return up;
                case 5: return down;
                default: throw new System.ArgumentOutOfRangeException("Index out of range 5: " + side);
            }
        }

        set
        {
            switch (side)
            {
                case 0: north = value; break;
                case 1: south = value; break;
                case 2: east = value; break;
                case 3: west = value; break;
                case 4: up = value; break;
                case 5: down = value; break;
                default: throw new System.ArgumentOutOfRangeException("Index out of range 5: " + side);
            }
        }
    }
}

[InternalBufferCapacity(0)]
public struct MeshVert : IBufferElementData
{
    public float3 vertex;
}

[InternalBufferCapacity(0)]
public struct MeshUv : IBufferElementData
{
    public float2 uv;
}

[InternalBufferCapacity(0)]
public struct MeshNorm : IBufferElementData
{
    public float3 normal;
}

[InternalBufferCapacity(0)]
public struct MeshTri : IBufferElementData
{
    public int triangle;
}