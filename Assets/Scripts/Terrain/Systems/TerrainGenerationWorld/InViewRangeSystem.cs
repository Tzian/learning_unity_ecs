using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TerrainGeneration
{
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(TerrainGenerationStartSystem))]
    public class InViewRangeSystem : JobComponentSystem
    {
        TerrainGenerationBuffer tGBuffer;
        EntityManager tGEntityManager;
        public Entity playerEntity;
        Util util;
        int range;
        EntityQuery myQuery;
        NativeMultiHashMap<Entity, Voxel> myHashMap;
        NativeQueue<Entity>  tagRemovalQueue;
        NativeQueue<Entity> applyinDrawRangeTag;
        NativeQueue<Entity> applyInDrawRangeBufferZoneTag;

        protected override void OnCreateManager()
        {
            playerEntity = Bootstrapped.playerEntity;
            tGEntityManager = World.EntityManager;   
            util = new Util();
            range = TerrainSettings.areaGenerationRange;
            tGBuffer = World.GetOrCreateSystem<TerrainGenerationBuffer>();
            myQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Voxel), typeof(GetVoxelDrawRange), typeof(Translation) },
            });
        }

        protected void NativeCleanUp()
        {
            if (tagRemovalQueue.IsCreated)
            {
                tagRemovalQueue.Dispose();
            }
            if (applyinDrawRangeTag.IsCreated)
            {
                applyinDrawRangeTag.Dispose();
            }
            if (applyInDrawRangeBufferZoneTag.IsCreated)
            {
                applyInDrawRangeBufferZoneTag.Dispose();
            }
            if (tagRemovalQueue.IsCreated)
            {
                tagRemovalQueue.Dispose();
            }
            if (myHashMap.IsCreated)
            {
                myHashMap.Dispose();
            }
        }

        protected override void OnStopRunning()
        {
            World.EntityManager.CompleteAllJobs();
            NativeCleanUp();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeCleanUp();

            tagRemovalQueue = new NativeQueue<Entity>(Allocator.TempJob);
            applyinDrawRangeTag = new NativeQueue<Entity>(Allocator.TempJob);
            applyInDrawRangeBufferZoneTag = new NativeQueue<Entity>(Allocator.TempJob);

            myHashMap = new NativeMultiHashMap<Entity, Voxel>(myQuery.CalculateLength(), Allocator.TempJob);

            var handle = 
            new ApplyDrawRangeTagChangesJob
            {
                EntitiesForTagRemoval = tagRemovalQueue,
                ApplyInDrawRangeTag = applyinDrawRangeTag,
                ApplyInDrawRangeBufferZoneTag = applyInDrawRangeBufferZoneTag,
                ECBuffer = tGBuffer.CreateCommandBuffer()

            }.Schedule(new VoxelDrawRangeJob
            {
                MyHashMap = myHashMap.ToConcurrent(),
                TagRemovalQueue = tagRemovalQueue.ToConcurrent(),
                ApplyInDrawRangeTag = applyinDrawRangeTag.ToConcurrent(),
                ApplyInDrawRangeBufferZoneTag = applyInDrawRangeBufferZoneTag.ToConcurrent(),
                Util = util,
                Range = range,
                PlayersCurrentPosition = GetPlayersCurrentPosition()

            }.Schedule(myQuery, inputDeps));

            tGBuffer.AddJobHandleForProducer(handle);
            return handle;
        }

        public int3 GetPlayersCurrentPosition()
        {
            EntityManager eM = Bootstrapped.defaultWorld.EntityManager;
            int3 playerPosition = (int3)eM.GetComponentData<Translation>(playerEntity).Value;
            return playerPosition;
        }
    }
}

[BurstCompile]
[RequireComponentTag(typeof(GetVoxelDrawRange))]
public struct VoxelDrawRangeJob :  IJobForEachWithEntity  <Voxel>
{
    public NativeMultiHashMap<Entity, Voxel>.Concurrent MyHashMap;
    public NativeQueue<Entity>.Concurrent TagRemovalQueue;
    public NativeQueue<Entity>.Concurrent ApplyInDrawRangeTag;
    public NativeQueue<Entity>.Concurrent ApplyInDrawRangeBufferZoneTag;
    [ReadOnly] public Util Util;
    [ReadOnly] public int Range;
    [ReadOnly] public int3 PlayersCurrentPosition;

    public void Execute(Entity entity, int index, ref Voxel voxel)
    {
        MyHashMap.Add(entity, voxel);
        int3 position = (int3)voxel.WorldPosition;

        if (Util.MatrixInRangeFromWorldPositionCheck(PlayersCurrentPosition, position, PlayersCurrentPosition, Range - 4, 1))
        {
            ApplyInDrawRangeTag.Enqueue(entity);
        }
        else if (Util.MatrixInRangeFromWorldPositionCheck(PlayersCurrentPosition, position, PlayersCurrentPosition, Range - 2, 1))
        {
            ApplyInDrawRangeBufferZoneTag.Enqueue(entity);
        }
        TagRemovalQueue.Enqueue(entity);
    }
}

// Not burstable
public struct ApplyDrawRangeTagChangesJob : IJob
{
    public NativeQueue<Entity> ApplyInDrawRangeTag;
    public NativeQueue<Entity> ApplyInDrawRangeBufferZoneTag;
    public NativeQueue<Entity> EntitiesForTagRemoval;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagRemoval.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GetVoxelDrawRange));
        }
        while (ApplyInDrawRangeTag.TryDequeue(out Entity iDREntity))
        {
            ECBuffer.AddComponent(iDREntity, new VoxelIsInDrawRange());
        }
        while (ApplyInDrawRangeBufferZoneTag.TryDequeue(out Entity bZEntity))
        {
            ECBuffer.AddComponent(bZEntity, new VoxelIsInDrawBufferZone());
        }
    }
}