using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

public struct Matrix3D<T> where T : struct
{
    NativeArray<T> matrix;
    NativeArray<sbyte> isSet;
    Util util;

    public int width;
    public Allocator label;

    public int3 rootPosition;
    public int itemWorldSize;

    public int Length { get { return matrix.Length; } }

    public void Dispose ()
    {
        if (matrix.IsCreated) matrix.Dispose ();
        if (isSet.IsCreated) isSet.Dispose ();
    }

    // Initialise both matrices 
    public Matrix3D (int width, Allocator label, int3 rootPosition, int itemWorldSize = 1)
    {
        matrix = new NativeArray<T> ((int) math.pow (width, 3), label);
        isSet = new NativeArray<sbyte> (matrix.Length, label);

        this.width = width;
        this.label = label;
        this.rootPosition = rootPosition;
        this.itemWorldSize = itemWorldSize;
    }

    // add and set item to matrix
    public void AddItem (T item, int3 worldPosition)
    {
        if (!WorldPositionIsInMatrix (worldPosition))
            RepositionResize (WorldToMatrixPosition (worldPosition));

        int index = WorldPositionToIndex (worldPosition);
        SetItem (item, index);
    }

    public void SetItem (T item, int index)
    {
        matrix [index] = item;
        isSet [index] = 1;
    }

    // unset item 
    public void UnsetItem (int3 worldPosition)
    {
        UnsetItem (WorldPositionToIndex (worldPosition));
    }

    public void UnsetItem (int index)
    {
        matrix [index] = new T ();
        isSet [index] = 0;
    }

    // item is SET in matrix bools
    public bool ItemIsSet (int3 worldPosition)
    {
        if (!WorldPositionIsInMatrix (worldPosition))
            return false;

        return ItemIsSet (WorldPositionToIndex (worldPosition));
    }

    public bool ItemIsSet (int index)
    {
        if (index < 0 || index >= matrix.Length)
            return false;

        return isSet [index] > 0;
    }

    // bool check to see if item is in the matrix AND isSet
    public bool TryGetItem (int3 worldPosition, out T item)
    {
        if (!WorldPositionIsInMatrix (worldPosition) || !ItemIsSet (worldPosition))
        {
            item = new T ();
            return false;
        }
        item = GetItem (WorldPositionToIndex (worldPosition));
        return true;
    }

    // get item out of matrix by index or worldPosition
    public T GetItem (int3 worldPosition)
    {
        int index = WorldPositionToIndex (worldPosition);
        return GetItem (index);
    }

    public T GetItem (int index)
    {
        return matrix [index];
    }

    #region Position is within matrix checks

    public bool WorldPositionIsInMatrix (int3 worldPosition, int offset = 0)
    {
        int3 matrixPosition = WorldToMatrixPosition (worldPosition);

        return MatrixPositionIsInMatrix (matrixPosition, offset);
    }

    public bool MatrixPositionIsInMatrix (int3 matrixPosition, int offset = 0)
    {
        int arrayWidth = width - 1;

        if (matrixPosition.x >= offset && matrixPosition.x <= arrayWidth - offset &&
            matrixPosition.y >= offset && matrixPosition.y <= arrayWidth - offset &&
            matrixPosition.z >= offset && matrixPosition.z <= arrayWidth - offset)
            return true;
        else
            return false;
    }
  
    #endregion

    #region In range checks
    // in range from
    public bool InRangeFromWorldPosition (int3 fromWorldPos, int3 toWorldPos, int offset)
    {
        int3 fromMatrixPos = WorldToMatrixPosition (fromWorldPos);
        int3 toMatrixPos = WorldToMatrixPosition (toWorldPos);

        return InRangeFromMatrixPosition (fromMatrixPos, toMatrixPos, offset);
    }

    public bool InRangeFromMatrixPosition (int3 fromMatrixPos, int3 toMatrixPos, int offset)
    {
        if (fromMatrixPos.x >= toMatrixPos.x - offset &&
            fromMatrixPos.y >= toMatrixPos.y - offset &&
            fromMatrixPos.z >= toMatrixPos.z - offset &&
            fromMatrixPos.x <= toMatrixPos.x + offset &&
            fromMatrixPos.y <= toMatrixPos.y + offset &&
            fromMatrixPos.z <= toMatrixPos.z + offset)

            return true;
        else
            return false;
    }
    #endregion

    #region Reposition and Resize methods

    public int3 RepositionResize (int3 matrixPosition)
    {
        // CustomDebugTools.IncrementDebugCount ("Matrix march count");
        int x = matrixPosition.x;
        int y = matrixPosition.y;
        int z = matrixPosition.z;

        float3 rootPositionChange = float3.zero;
        float3 widthChange = float3.zero;

        if (x < 0)
        {
            int rightGap = EmptyLayersAtEdge (0);
            rootPositionChange.x = x;

            widthChange.x = (x * -1) - rightGap;
            if (widthChange.x < 0) widthChange.x = 0;

        }
        else if (x >= width)
        {
            int leftGap = EmptyLayersAtEdge (1);
            widthChange.x = x - (width - 1) - leftGap;

            rootPositionChange.x = leftGap;
        }

        if (y < 0)
        {
            int topGap = EmptyLayersAtEdge (2);
            rootPositionChange.y = y;

            widthChange.y = (y * -1) - topGap;
            if (widthChange.y < 0) widthChange.y = 0;

        }
        else if (y >= width)
        {
            int bottomGap = EmptyLayersAtEdge (3);
            widthChange.y = y - (width - 1) - bottomGap;

            rootPositionChange.y = bottomGap;
        }

        if (z < 0)
        {
            int frontGap = EmptyLayersAtEdge (4);
            rootPositionChange.z = z;

            widthChange.z = (z * -1) - frontGap;
            if (widthChange.z < 0) widthChange.z = 0;
        }
        else if (z >= width)
        {
            int backGap = EmptyLayersAtEdge (5);
            widthChange.z = z - (width - 1) - backGap;

            rootPositionChange.z = backGap;
        }

        rootPositionChange -= 3;
        widthChange += 3;

        int newWidth = width;
        if (widthChange.x + widthChange.y + widthChange.z > 0)
            newWidth += util.maximum ((int) widthChange.x, (int) widthChange.y, (int) widthChange.z);

        int3 rootIndexOffset = new int3 (rootPositionChange) * -1;

        CopyToAdjustedMatrix (rootIndexOffset, newWidth);

        rootPosition += new int3 (rootPositionChange) * itemWorldSize;

        return rootIndexOffset;
    }

    int EmptyLayersAtEdge (int emptyLayersAtEdgeCount)
    {
        int count = 0;

        while (LayerIsEmpty (emptyLayersAtEdgeCount, count))
            count++;

        return count;
    }

    bool LayerIsEmpty (int emptyLayersAtEdgeCount, int offset = 0)
    {
        if (offset >= math.floor (width / 3)) return false;

        if (emptyLayersAtEdgeCount < 2) // getting x axis layers
        {
            int x = emptyLayersAtEdgeCount == 0 ? width - 1 : 0;  // returns either width - 1 for true or 0 for false
            int xOffset = emptyLayersAtEdgeCount == 0 ? -offset : offset;  // - offset for true, offset for false
            for (int y = 0; y < width; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    if (ItemIsSet (MatrixPositionToIndex (new int3 (x + xOffset, y, z))))
                        return false;
                }
            }
        }
        else if (emptyLayersAtEdgeCount > 1 && emptyLayersAtEdgeCount < 4)
        {
            int y = emptyLayersAtEdgeCount == 2 ? width - 1 : 0;
            int yOffset = emptyLayersAtEdgeCount == 2 ? -offset : offset;
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < width; z++)
                {
                    if (ItemIsSet (MatrixPositionToIndex (new int3 (x, y + yOffset, z))))
                        return false;
                }
            }
        }
        else
        {
            int z = emptyLayersAtEdgeCount >= 4 ? width - 1 : 0;
            int zOffset = emptyLayersAtEdgeCount == 4 ? -offset : offset;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    if (ItemIsSet (MatrixPositionToIndex (new int3 (x, y, z + zOffset))))
                        return false;
                }
            }
        }
        return true;
    }

    public void CopyToAdjustedMatrix (int3 rootIndexOffset, int newWidth)
    {
        NativeArray<T> newMatrix = new NativeArray<T> ((int) math.pow (newWidth, 3), label);
        NativeArray<sbyte> newIsSet = new NativeArray<sbyte> (newMatrix.Length, label);

        for (int i = 0; i < matrix.Length; i++)
        {
            int3 oldMatrixPosition = IndexToMatrixPosition (i);
            int3 newMatrixPosition = oldMatrixPosition + rootIndexOffset;

            int newIndex = util.Flatten (newMatrixPosition, newWidth);

            if (newIndex < 0 || newIndex >= newMatrix.Length) continue;

            newMatrix [newIndex] = matrix [i];
            newIsSet [newIndex] = isSet [i];
        }
        width = newWidth;

        Dispose ();
        matrix = newMatrix;
        isSet = newIsSet;
    }
    #endregion

    #region Index, WorldPos and MatrixPos calcs

    // worldpos, matrixpos and index calcs
    public int WorldPositionToIndex (int3 worldPosition)
    {
        return MatrixPositionToIndex (WorldToMatrixPosition (worldPosition));
    }

    public int3 WorldToMatrixPosition (int3 worldPosition)
    {
        return (worldPosition - rootPosition) / itemWorldSize;
    }

    public int MatrixPositionToIndex (int3 matrixPosition)
    {
        return util.Flatten (matrixPosition, width);
    }

    public int3 IndexToWorldPosition (int index)
    {
        return MatrixToWorldPosition (IndexToMatrixPosition (index));
    }

    public int3 IndexToMatrixPosition (int index)
    {
        return util.UnflattenToInt3 (index, width);
    }

    public int3 MatrixToWorldPosition (int3 matrixPosition)
    {
        return (matrixPosition * itemWorldSize) + rootPosition;
    }
    #endregion
}