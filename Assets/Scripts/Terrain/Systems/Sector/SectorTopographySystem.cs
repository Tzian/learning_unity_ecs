using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(SectorSurfaceNoiseSystem))]
public class SectorTopographySystem : JobComponentSystem
{
    EntityManager entityManager;
    int sectorSize;

    protected override void OnCreateManager()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        sectorSize = TerrainSettings.sectorSize;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.Temp);

        NativeQueue<Entity> entitiesForTagRemoval = new NativeQueue<Entity>(Allocator.TempJob);

        var sectorTopographyJob = new GetStartingTopograpnyJob
        {
            EntitiesForTagRemoval = entitiesForTagRemoval,

            SurfaceNoiseBufferFrom = GetBufferFromEntity<WorleySurfaceNoise>(true),
            TopographyBufferFrom = GetBufferFromEntity<Topography>(false),
            TTUtil = new TopographyTypeUtil(),
            Util = new Util(),
            SectorSize = sectorSize,
            MinSurfaceHeight = TerrainSettings.minWorldHeight,
            MaxSurfaceHeight = TerrainSettings.maxWorldHeight

        }.Schedule(this, inputDeps);
        sectorTopographyJob.Complete();

        for (int i = 0; i < entitiesForTagRemoval.Count; i++)
        {
            Entity sectorEntity = entitiesForTagRemoval.Dequeue();
            eCBuffer.RemoveComponent(sectorEntity, typeof(GetSectorTopography));
        }
        entitiesForTagRemoval.Dispose();

        eCBuffer.Playback(entityManager);
        eCBuffer.Dispose();

        return sectorTopographyJob;
    }
}