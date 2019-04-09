using Unity.Entities;
using Unity.Mathematics;



public struct Voxel : IComponentData
{
    public float3 WorldPosition;
    public ushort GeologyID;
}

public struct SurfaceTopography : IComponentData
{
    public int Type;
    public float Height;
}

public struct VoxelVisibleFaces : IComponentData
{
    public byte north;
    public byte south;
    public byte east;
    public byte west;
    public byte up;
    public byte down;

    public int faceCount;
    public int vertexCount;
    public int triangleCount;
    public int uvsCount;

    public byte this[byte side]
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