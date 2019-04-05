using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(SectorTopographySystem))]
    public class SectorGeologySystem : JobComponentSystem
    {
        EntityManager entityManager;
        int sectorSize;

        protected override void OnCreateManager()
        {
            entityManager = World.GetOrCreateManager<EntityManager>();
            sectorSize = TerrainSettings.sectorSize;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);

            NativeQueue<Entity> entitiesForTagRemoval = new NativeQueue<Entity>(Allocator.TempJob);

            new RemoveGenerateSectorGeologyTagJob
            {
                EntitiesForTagRemoval = entitiesForTagRemoval,
                ECBuffer = eCBuffer

            }.Schedule(new SectorGeologyJob
            {
                EntitiesForTagRemoval = entitiesForTagRemoval.ToConcurrent(),
                BlockBufferFromSectorEntity = GetBufferFromEntity<Block>(false),
                TopographyBufferFromSectorEntity = GetBufferFromEntity<Topography>(true),
                Util = new Util(),
                SectorSize = sectorSize,
                DirtLayerThickness = 3

            }.Schedule(this, inputDeps)).Complete();

            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();
            entitiesForTagRemoval.Dispose();

            return inputDeps;
        }
    }
}