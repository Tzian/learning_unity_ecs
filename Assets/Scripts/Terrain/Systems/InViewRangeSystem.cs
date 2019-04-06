using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TerrainGeneration
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(TerrainGenerationStartSystem))]
    public class InViewRangeSystem : JobComponentSystem
    {
        EntityManager tGEntityManager;
        public Entity playerEntity;
        Util util;
        int range;

        protected override void OnCreateManager()
        {
            playerEntity = Bootstrapped.playerEntity;
            tGEntityManager = World.GetOrCreateManager<EntityManager>();
            util = new Util();
            range = TerrainSettings.areaGenerationRange;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var tagRemovalQueue = new NativeQueue<Entity>(Allocator.TempJob);
            var applyinDrawRangeTag = new NativeQueue<Entity>(Allocator.TempJob);
            var applyInDrawRangeBufferZoneTag = new NativeQueue<Entity>(Allocator.TempJob);


            new ApplyDrawRangeTagChangesJob
            {
                EntitiesForTagRemoval = tagRemovalQueue,
                ApplyInDrawRangeTag = applyinDrawRangeTag,
                ApplyInDrawRangeBufferZoneTag = applyInDrawRangeBufferZoneTag,
                ECBuffer = eCBuffer

            }.Schedule(new VoxelDrawRangeJob
            {
                TagRemovalQueue = tagRemovalQueue.ToConcurrent(),
                ApplyInDrawRangeTag = applyinDrawRangeTag.ToConcurrent(),
                ApplyInDrawRangeBufferZoneTag = applyInDrawRangeBufferZoneTag.ToConcurrent(),
                Util = util,
                Range = range,
                PlayersCurrentPosition = GetPlayersCurrentPosition()

            }.Schedule(this, inputDeps)).Complete();
            eCBuffer.Playback(tGEntityManager);
            eCBuffer.Dispose();
            tagRemovalQueue.Dispose();
            applyinDrawRangeTag.Dispose();
            applyInDrawRangeBufferZoneTag.Dispose();
            return inputDeps;
        }

        public int3 GetPlayersCurrentPosition()
        {
            EntityManager eM = Bootstrapped.defaultWorld.GetExistingManager<EntityManager>();
            int3 playerPosition = (int3)eM.GetComponentData<Translation>(playerEntity).Value;
            return playerPosition;
        }

    }
}


[BurstCompile]
[RequireComponentTag(typeof(GetVoxelDrawRange))]
public struct VoxelDrawRangeJob : IJobProcessComponentDataWithEntity<Voxel, Translation>
{
    public NativeQueue<Entity>.Concurrent TagRemovalQueue;
    public NativeQueue<Entity>.Concurrent ApplyInDrawRangeTag;
    public NativeQueue<Entity>.Concurrent ApplyInDrawRangeBufferZoneTag;
    [ReadOnly] public Util Util;
    [ReadOnly] public int Range;
    [ReadOnly] public int3 PlayersCurrentPosition;

    public void Execute(Entity entity, int index, ref Voxel voxel, ref Translation translation)
    {
        int3 position = (int3)translation.Value;

        if (Util.SectorMatrixInRangeFromWorldPositionCheck(PlayersCurrentPosition, position, PlayersCurrentPosition, Range - 4, 1))
        {
            ApplyInDrawRangeTag.Enqueue(entity);
        }
        else if (Util.SectorMatrixInRangeFromWorldPositionCheck(PlayersCurrentPosition, position, PlayersCurrentPosition, Range - 2, 1))
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