using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;

struct MeshGenerator
{
    public DynamicBuffer<Verts> vertices;
    public DynamicBuffer<Tris> triangles;
    public DynamicBuffer<Uvs> uvs;
    public Voxel voxel;
    public VoxelVisibleFaces visibleFaces;

    public TextureAtlasSettings texAtlasSettings;

    public CubeVertices baseVerts;

    public void Execute()
    {
        // skip blocks that have no exposed sides
        if (visibleFaces.faceCount == 0) return;

        // get blocks atlasID
        ushort atlasID = voxel.GeologyID;

        //	Get voxel position for vertex offset
        float3 position = voxel.WorldPosition;

        //	Current local indices
        int vertIndex = 0;
        int triIndex = 0;
        int uvIndex = 0;

        // draw faces for exposed sides
        for (int f = 0; f < 6; f++)
        {
            switch (f)
            {
                case 0: 
                    if(visibleFaces.north == 1)
                    {
                        DrawFace(f, triIndex, vertIndex, uvIndex, atlasID, position, ref triIndex, ref vertIndex, ref uvIndex);
                    }
                    break;
                case 1:
                    if (visibleFaces.south == 1)
                    {
                        DrawFace(f, triIndex, vertIndex, uvIndex, atlasID, position, ref triIndex, ref vertIndex, ref uvIndex);
                    }
                    break;
                case 2:
                    if (visibleFaces.east == 1)
                    {
                        DrawFace(f, triIndex, vertIndex, uvIndex, atlasID, position, ref triIndex, ref vertIndex, ref uvIndex);
                    }
                    break;
                case 3:
                    if (visibleFaces.west == 1)
                    {
                        DrawFace(f, triIndex, vertIndex, uvIndex, atlasID, position, ref triIndex, ref vertIndex, ref uvIndex);
                    }
                    break;
                case 4:
                    if (visibleFaces.up == 1)
                    {
                        DrawFace(f, triIndex, vertIndex, uvIndex, atlasID, position, ref triIndex, ref vertIndex, ref uvIndex);
                    }
                    break;
                case 5:
                    if (visibleFaces.down == 1)
                    {
                        DrawFace(f, triIndex, vertIndex, uvIndex, atlasID, position, ref triIndex, ref vertIndex, ref uvIndex);
                    }
                    break;
            }
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
                vertices[index + 0] = new Verts { vertex = baseVerts[4] + position };
                vertices[index + 1] = new Verts { vertex = baseVerts[5] + position };
                vertices[index + 2] = new Verts { vertex = baseVerts[1] + position };
                vertices[index + 3] = new Verts { vertex = baseVerts[0] + position };
                break;
            case 1: // South
                vertices[index + 0] = new Verts { vertex = baseVerts[6] + position };
                vertices[index + 1] = new Verts { vertex = baseVerts[7] + position };
                vertices[index + 2] = new Verts { vertex = baseVerts[3] + position };
                vertices[index + 3] = new Verts { vertex = baseVerts[2] + position };
                break;
            case 2: // East
                vertices[index + 0] = new Verts { vertex = baseVerts[5] + position };
                vertices[index + 1] = new Verts { vertex = baseVerts[6] + position };
                vertices[index + 2] = new Verts { vertex = baseVerts[2] + position };
                vertices[index + 3] = new Verts { vertex = baseVerts[1] + position };
                break;
            case 3: // West
                vertices[index + 0] = new Verts { vertex = baseVerts[7] + position };
                vertices[index + 1] = new Verts { vertex = baseVerts[4] + position };
                vertices[index + 2] = new Verts { vertex = baseVerts[0] + position };
                vertices[index + 3] = new Verts { vertex = baseVerts[3] + position };
                break;
            case 4: // Up
                vertices[index + 0] = new Verts { vertex = baseVerts[7] + position };
                vertices[index + 1] = new Verts { vertex = baseVerts[6] + position };
                vertices[index + 2] = new Verts { vertex = baseVerts[5] + position };
                vertices[index + 3] = new Verts { vertex = baseVerts[4] + position };
                break;
            case 5: // Down
                vertices[index + 0] = new Verts { vertex = baseVerts[0] + position };
                vertices[index + 1] = new Verts { vertex = baseVerts[1] + position };
                vertices[index + 2] = new Verts { vertex = baseVerts[2] + position };
                vertices[index + 3] = new Verts { vertex = baseVerts[3] + position };
                break;

            default: throw new System.ArgumentOutOfRangeException("Index out of range 5: " + side);
        }
    }

    //	Triangles for normal cube
    void Triangles(int index, int vertIndex)
    {
        triangles[index + 0] = new Tris { triangle = 3 + vertIndex };
        triangles[index + 1] = new Tris { triangle = 1 + vertIndex };
        triangles[index + 2] = new Tris { triangle = 0 + vertIndex };
        triangles[index + 3] = new Tris { triangle = 3 + vertIndex };
        triangles[index + 4] = new Tris { triangle = 2 + vertIndex };
        triangles[index + 5] = new Tris { triangle = 1 + vertIndex };
    }

    // Uvs for normal cube
    void Uvs(int dir, ushort atlasID, int Index)
    {
        if (dir == 4 && atlasID == 0)
        {

        }
        TextureUVHelper uvHelper = texAtlasSettings.GetUVs(atlasID, (TextureAtlasSettings.Dir)dir);

        for (int u = 0; u < 4; u++)
        {
            Uvs newUvs = new Uvs { uv = uvHelper[u] };
            uvs[Index + u] = newUvs;
        }
    }
}