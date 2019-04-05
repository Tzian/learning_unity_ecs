using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;


namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(SectorDrawRangeSystem))]
    public class SectorDrawRangeTagSystem : JobComponentSystem
    {
        EntityManager entityManager;

        protected override void OnCreateManager()
        {
            entityManager = World.GetOrCreateManager<EntityManager>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);

            var entitiesForApplyInnerDrawRangeTag = new NativeQueue<Entity>(Allocator.TempJob);
            var entitiesForApplyOuterDrawRangeTag = new NativeQueue<Entity>(Allocator.TempJob);
            var entitiesForApplyEdgeDrawRangeTag = new NativeQueue<Entity>(Allocator.TempJob);
            var entitiesForApplyNotInDrawRangeTag = new NativeQueue<Entity>(Allocator.TempJob);

            new ApplyNewSectorDrawRangeTagJob
            {
                EntitiesForApplyInnerDrawRangeTag = entitiesForApplyInnerDrawRangeTag,
                EntitiesForApplyOuterDrawRangeTag = entitiesForApplyOuterDrawRangeTag,
                EntitiesForApplyEdgeDrawRangeTag = entitiesForApplyEdgeDrawRangeTag,
                EntitiesForApplyNotInDrawRangeTag = entitiesForApplyNotInDrawRangeTag,
                ECBuffer = eCBuffer

            }.Schedule(new SectorDrawRangeTagJob
            {
                EntitiesForApplyInnerDrawRangeTag = entitiesForApplyInnerDrawRangeTag.ToConcurrent(),
                EntitiesForApplyOuterDrawRangeTag = entitiesForApplyOuterDrawRangeTag.ToConcurrent(),
                EntitiesForApplyEdgeDrawRangeTag = entitiesForApplyEdgeDrawRangeTag.ToConcurrent(),
                EntitiesForApplyNotInDrawRangeTag = entitiesForApplyNotInDrawRangeTag.ToConcurrent(),
            }.Schedule(this, inputDeps)).Complete();

            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();
            entitiesForApplyInnerDrawRangeTag.Dispose();
            entitiesForApplyOuterDrawRangeTag.Dispose();
            entitiesForApplyEdgeDrawRangeTag.Dispose();
            entitiesForApplyNotInDrawRangeTag.Dispose();
            return inputDeps;
        }
    }
}

