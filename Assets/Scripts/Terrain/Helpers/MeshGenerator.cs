using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;

struct MeshGenerator
{
    public DynamicBuffer<MeshVert> vertices;
    public DynamicBuffer<MeshTri> triangles;
    public DynamicBuffer<MeshUv> uvs;

    public Sector sector;

    public NativeArray<Block> blocks;
    public NativeArray<BlockFaces> blockFaces;

    public Util util;
    public TextureAtlasSettings texAtlasSettings;
    public int sectorSize;

    public CubeVertices baseVerts;

    public void Execute(int i)
    {
        // skip blocks that have no exposed sides
        if (blockFaces[i].count == 0) return;

        // get blocks atlasID
        ushort atlasID = blockFaces[i].atlasID;

        //	Get blocks local position for vertex offset
        float3 positionInMesh = util.UnflattenToFloat3(i, sectorSize);

        //	Current local indices
        int vertIndex = 0;
        int triIndex = 0;
        int uvIndex = 0;

        //	Block starting indices
        int vertOffset = blockFaces[i].vertIndex;
        int triOffset = blockFaces[i].triIndex;
        int uvOffset = blockFaces[i].uvIndex;

        // draw faces for exposed sides
        for (int f = 0; f < 6; f++)
        {
            int blockFaceExposure = blockFaces[i][f];

            if (blockFaceExposure == 0) continue;  // we do not need a face, carry on to next one

            DrawFace(f, triIndex + triOffset, vertIndex + vertOffset, uvIndex + uvOffset, atlasID, positionInMesh, ref triIndex, ref vertIndex, ref uvIndex);
        }
    }

    //	Normal face
    void DrawFace(int face, int triOffset, int vertOffset, int uvOffset, ushort atlasID, float3 positionInMesh, ref int triIndex, ref int vertIndex, ref int uvIndex)
    {
        Triangles(triOffset, vertOffset);
        Vertices(face, positionInMesh, vertOffset);
        Uvs(face, atlasID, uvOffset);
        triIndex += 6;
        vertIndex += 4;
        uvIndex += 4;
    }

    //	Vertices for normal cube
    void Vertices(int side, float3 position, int index)
    {
        switch (side)
        {
            case 0: // North
                vertices[index + 0] = new MeshVert { vertex = baseVerts[4] + position };
                vertices[index + 1] = new MeshVert { vertex = baseVerts[5] + position };
                vertices[index + 2] = new MeshVert { vertex = baseVerts[1] + position };
                vertices[index + 3] = new MeshVert { vertex = baseVerts[0] + position };
                break;
            case 1: // South
                vertices[index + 0] = new MeshVert { vertex = baseVerts[6] + position };
                vertices[index + 1] = new MeshVert { vertex = baseVerts[7] + position };
                vertices[index + 2] = new MeshVert { vertex = baseVerts[3] + position };
                vertices[index + 3] = new MeshVert { vertex = baseVerts[2] + position };
                break;
            case 2: // East
                vertices[index + 0] = new MeshVert { vertex = baseVerts[5] + position };
                vertices[index + 1] = new MeshVert { vertex = baseVerts[6] + position };
                vertices[index + 2] = new MeshVert { vertex = baseVerts[2] + position };
                vertices[index + 3] = new MeshVert { vertex = baseVerts[1] + position };
                break;
            case 3: // West
                vertices[index + 0] = new MeshVert { vertex = baseVerts[7] + position };
                vertices[index + 1] = new MeshVert { vertex = baseVerts[4] + position };
                vertices[index + 2] = new MeshVert { vertex = baseVerts[0] + position };
                vertices[index + 3] = new MeshVert { vertex = baseVerts[3] + position };
                break;
            case 4: // Up
                vertices[index + 0] = new MeshVert { vertex = baseVerts[7] + position };
                vertices[index + 1] = new MeshVert { vertex = baseVerts[6] + position };
                vertices[index + 2] = new MeshVert { vertex = baseVerts[5] + position };
                vertices[index + 3] = new MeshVert { vertex = baseVerts[4] + position };
                break;
            case 5: // Down
                vertices[index + 0] = new MeshVert { vertex = baseVerts[0] + position };
                vertices[index + 1] = new MeshVert { vertex = baseVerts[1] + position };
                vertices[index + 2] = new MeshVert { vertex = baseVerts[2] + position };
                vertices[index + 3] = new MeshVert { vertex = baseVerts[3] + position };
                break;

            default: throw new System.ArgumentOutOfRangeException("Index out of range 5: " + side);
        }
    }

    //	Triangles for normal cube
    void Triangles(int index, int vertIndex)
    {
        triangles[index + 0] = new MeshTri { triangle = 3 + vertIndex };
        triangles[index + 1] = new MeshTri { triangle = 1 + vertIndex };
        triangles[index + 2] = new MeshTri { triangle = 0 + vertIndex };
        triangles[index + 3] = new MeshTri { triangle = 3 + vertIndex };
        triangles[index + 4] = new MeshTri { triangle = 2 + vertIndex };
        triangles[index + 5] = new MeshTri { triangle = 1 + vertIndex };
    }

    // Uvs for normal cube
    void Uvs(int dir, ushort atlasID, int Index)
    {
        if(dir == 4 && atlasID == 0)
        {

        }
        TextureUVHelper uvHelper = texAtlasSettings.GetUVs(atlasID, (TextureAtlasSettings.Dir)dir);

        for (int u = 0; u < 4; u++)
        {
            MeshUv newMeshUV = new MeshUv { uv = uvHelper[u] };
            uvs[Index + u] = newMeshUV;
        }
    }
}