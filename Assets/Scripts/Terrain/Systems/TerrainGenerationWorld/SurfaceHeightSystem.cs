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
    [UpdateAfter(typeof(SurfaceTopographySystem))]
    public class SurfaceHeightSystem : JobComponentSystem
    {
        TerrainGenerationBuffer tGBuffer;
        EntityQuery myQuery;
        NativeQueue<Entity> EntitiesForTagChange;


        protected override void OnCreateManager()
        {
            tGBuffer = World.GetOrCreateSystem<TerrainGenerationBuffer>();
            myQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Voxel), typeof(SurfaceTopography), typeof(GetSurfaceHeight) },
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
            new RemoveGetSurfaceHeightTagJob
            {
                EntitiesForTagChange = EntitiesForTagChange,
                ECBuffer = tGBuffer.CreateCommandBuffer()

            }.Schedule(new GetSurfaceHeightJob
            {
                EntitiesForTagChange = EntitiesForTagChange.ToConcurrent(),
                TTUtil = new TopographyTypeUtil(),

            }.Schedule(myQuery, inputDeps));

            tGBuffer.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}

[BurstCompile]  
[RequireComponentTag(typeof(GetSurfaceHeight))]
public struct GetSurfaceHeightJob : IJobForEachWithEntity<Voxel, SurfaceTopography>
{
    public NativeQueue<Entity>.Concurrent EntitiesForTagChange;
    public TopographyTypeUtil TTUtil;


    public void Execute(Entity entity, int index, ref Voxel voxel, ref SurfaceTopography surfaceTopography)
    {
        float surfaceHeight = TerrainSettings.seaLevel;
        float modifier = GetNoise(surfaceTopography.Type, voxel.WorldPosition);

        surfaceHeight = surfaceHeight + modifier;
        surfaceTopography.Height = surfaceHeight;

        EntitiesForTagChange.Enqueue(entity);
    }

    float GetNoise(int topographyType, float3 worldPosition)
    {
        float modifier = 0;
        TopographyTypeStats topographyStats = TTUtil.GetTerrainTypeStats((TopographyTypes)topographyType);

        modifier = modifier + TTUtil.AddNoise(topographyStats, (int)worldPosition.x, (int)worldPosition.z);

        return modifier;
    }
}

// Not burstable
[RequireComponentTag(typeof(GetSurfaceHeight))]
public struct RemoveGetSurfaceHeightTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForTagChange;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagChange.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GetSurfaceHeight));
            ECBuffer.AddComponent(myEntity, new GetVoxelGeology());
        }
    }
}