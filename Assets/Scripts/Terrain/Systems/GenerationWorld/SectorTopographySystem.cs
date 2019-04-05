using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(SectorSurfaceNoiseSystem))]
    public class SectorTopographySystem : JobComponentSystem
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

            new RemoveGetSectorTopographyTagJob
            {
                EntitiesForTagRemoval = entitiesForTagRemoval,
                ECBuffer = eCBuffer

            }.Schedule(new GetStartingTopograpnyJob
            {
                EntitiesForTagRemoval = entitiesForTagRemoval.ToConcurrent(),
                SurfaceNoiseBufferFrom = GetBufferFromEntity<WorleySurfaceNoise>(true),
                TopographyBufferFrom = GetBufferFromEntity<Topography>(false),
                TTUtil = new TopographyTypeUtil(),
                Util = new Util(),
                SectorSize = sectorSize,
                MinSurfaceHeight = TerrainSettings.minWorldHeight,
                MaxSurfaceHeight = TerrainSettings.maxWorldHeight

            }.Schedule(this, inputDeps)).Complete();

            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();
            entitiesForTagRemoval.Dispose();

            return inputDeps;
        }
    }
}