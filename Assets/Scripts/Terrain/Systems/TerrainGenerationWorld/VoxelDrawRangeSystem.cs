//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;

//namespace TerrainGeneration
//{
//    [CreateInWorld("TerrainGenerationWorld")]
//    [UpdateInGroup(typeof(TerrainGenerationGroup))]
//    [UpdateAfter(typeof(TerrainGenerationStartSystem))]
//    public class VoxelDrawRangeSystem : JobComponentSystem
//    {
//        TerrainGenerationBuffer tGBuffer;
//        public Entity playerEntity;
//        Util util;
//        int range;
//        EntityQuery myQuery;
//        NativeQueue<Entity>  EntitiesForTagChange;
//        NativeQueue<Entity> applyinDrawRangeTag;
//        NativeQueue<Entity> applyInDrawRangeBufferZoneTag;
//        NativeQueue<Entity> applyNotInDrawRangeTag;

//        protected override void OnCreateManager()
//        {
//            util = new Util();
//            range = TerrainSettings.areaGenerationRange;
//            tGBuffer = World.GetOrCreateSystem<TerrainGenerationBuffer>();
//            myQuery = GetEntityQuery(new EntityQueryDesc
//            {
//                All = new ComponentType[] { typeof(Voxel), typeof(GetVoxelDrawRange) },
//            });
//        }

//        protected override void OnStartRunning()
//        {
//            playerEntity = Bootstrapped.playerEntity;
//        }

//        protected void NativeCleanUp()
//        {
//            if (EntitiesForTagChange.IsCreated)
//            {
//                EntitiesForTagChange.Dispose();
//            }
//            if (applyinDrawRangeTag.IsCreated)
//            {
//                applyinDrawRangeTag.Dispose();
//            }
//            if (applyInDrawRangeBufferZoneTag.IsCreated)
//            {
//                applyInDrawRangeBufferZoneTag.Dispose();
//            }
//            if (applyNotInDrawRangeTag.IsCreated)
//            {
//                applyNotInDrawRangeTag.Dispose();
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
//            applyinDrawRangeTag = new NativeQueue<Entity>(Allocator.TempJob);
//            applyInDrawRangeBufferZoneTag = new NativeQueue<Entity>(Allocator.TempJob);
//            applyNotInDrawRangeTag = new NativeQueue<Entity>(Allocator.TempJob);

//            var handle = 
//            new ApplyDrawRangeTagChangesJob
//            {
//                EntitiesForTagChange = EntitiesForTagChange,
//                ApplyInDrawRangeTag = applyinDrawRangeTag,
//                ApplyInDrawRangeBufferZoneTag = applyInDrawRangeBufferZoneTag,
//                ApplyNotInDrawRangeTag = applyNotInDrawRangeTag,
//                ECBuffer = tGBuffer.CreateCommandBuffer()

//            }.Schedule(new VoxelDrawRangeJob
//            {
//                EntitiesForTagChange = EntitiesForTagChange.ToConcurrent(),
//                ApplyInDrawRangeTag = applyinDrawRangeTag.ToConcurrent(),
//                ApplyInDrawRangeBufferZoneTag = applyInDrawRangeBufferZoneTag.ToConcurrent(),
//                ApplyNotInDrawRangeTag = applyNotInDrawRangeTag.ToConcurrent(),
//                Util = util,
//                Range = range,
//                PlayersCurrentPosition = GetPlayersCurrentPosition()

//            }.Schedule(myQuery, inputDeps));

//            tGBuffer.AddJobHandleForProducer(handle);
//            return handle;
//        }

//        public int3 GetPlayersCurrentPosition()
//        {
            
//            EntityManager eM = Bootstrapped.DefaultWorld.EntityManager;
//            int3 playerPosition = (int3)eM.GetComponentData<Translation>(playerEntity).Value;
//            return playerPosition;
//        }
//    }
//}

//[BurstCompile]
//[RequireComponentTag(typeof(GetVoxelDrawRange))]
//public struct VoxelDrawRangeJob :  IJobForEachWithEntity  <Voxel>
//{
//    public NativeQueue<Entity>.Concurrent EntitiesForTagChange;
//    public NativeQueue<Entity>.Concurrent ApplyInDrawRangeTag;
//    public NativeQueue<Entity>.Concurrent ApplyInDrawRangeBufferZoneTag;
//    public NativeQueue<Entity>.Concurrent ApplyNotInDrawRangeTag;

//    [ReadOnly] public Util Util;
//    [ReadOnly] public int Range;
//    [ReadOnly] public int3 PlayersCurrentPosition;

//    public void Execute(Entity entity, int index, ref Voxel voxel)
//    {
//        int3 position = (int3)voxel.WorldPosition;

//        if (Util.MatrixInRangeFromWorldPositionCheck(PlayersCurrentPosition, position, PlayersCurrentPosition, Range - 4, 1))
//        {
//            ApplyInDrawRangeTag.Enqueue(entity);
//        }
//        else if (Util.MatrixInRangeFromWorldPositionCheck(PlayersCurrentPosition, position, PlayersCurrentPosition, Range - 2, 1))
//        {
//            ApplyInDrawRangeBufferZoneTag.Enqueue(entity);
//        }
//        else
//        {
//            ApplyNotInDrawRangeTag.Enqueue(entity);
//        }
//        EntitiesForTagChange.Enqueue(entity);
//    }
//}

//// Not burstable
//public struct ApplyDrawRangeTagChangesJob : IJob
//{
//    public NativeQueue<Entity> ApplyInDrawRangeTag;
//    public NativeQueue<Entity> ApplyInDrawRangeBufferZoneTag;
//    public NativeQueue<Entity> ApplyNotInDrawRangeTag;
//    public NativeQueue<Entity> EntitiesForTagChange;
//    public EntityCommandBuffer ECBuffer;

//    public void Execute()
//    {
//        while (ApplyInDrawRangeTag.TryDequeue(out Entity iDREntity))
//        {
//            ECBuffer.AddComponent(iDREntity, new VoxelIsInDrawRange());
//        }
//        while (ApplyInDrawRangeBufferZoneTag.TryDequeue(out Entity bZEntity))
//        {
//            ECBuffer.AddComponent(bZEntity, new VoxelIsInDrawBufferZone());
//        }
//        while (ApplyNotInDrawRangeTag.TryDequeue(out Entity nDREntity))
//        {
//            ECBuffer.AddComponent(nDREntity, new VoxelIsNotInDrawRange());
//        }
//        while (EntitiesForTagChange.TryDequeue(out Entity myEntity))
//        {
//            ECBuffer.RemoveComponent(myEntity, typeof(GetVoxelDrawRange));
//            ECBuffer.AddComponent(myEntity, new GetSurfaceTopography());
//        }
//    }
//}