using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TerrainGeneration
{
    [CreateInWorld("TerrainGenerationWorld")]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(FaceCullingSystem))]
    public class GenerateMeshDataSystem : JobComponentSystem
    {
        TerrainGenerationBuffer tGBuffer;
        EntityQuery myQuery;
        NativeQueue<Entity> EntitiesForTagChange;


        protected override void OnCreateManager()
        {
            tGBuffer = World.GetOrCreateSystem<TerrainGenerationBuffer>();
            myQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Voxel), typeof(GetMeshData), typeof(Verts), typeof(VoxelVisibleFaces) },
            });
        }

        protected void NativeCleanUp()
        {
            if (EntitiesForTagChange.IsCreated)
            {
                EntitiesForTagChange.Dispose();
            }
        }

        protected override void OnStopRunning()
        {
            NativeCleanUp();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeCleanUp();

            EntitiesForTagChange = new NativeQueue<Entity>(Allocator.TempJob);

            var handle =
            new RemoveGetMeshDataTagJob
            {
                EntitiesForTagChange = EntitiesForTagChange,
                ECBuffer = tGBuffer.CreateCommandBuffer()

            }.Schedule(new GetVoxelMeshData
            {
                EntitiesForTagChange = EntitiesForTagChange.ToConcurrent(),
                VertBufferFromVoxelEntity = GetBufferFromEntity<Verts>(false),
                TrisBufferFromVoxelEntity = GetBufferFromEntity<Tris>(false),
                UvsBufferFromVoxelEntity = GetBufferFromEntity<Uvs>(false)

            }.Schedule(myQuery, inputDeps));

            tGBuffer.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}

[BurstCompile]
[RequireComponentTag(typeof(GetMeshData))]
public struct GetVoxelMeshData : IJobForEachWithEntity<Voxel, VoxelVisibleFaces>
{
    public NativeQueue<Entity>.Concurrent EntitiesForTagChange;
    [NativeDisableParallelForRestriction]
    public BufferFromEntity<Verts> VertBufferFromVoxelEntity;
    [NativeDisableParallelForRestriction]
    public BufferFromEntity<Tris> TrisBufferFromVoxelEntity;
    [NativeDisableParallelForRestriction]
    public BufferFromEntity<Uvs> UvsBufferFromVoxelEntity;

    public void Execute(Entity entity, int index, ref Voxel voxel, ref VoxelVisibleFaces visibleFaces)
    {
        if (visibleFaces.faceCount == 0)
        {
            return;
        }
        else
        {
            DynamicBuffer<Verts> vertsBuffer = VertBufferFromVoxelEntity[entity];
            if (vertsBuffer.Length < visibleFaces.vertexCount)
            {
                vertsBuffer.ResizeUninitialized(visibleFaces.vertexCount);
            }
            DynamicBuffer<Tris> trisBuffer = TrisBufferFromVoxelEntity[entity];
            if (trisBuffer.Length < visibleFaces.triangleCount)
            {
                trisBuffer.ResizeUninitialized(visibleFaces.triangleCount);
            }
            DynamicBuffer<Uvs> uvsBuffer = UvsBufferFromVoxelEntity[entity];
            if (uvsBuffer.Length < visibleFaces.uvsCount)
            {
                uvsBuffer.ResizeUninitialized(visibleFaces.uvsCount);
            }

            MeshGenerator meshGenerator = new MeshGenerator
            {
                vertices = vertsBuffer,
                triangles = trisBuffer,
                uvs = uvsBuffer,
                texAtlasSettings = new TextureAtlasSettings(),
                baseVerts = new CubeVertices(true),
                visibleFaces = visibleFaces,
                voxel = voxel

            };
            meshGenerator.Execute();
        }
        EntitiesForTagChange.Enqueue(entity);
    }
}

// Not burstable
[RequireComponentTag(typeof(GetMeshData))]
public struct RemoveGetMeshDataTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForTagChange;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagChange.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GetMeshData));
            ECBuffer.AddComponent(myEntity, new ReadyToMesh());
        }
    }
}