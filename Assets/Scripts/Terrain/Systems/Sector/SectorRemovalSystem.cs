using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;


public class SectorRemovalSystem : JobComponentSystem
{
    EntityManager entityManager;

    protected override void OnCreateManager ()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
    }

    protected override JobHandle OnUpdate (JobHandle inputDeps)
    {
        EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);

        var removeSectorJob = new RemoveSectorJob
        {
            ECBuffer = eCBuffer,

        }.Schedule (this, inputDeps);
        removeSectorJob.Complete();

        eCBuffer.Playback(entityManager);
        eCBuffer.Dispose();

        return removeSectorJob;
    }
}