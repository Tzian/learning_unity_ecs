using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TerrainGeneration
{
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(TerrainGenerationStartSystem))]
    public class SurfaceNoiseSystem : JobComponentSystem
    {
        TerrainGenerationBuffer tGBuffer;
        WorleyNoiseGenerator worleyNoiseGenerator;

        EntityQuery myQuery;
        NativeQueue<Entity> tagRemovalQueue;


        protected override void OnCreateManager()
        {
            tGBuffer = World.GetOrCreateSystem<TerrainGenerationBuffer>();
            worleyNoiseGenerator = new WorleyNoiseGenerator
            (
                TerrainSettings.seed, TerrainSettings.cellFrequency, TerrainSettings.perterbAmp, TerrainSettings.cellularJitter
            );
            myQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Voxel), typeof(SurfaceTopography), typeof(GetVoxelDrawRange) },
            });
        }

        protected void NativeCleanUp()
        {
            if (tagRemovalQueue.IsCreated)
            {
                tagRemovalQueue.Dispose();
            }
        }

        protected override void OnStopRunning()
        {
            World.EntityManager.CompleteAllJobs();
            NativeCleanUp();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            tagRemovalQueue = new NativeQueue<Entity>(Allocator.TempJob);

            var handle =
            new RemoveGetSurfaceTopographyTagJob
            {
                TagRemovalQueue = tagRemovalQueue,
                ECBuffer = tGBuffer.CreateCommandBuffer()

            }.Schedule(new SurfaceNoiseJob
            {
                TagRemovalQueue = tagRemovalQueue.ToConcurrent(),
                TTUtil = new TopographyTypeUtil(),
                Noise = worleyNoiseGenerator

            }.Schedule(myQuery, inputDeps));

            tGBuffer.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}

[BurstCompile]
[RequireComponentTag(typeof(GetSurfaceTopography))]
public struct SurfaceNoiseJob : IJobForEachWithEntity<Voxel, SurfaceTopography>
{
    [ReadOnly] public WorleyNoiseGenerator Noise;
    public NativeQueue<Entity>.Concurrent TagRemovalQueue;
    public TopographyTypeUtil TTUtil;

    public void Execute(Entity entity, int index, ref Voxel voxel, ref SurfaceTopography surfaceTopography)
    {
        WorleySurfaceNoise surfaceNoise = Noise.GetEdgeData(voxel.WorldPosition.x, voxel.WorldPosition.z);
        surfaceTopography.Type = (int)TTUtil.TerrainType(surfaceNoise.currentSurfaceCellValue);
        TagRemovalQueue.Enqueue(entity);
    }
}

// Not burstable
public struct RemoveGetSurfaceTopographyTagJob : IJob
{
    public NativeQueue<Entity> TagRemovalQueue;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (TagRemovalQueue.TryDequeue(out Entity entity))
        {
            ECBuffer.RemoveComponent(entity, typeof(GetSurfaceTopography));
        }
    }
}