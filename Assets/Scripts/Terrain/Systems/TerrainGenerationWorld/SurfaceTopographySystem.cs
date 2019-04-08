//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;

//namespace TerrainGeneration
//{
//    [CreateInWorld("TerrainGenerationWorld")]
//    [UpdateInGroup(typeof(TerrainGenerationGroup))]
//    [UpdateAfter(typeof(TerrainGenerationStartSystem))]
//    public class SurfaceTopographySystem : JobComponentSystem
//    {
//        TerrainGenerationBuffer tGBuffer;
//        WorleyNoiseGenerator worleyNoiseGenerator;
//        EntityQuery myQuery;
//        NativeQueue<Entity> EntitiesForTagChange;


//        protected override void OnCreateManager()
//        {
//            tGBuffer = World.GetOrCreateSystem<TerrainGenerationBuffer>();
//            worleyNoiseGenerator = new WorleyNoiseGenerator
//            (
//                TerrainSettings.seed, TerrainSettings.cellFrequency, TerrainSettings.perterbAmp, TerrainSettings.cellularJitter
//            );
//            myQuery = GetEntityQuery(new EntityQueryDesc
//            {
//                All = new ComponentType[] { typeof(Voxel), typeof(SurfaceTopography), typeof(GetSurfaceTopography) },
//            });
//        }

//        protected void NativeCleanUp()
//        {
//            if (EntitiesForTagChange.IsCreated)
//            {
//                EntitiesForTagChange.Dispose();
//            }
//        }

//        protected override void OnStopRunning()
//        {
//            NativeCleanUp();
//        }

//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//            NativeCleanUp();

//            EntitiesForTagChange = new NativeQueue<Entity>(Allocator.TempJob);

//            var handle =
//            new RemoveGetSurfaceTopographyTagJob
//            {
//                EntitiesForTagChange = EntitiesForTagChange,
//                ECBuffer = tGBuffer.CreateCommandBuffer()

//            }.Schedule(new SurfaceNoiseJob
//            {
//                EntitiesForTagChange = EntitiesForTagChange.ToConcurrent(),
//                TTUtil = new TopographyTypeUtil(),
//                Noise = worleyNoiseGenerator

//            }.Schedule(myQuery, inputDeps));

//            tGBuffer.AddJobHandleForProducer(handle);
//            return handle;
//        }
//    }
//}

//[BurstCompile]
//[RequireComponentTag(typeof(GetSurfaceTopography))]
//public struct SurfaceNoiseJob : IJobForEachWithEntity<Voxel, SurfaceTopography>
//{
//    [ReadOnly] public WorleyNoiseGenerator Noise;
//    public NativeQueue<Entity>.Concurrent EntitiesForTagChange;
//    public TopographyTypeUtil TTUtil;

//    public void Execute(Entity entity, int index, ref Voxel voxel, ref SurfaceTopography surfaceTopography)
//    {
//        WorleySurfaceNoise surfaceNoise = Noise.GetEdgeData(voxel.WorldPosition.x, voxel.WorldPosition.z);
//        surfaceTopography.Type = (int)TTUtil.TerrainType(surfaceNoise.currentSurfaceCellValue);
//        EntitiesForTagChange.Enqueue(entity);
//    }
//}

//// Not burstable
//public struct RemoveGetSurfaceTopographyTagJob : IJob
//{
//    public NativeQueue<Entity> EntitiesForTagChange;
//    public EntityCommandBuffer ECBuffer;

//    public void Execute()
//    {
//        while (EntitiesForTagChange.TryDequeue(out Entity myEntity))
//        {
//            ECBuffer.RemoveComponent(myEntity, typeof(GetSurfaceTopography));
//            ECBuffer.AddComponent(myEntity, new GetSurfaceHeight());
//        }
//    }
//}