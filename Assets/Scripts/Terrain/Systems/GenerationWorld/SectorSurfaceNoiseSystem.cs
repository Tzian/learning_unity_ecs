using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;


namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(SectorDrawRangeTagSystem))]
    public class SectorSurfaceNoiseSystem : JobComponentSystem
    {
        EntityManager entityManager;
        Util util;
        WorleyNoiseGenerator worleyNoiseGenerator;
        int sectorSize;

        protected override void OnCreateManager()
        {
            entityManager = World.GetOrCreateManager<EntityManager>();
            sectorSize = TerrainSettings.sectorSize;
            util = new Util();
            worleyNoiseGenerator = new WorleyNoiseGenerator
            (
                TerrainSettings.seed, TerrainSettings.cellFrequency, TerrainSettings.perterbAmp, TerrainSettings.cellularJitter
            );
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);

            NativeQueue<Entity> entitiesForTagRemoval = new NativeQueue<Entity>(Allocator.TempJob);

            var sectorNoiseJob = new SectorNoiseJob
            {
                SurfaceNoiseBufferFrom = GetBufferFromEntity<WorleySurfaceNoise>(false),
                Util = util,
                SectorSize = sectorSize,
                Noise = worleyNoiseGenerator,

            }.Schedule(this, inputDeps);

            new RemoveGetSectorNoiseTagJob
            {
                EntitiesForTagRemoval = entitiesForTagRemoval,
                ECBuffer = eCBuffer
            }.Schedule(new SectorGetIndividualCellsJob
            {
                SectorEntities = entitiesForTagRemoval.ToConcurrent(),
                SurfaceNoiseBufferFrom = GetBufferFromEntity<WorleySurfaceNoise>(true),
                SurfaceCellBufferFrom = GetBufferFromEntity<SurfaceCell>(false)

            }.Schedule(this, sectorNoiseJob)).Complete();
 
            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();
            entitiesForTagRemoval.Dispose();

            return sectorNoiseJob;
        }
    }
}