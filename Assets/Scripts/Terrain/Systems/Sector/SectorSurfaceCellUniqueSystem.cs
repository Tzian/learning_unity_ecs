using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class SectorSurfaceCellUniqueSystem : JobComponentSystem
{
    EntityManager entityManager;
    int sectorSize;

    ChunkIterationSystem chunkIterationSystem;
    ComponentGroup compGroup;

    ArchetypeChunkEntityType entityType;
    NativeArray<Entity> entities = new NativeArray<Entity>();

    protected override void OnCreateManager()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        sectorSize = TerrainSettings.sectorSize;

        chunkIterationSystem = new ChunkIterationSystem();

        EntityArchetypeQuery query = new EntityArchetypeQuery
        {
            Any = Array.Empty<ComponentType>(),
            None = new ComponentType[] { typeof(GetSectorNoise) },
            All = new ComponentType[] { typeof(GetUniqueSurfaceCells), typeof (Sector) }
        };
        compGroup = GetComponentGroup(query);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.Temp);

        entityType = GetArchetypeChunkEntityType();
        entities = chunkIterationSystem.GetEntities(compGroup, entityType);

        NativeList<SurfaceCell> uniqueCellList = new NativeList<SurfaceCell>((int)math.pow(sectorSize, 2), Allocator.TempJob);
        NativeQueue<Entity> entitiesForTagRemoval = new NativeQueue<Entity>(Allocator.TempJob);

        var getUniqueSurfaceCellsJob = new GetUniqueSurfaceCellsJob
        {
            Entities = entities,
            SurfaceCellBufferFrom = GetBufferFromEntity<SurfaceCell>(false),
            UniqueCellList = uniqueCellList,
            EntitiesForTagRemoval = entitiesForTagRemoval

        }.Schedule(inputDeps);
        getUniqueSurfaceCellsJob.Complete();
        uniqueCellList.Dispose();

        for (int i = 0; i < entitiesForTagRemoval.Count; i++)
        {
            Entity sectorEntity = entitiesForTagRemoval.Dequeue();
            eCBuffer.RemoveComponent(sectorEntity, typeof(GetUniqueSurfaceCells));
        }
        entitiesForTagRemoval.Dispose();

        entities.Dispose();

        eCBuffer.Playback(entityManager);
        eCBuffer.Dispose();

        return getUniqueSurfaceCellsJob;
    }
}