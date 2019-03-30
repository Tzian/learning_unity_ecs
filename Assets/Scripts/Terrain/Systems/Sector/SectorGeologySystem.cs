using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;


[UpdateAfter(typeof(SectorTopographySystem))]
public class SectorGeologySystem : JobComponentSystem
{
    EntityManager entityManager;
    Util util;
    int sectorSize;

    protected override void OnCreateManager()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        sectorSize = TerrainSettings.sectorSize;
        util = new Util();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.Temp);

        NativeQueue<Entity> entitiesForTagRemoval = new NativeQueue<Entity>(Allocator.TempJob);

        var sectorGeologyJob = new SectorGeologyJob
        {
            EntitiesForTagRemoval = entitiesForTagRemoval,
            BlockBufferFromSectorEntity = GetBufferFromEntity<Block>(false),
            TopographyBufferFromSectorEntity = GetBufferFromEntity<Topography>(true),
            Util = util,
            SectorSize = sectorSize,
            DirtLayerThickness = 3

        }.Schedule(this, inputDeps);
        sectorGeologyJob.Complete();

        for (int i = 0; i < entitiesForTagRemoval.Count; i++)
        {
            Entity sectorEntity = entitiesForTagRemoval.Dequeue();
            eCBuffer.RemoveComponent(sectorEntity, typeof(GenerateSectorGeology));
        }
        entitiesForTagRemoval.Dispose();

        eCBuffer.Playback(entityManager);
        eCBuffer.Dispose();

        return sectorGeologyJob;
    }
}