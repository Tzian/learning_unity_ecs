using Unity.Entities;
using Unity.Mathematics;


public struct Sector : IComponentData
{
    public Entity entity;
    public float3 worldPosition;
}

public struct SectorDrawRange : IComponentData
{
    public sbyte sectorDrawRange;
}

public struct AdjacentSectors : IComponentData
{
    public Entity north;
    public Entity south;
    public Entity east;
    public Entity west;
    public Entity up;
    public Entity down;

    public Entity this [int side]
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

                default: throw new System.ArgumentOutOfRangeException ("Index out of range 5: " + side);
            }
        }
    }

    public Entity GetByDirection (float3 dir)
    {
        if (dir.x == 0 && dir.y == 0 && dir.z == 1) return north;
        else if (dir.x == 0 && dir.y == 0 && dir.z == -1) return south;
        else if (dir.x == 1 && dir.y == 0 && dir.z == 0) return east;
        else if (dir.x == -1 && dir.y == 0 && dir.z == 0) return west;
        else if (dir.x == 0 && dir.y == 1 && dir.z == 0) return up;
        else if (dir.x == 0 && dir.y == -1 && dir.z == 0) return down;

        else throw new System.ArgumentOutOfRangeException ("Index out of range 5: " + dir);
    }
}

public struct SectorVisFacesCount : IComponentData
{
    public readonly int faceCount;
    public readonly int vertCount;
    public readonly int triCount;
    public readonly int uvCount;

    public SectorVisFacesCount (int faceCount, int vertCount, int triCount, int uvCount)
    {
        this.faceCount = faceCount;
        this.vertCount = vertCount;
        this.triCount = triCount;
        this.uvCount = uvCount;
    }
}
