using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace TerrainGeneration
{
    [CreateInWorld("TerrainGenerationWorld")]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(SurfaceHeightSystem))]
    public class GeologySystem : JobComponentSystem
    {
        TerrainGenerationBuffer tGBuffer;
        EntityQuery myQuery;
        NativeQueue<Entity> EntitiesForTagChange;


        protected override void OnCreateManager()
        {
            tGBuffer = World.GetOrCreateSystem<TerrainGenerationBuffer>();
            myQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Voxel), typeof(SurfaceTopography), typeof(GetVoxelGeology) },
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
            new RemoveGetVoxelGeologyTagJob
            {
                EntitiesForTagChange = EntitiesForTagChange,
                ECBuffer = tGBuffer.CreateCommandBuffer()

            }.Schedule(new GetGeologyJob
            {
                EntitiesForTagChange = EntitiesForTagChange.ToConcurrent(),
                DirtLayerThickness = TerrainSettings.dirtLayerThickness,
                VoxelGeographyBufferFromVoxelEntity = GetBufferFromEntity<VoxelGeology>(false)

            }.Schedule(myQuery, inputDeps));

            tGBuffer.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}

[BurstCompile]
[RequireComponentTag(typeof(GetVoxelGeology))]
public struct GetGeologyJob : IJobForEachWithEntity<Voxel, SurfaceTopography>
{
    public NativeQueue<Entity>.Concurrent EntitiesForTagChange;
    public int DirtLayerThickness;

    [NativeDisableParallelForRestriction]
    public BufferFromEntity<VoxelGeology> VoxelGeographyBufferFromVoxelEntity;

    public void Execute(Entity entity, int index, ref Voxel voxel, ref SurfaceTopography surfaceTopography)
    {
        DynamicBuffer<VoxelGeology> voxelGeologyBuffer = VoxelGeographyBufferFromVoxelEntity[entity];
        if (voxelGeologyBuffer.Length < 1)
            voxelGeologyBuffer.ResizeUninitialized(1);

        float3 voxelPos = voxel.WorldPosition;
        float SurfaceHeight = surfaceTopography.Height;
        ushort geologyID;

        if (voxelPos.y < 5)
        {
            geologyID = (ushort)TextureAtlasSettings.ID.BEDROCK;
        }
        else if (voxelPos.y < SurfaceHeight - DirtLayerThickness)
        {
            geologyID = (ushort)TextureAtlasSettings.ID.GRANITE;
        }
        else if (voxelPos.y >= SurfaceHeight - DirtLayerThickness && voxelPos.y < SurfaceHeight)
        {
            geologyID = (ushort)TextureAtlasSettings.ID.MUD;
        }
        else if (voxelPos.y == SurfaceHeight)
        {
            geologyID = (ushort)TextureAtlasSettings.ID.GRASS;
        }
        else
        {
            geologyID = (ushort)TextureAtlasSettings.ID.AIR;
        }
        voxel.GeologyID = geologyID;

        VoxelGeology newGeology = new VoxelGeology { ID = geologyID };
        voxelGeologyBuffer[0] = newGeology;

        EntitiesForTagChange.Enqueue(entity);
    }
}

// Not burstable
[RequireComponentTag(typeof(GetVoxelGeology))]
public struct RemoveGetVoxelGeologyTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForTagChange;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagChange.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GetVoxelGeology));
            ECBuffer.AddComponent(myEntity, new GetAdjacentVoxels());
        }
    }
}