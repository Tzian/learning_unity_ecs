using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public struct BlockFaceChecker
{
    public NativeArray<BlockFaces> exposedFaces;

    //	Block data for this and adjacent map squares
    public NativeArray<Block> current;
    public NativeArray<Block> northNeighbour;
    public NativeArray<Block> southNeighbour;
    public NativeArray<Block> eastNeighbour;
    public NativeArray<Block> westNeighbour;
    public NativeArray<Block> upNeighbour;
    public NativeArray<Block> downNeighbour;

    public int sectorSize;
    public NativeArray<float3> directions;
    public TextureAtlasSettings texAtlasSettings;
    public Util util;

    public void Execute(int index)
    {
        BlockFaces blockFaces = new BlockFaces();

        Block block = current[index];
        blockFaces.atlasID = block.atlasID;

        if (blockFaces.atlasID == 1) // this block is AIR, so no faces are needing to be rendered
        {
            blockFaces.north = 0;  // its an air block so never need side rendered
            blockFaces.south = 0;
            blockFaces.east = 0;
            blockFaces.west = 0;
            blockFaces.up = 0;
            blockFaces.down = 0;
        }
        else
        {
            for (int f = 0; f < 6; f++)
            {
                // gets local position of neighbouring block in the direction of f
                float3 adjacentBlocksPos = block.localPosition + directions[f];

                ushort adjBlocksAtlasID = GetBlocksAtlasID(adjacentBlocksPos);

                byte transparency = texAtlasSettings.IsTransparent(adjBlocksAtlasID);  // get transparency for this block, 1 = transparent, 0 for solid

                //Debug.Log ("adjblocksID " + adjBlocksAtlasID + "transparency " + transparency);

                // if transparency of adjacent block is 1, then we need this block to render a face in that direction
                // below sets our blocks face to 1 for i need a face

                blockFaces[f] = transparency;

                //switch (f)
                //{
                //    case 0:
                //        blockFaces.northID = adjBlocksAtlasID;
                //        break;
                //    case 1:
                //        blockFaces.southID = adjBlocksAtlasID;
                //        break;
                //    case 2:
                //        blockFaces.eastID = adjBlocksAtlasID;
                //        break;
                //    case 3:
                //        blockFaces.westID = adjBlocksAtlasID;
                //        break;
                //    case 4:
                //        blockFaces.upID = adjBlocksAtlasID;
                //        break;
                //    case 5:
                //        blockFaces.downID = adjBlocksAtlasID;
                //        break;
                //}
            }
        }

        blockFaces.SetCount();

        //Debug.Log (" faces " + faces.count + " north " + faces.northID + " south " + faces.southID + " east " + faces.eastID + " west " + faces.westID + " up " + faces.upID + " down " + faces.downID);

        exposedFaces[index] = blockFaces;
    }

    ushort GetBlocksAtlasID(float3 adjPos)
    {
        // if axis pos == sectorSize then 1, if axis pos < 0 then -1, otherwise returns 0
        float3 edge = util.EdgeOverlap3D(adjPos, sectorSize);

        if (edge.z > 0)
        {
            ushort northN = northNeighbour[AdjacentBlockIndex(adjPos)].atlasID;
            return northN;
        }
        else if (edge.z < 0)
        {
            ushort southN = southNeighbour[AdjacentBlockIndex(adjPos)].atlasID;
            return southN;
        }
        else if (edge.x > 0)
        {
            ushort eastN = eastNeighbour[AdjacentBlockIndex(adjPos)].atlasID;
            return eastN;
        }
        else if (edge.x < 0)
        {
            ushort westN = westNeighbour[AdjacentBlockIndex(adjPos)].atlasID;
            return westN;
        }
        else if (edge.y > 0)
        {
            ushort upN = upNeighbour[AdjacentBlockIndex(adjPos)].atlasID;
            return upN;
        }
        else if (edge.y < 0)
        {
            ushort downN = downNeighbour[AdjacentBlockIndex(adjPos)].atlasID;
            return downN;
        }
        else
        {
            ushort blockID = current[util.Flatten(adjPos.x, adjPos.y, adjPos.z, sectorSize)].atlasID;
            return blockID;
        }
    }

    int AdjacentBlockIndex(float3 adjPos)
    {
        return util.WrapAndFlatten(new int3((int)adjPos.x, (int)adjPos.y, (int)adjPos.z), sectorSize);
    }

}
