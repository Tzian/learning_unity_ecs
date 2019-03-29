using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public struct MeshDataJob : IJob
{
    public EntityCommandBuffer ECBuffer;

    [ReadOnly] public Entity entity;
    [ReadOnly] public SectorVisFacesCount counts;
    [ReadOnly] public Sector sector;

    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Block> blocks;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<BlockFaces> blockFaces;

    public void Execute()
    {
        if(counts.vertCount > 0)
        {

        }
        DynamicBuffer<MeshVert> vertBuffer = ECBuffer.AddBuffer<MeshVert>(entity);
        vertBuffer.ResizeUninitialized(counts.vertCount);
        DynamicBuffer<MeshUv> uvBuffer = ECBuffer.AddBuffer<MeshUv>(entity);
        uvBuffer.ResizeUninitialized(counts.vertCount);

        DynamicBuffer<MeshTri> triBuffer = ECBuffer.AddBuffer<MeshTri>(entity);
        triBuffer.ResizeUninitialized(counts.triCount);

        MeshGenerator meshGenerator = new MeshGenerator
        {
            vertices = vertBuffer,
            triangles = triBuffer,
            uvs = uvBuffer,
            
            sector = sector,
            sectorSize = TerrainSettings.sectorSize,

            blocks = blocks,
            blockFaces = blockFaces,

            util = new Util(),
            texAtlasSettings = new TextureAtlasSettings(),

            baseVerts = new CubeVertices(true)
        };

        for (int i = 0; i < blocks.Length; i++)
        {
            meshGenerator.Execute(i);
        }

        ECBuffer.RemoveComponent<SectorVisFacesCount>(entity);
        ECBuffer.RemoveComponent<BlockFaces>(entity);
    }
}