using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;


namespace Terrain
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGroup))]
    [UpdateAfter(typeof(TerrainSystem))]
    public class SectorRemovalSystem : JobComponentSystem
    {
        EntityManager entityManager;

        protected override void OnCreateManager()
        {
            entityManager = Bootstrapped.defaultWorld.GetOrCreateManager<EntityManager>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);

            var removeSectorJob = new RemoveSectorJob
            {
                ECBuffer = eCBuffer,

            }.Schedule(this, inputDeps);
            removeSectorJob.Complete();

            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();

            return removeSectorJob;
        }
    }
}