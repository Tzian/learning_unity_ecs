using Unity.Mathematics;
using UnityEngine;

public struct Util
{
    public int Flatten (float3 pos, int width)
    {
        return ((int) pos.z * width * width) + ((int) pos.y * width) + (int) pos.x;
    }

    public int Flatten (int x, int y, int z, int width)
    {
        return (z * width * width) + (y * width) + x;
    }

    public int Flatten (float x, float y, float z, int width)
    {
        return ((int) z * width * width) + ((int) y * width) + (int) x;
    }

    public float3 UnflattenToFloat3 (int index, int width)
    {
        int z = index / (width * width);
        index -= (z * width * width);
        int y = index / width;
        int x = index % width;
        return new float3 (x, y, z);
    }

    public int3 UnflattenToInt3 (int index, int width)
    {
        int z = index / (width * width);
        index -= (z * width * width);
        int y = index / width;
        int x = index % width;
        return new int3 (x, y, z);
    }

    // 2D to use on atlas coords?
    public int Flatten2D (int x, int z, int size)
    {
        return (z * size) + x;
    }

    public int Flatten2D (int2 i2, int size)
    {
        return (i2.y * size) + i2.x;
    }

    public int Flatten2D (float x, float z, int size)
    {
        return ((int) z * size) + (int) x;
    }

    public int Flatten2D (float3 pos, int size)
    {
        return ((int) pos.z * size) + (int) pos.x;
    }

    public float3 Unflatten2D (int index, int size)
    {
        int x = index % size;
        int z = index / size;

        return new float3 (x, 0, z);
    }

    // takes in a world position and returns the world position for the sector it is within
    public float3 GetSectorPosition (float3 worldPos, int sectorSize)
    {
        int x = (int) (worldPos.x / sectorSize);
        int y = (int) (worldPos.y / sectorSize);
        int z = (int) (worldPos.z / sectorSize);

        return new Vector3 (x * sectorSize, y * sectorSize, z * sectorSize);
    }

    public int3 GetSectorPosition (int3 worldPos, int sectorSize)
    {
        int x = (int) (worldPos.x / sectorSize);
        int y = (int) (worldPos.y / sectorSize);
        int z = (int) (worldPos.z / sectorSize);

        return new int3 (x * sectorSize, y * sectorSize, z * sectorSize);
    }

    public bool Float3sMatchXYZ(float3 a, float3 b)
    {
        if (a.x == b.x && a.y == b.y && a.z == b.z) return true;
        else return false;
    }

    public int maximum (int x, int y, int z)  // for getting maximum out of 3 
    {
        int a = math.max (x, y);
        int b = math.max (a, z);

        return b;
    }

    public bool IsInRange3D (float3 fromPos, float3 toPos, int rangeInUnits)  // unit = unity unit, one block
    {
        float3 difference = fromPos - toPos;
        return InRangeCheck3D (difference, rangeInUnits);
    }

    public bool InRangeCheck3D (float3 difference, int range)
    {
        int x = (int) difference.x;
        int y = (int) difference.y;
        int z = (int) difference.z;

        int absX = x < 0 ? -x : x;
        int absY = y < 0 ? -y : y;
        int absZ = z < 0 ? -z : z;

        return absX < range && absY < range && absZ < range;
    }

    public float3 EdgeOverlap3D(float3 localPosition, int sectorSize)
    {
        return new float3(
            (int)localPosition.x == sectorSize ? 1 : (int)localPosition.x < 0 ? -1 : 0,
            (int)localPosition.y == sectorSize ? 1 : (int)localPosition.y < 0 ? -1 : 0,
            (int)localPosition.z == sectorSize ? 1 : (int)localPosition.z < 0 ? -1 : 0
            );
    }

    public int WrapAndFlatten(int3 position, int sectorSize)
    {
        return Flatten(WrapBlockIndex(position, sectorSize), sectorSize);
    }

    int3 WrapBlockIndex(int3 pos, int sectorSize)
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;

        if (x == -1)
            x = sectorSize - 1;
        else if (x == sectorSize)
            x = 0;

        if (y == -1)
            y = sectorSize - 1;
        else if (y == sectorSize)
            y = 0;

        if (z == -1)
            z = sectorSize - 1;
        else if (z == sectorSize)
            z = 0;

        return new int3(x, y, z);
    }



    // sector range checks if needed inside burst jobs/ mimics methods in matrix3D
    public bool MatrixInRangeFromWorldPositionCheck (int3 fromWorldPos, int3 toWorldPos, int3 rootPos, int range, int sectorSize)
    {
        int3 fromMatrixPos = WorldToSectorMatrixPosition(fromWorldPos, rootPos, sectorSize);
        int3 toMatrixPos = WorldToSectorMatrixPosition(toWorldPos, rootPos, sectorSize);

        return SectorMatrixInRangeFromMatrixPosition(fromMatrixPos, toMatrixPos, range);
    }

    int3 WorldToSectorMatrixPosition (int3 worldPosition, int3 rootPos, int itemWorldSize)
    {
        return (worldPosition - rootPos) / itemWorldSize;
    }

    bool SectorMatrixInRangeFromMatrixPosition (int3 fromMatrixPos, int3 toMatrixPos, int range)
    {
        if (fromMatrixPos.x >= toMatrixPos.x - range &&
            fromMatrixPos.y >= toMatrixPos.y - range &&
            fromMatrixPos.z >= toMatrixPos.z - range &&
            fromMatrixPos.x <= toMatrixPos.x + range &&
            fromMatrixPos.y <= toMatrixPos.y + range &&
            fromMatrixPos.z <= toMatrixPos.z + range)

            return true;
        else
            return false;
    }
}

public struct CubeDirections
{
    public int3 this [int direction]
    {
        get
        {
            switch (direction)
            {
                case 0: return new int3 (0, 0, 1);
                case 1: return new int3 (0, 0, -1);
                case 2: return new int3 (1, 0, 0);
                case 3: return new int3 (-1, 0, 0);
                case 4: return new int3 (0, 1, 0);
                case 5: return new int3 (0, -1, 0);

                default: throw new System.ArgumentOutOfRangeException ("Index out of range 5: " + direction);
            }
        }
    }
}