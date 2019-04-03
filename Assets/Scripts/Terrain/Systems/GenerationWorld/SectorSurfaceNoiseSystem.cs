using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;


namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(SectorRangeTagSystem))]
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
            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.Temp);

            var sectorNoiseJob = new SectorNoiseJob
            {
                SurfaceNoiseBufferFrom = GetBufferFromEntity<WorleySurfaceNoise>(false),
                Util = util,
                SectorSize = sectorSize,
                Noise = worleyNoiseGenerator,

            }.Schedule(this, inputDeps);

            NativeQueue<Entity> sectorEntities = new NativeQueue<Entity>(Allocator.TempJob);

            var sectorIndividualCellsJob = new SectorGetIndividualCellsJob
            {
                SectorEntities = sectorEntities,
                SurfaceNoiseBufferFrom = GetBufferFromEntity<WorleySurfaceNoise>(true),
                SurfaceCellBufferFrom = GetBufferFromEntity<SurfaceCell>(false)

            }.Schedule(this, sectorNoiseJob);
            sectorIndividualCellsJob.Complete();

            for (int i = 0; i < sectorEntities.Count; i++)
            {
                Entity sectorEntity = sectorEntities.Dequeue();
                eCBuffer.RemoveComponent(sectorEntity, typeof(GetSectorNoise));
            }
            sectorEntities.Dispose();

            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();

            return sectorIndividualCellsJob;
        }
    }
}