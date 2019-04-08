//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using UnityEngine;

//namespace TerrainGeneration
//{
//    [CreateInWorld("TerrainGenerationWorld")]
//    [UpdateInGroup(typeof(TerrainGenerationGroup))]
//    [UpdateAfter(typeof(SurfaceHeightSystem))]
//    public class GeologySystem : JobComponentSystem
//    {
//        TerrainGenerationBuffer tGBuffer;
//        EntityQuery myQuery;
//        NativeQueue<Entity> EntitiesForTagChange;


//        protected override void OnCreateManager()
//        {
//            tGBuffer = World.GetOrCreateSystem<TerrainGenerationBuffer>();
//            myQuery = GetEntityQuery(new EntityQueryDesc
//            {
//                All = new ComponentType[] { typeof(Voxel), typeof(SurfaceTopography), typeof(GetVoxelGeology) },
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
//            new RemoveGetVoxelGeologyTagJob
//            {
//                EntitiesForTagChange = EntitiesForTagChange,
//                ECBuffer = tGBuffer.CreateCommandBuffer()

//            }.Schedule(new GetGeologyJob
//            {
//                EntitiesForTagChange = EntitiesForTagChange.ToConcurrent(),
//                DirtLayerThickness = TerrainSettings.dirtLayerThickness

//            }.Schedule(myQuery, inputDeps));

//            tGBuffer.AddJobHandleForProducer(handle);
//            return handle;
//        }
//    }
//}

//[BurstCompile]
//[RequireComponentTag(typeof(GetVoxelGeology))]
//public struct GetGeologyJob : IJobForEachWithEntity<Voxel, SurfaceTopography>
//{
//    public NativeQueue<Entity>.Concurrent EntitiesForTagChange;
//    public int DirtLayerThickness;

//    public void Execute(Entity entity, int index, ref Voxel voxel, ref SurfaceTopography surfaceTopography)
//    {
//        float3 voxelPos = voxel.WorldPosition;
//        float SurfaceHeight = surfaceTopography.Height;

//        if (voxelPos.y < 5)
//        {
//            voxel.GeologyID = (ushort)TextureAtlasSettings.ID.BEDROCK;
//        }
//        else if (voxelPos.y < SurfaceHeight - DirtLayerThickness)
//        {
//            voxel.GeologyID = (ushort)TextureAtlasSettings.ID.GRANITE;
//        }
//        else if (voxelPos.y >= SurfaceHeight - DirtLayerThickness && voxelPos.y < SurfaceHeight)
//        {
//            voxel.GeologyID = (ushort)TextureAtlasSettings.ID.MUD;
//        }
//        else if (voxelPos.y == SurfaceHeight)
//        {
//            voxel.GeologyID = (ushort)TextureAtlasSettings.ID.GRASS;
//        }
//        else
//        {
//            voxel.GeologyID = (ushort)TextureAtlasSettings.ID.AIR;
//        }
//        EntitiesForTagChange.Enqueue(entity);
//    }
//}

//// Not burstable
//[RequireComponentTag(typeof(GetVoxelGeology))]
//public struct RemoveGetVoxelGeologyTagJob : IJob
//{
//    public NativeQueue<Entity> EntitiesForTagChange;
//    public EntityCommandBuffer ECBuffer;

//    public void Execute()
//    {
//        while (EntitiesForTagChange.TryDequeue(out Entity myEntity))
//        {
//            ECBuffer.RemoveComponent(myEntity, typeof(GetVoxelGeology));
//            ECBuffer.AddComponent(myEntity, new GetAdjacentVoxels());
//        }
//    }
//}