using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;


[UpdateAfter(typeof (SectorFaceCullingSystem))]
public class SectorGenMeshDataSystem : ComponentSystem
{
    EntityManager entityManager;
    ComponentGroup meshDataGroup;

    JobHandle runningJobHandle;
    EntityCommandBuffer runningCommandBuffer;

    protected override void OnCreateManager()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();

        EntityArchetypeQuery meshDataQuery = new EntityArchetypeQuery
        {
            All = new ComponentType[] { typeof(Sector), typeof(SectorVisFacesCount) }
        };
        meshDataGroup = GetComponentGroup(meshDataQuery);

        runningJobHandle = new JobHandle();
        runningCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
    }

    protected override void OnDestroyManager()
    {
        if (runningCommandBuffer.IsCreated) runningCommandBuffer.Dispose();
    }

    protected override void OnUpdate()
    {
        if (!runningJobHandle.IsCompleted)
            return;

        JobCompleteAndBufferPlayback();
        ScheduleMoreJobs();
    }

    void JobCompleteAndBufferPlayback()
    {
        runningJobHandle.Complete();

        runningCommandBuffer.Playback(entityManager);
        runningCommandBuffer.Dispose();
    }

    void ScheduleMoreJobs()
    {
        NativeArray<ArchetypeChunk> dataChunks = meshDataGroup.CreateArchetypeChunkArray(Allocator.TempJob);

        EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);
        JobHandle allHandles = new JobHandle();
        JobHandle previousHandle = new JobHandle();

        ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();
        ArchetypeChunkComponentType<Sector> sectorType = GetArchetypeChunkComponentType<Sector>(true);
        ArchetypeChunkComponentType<SectorVisFacesCount> faceCountsType = GetArchetypeChunkComponentType<SectorVisFacesCount>(true);
        ArchetypeChunkBufferType<Block> blocksType = GetArchetypeChunkBufferType<Block>(true);
        ArchetypeChunkBufferType<BlockFaces> facesType = GetArchetypeChunkBufferType<BlockFaces>(true);

        for (int c = 0; c < dataChunks.Length; c++)
        {
            ArchetypeChunk dataChunk = dataChunks[c];

            //	Get chunk data
            NativeArray<Entity> entities = dataChunk.GetNativeArray(entityType);
            NativeArray<Sector> sectors = dataChunk.GetNativeArray(sectorType);
            NativeArray<SectorVisFacesCount> faceCounts = dataChunk.GetNativeArray(faceCountsType);
            BufferAccessor<Block> blockAccessor = dataChunk.GetBufferAccessor(blocksType);
            BufferAccessor<BlockFaces> facesAccessor = dataChunk.GetBufferAccessor(facesType);

            for (int e = 0; e < entities.Length; e++)
            {
                MeshDataJob meshDataJob = new MeshDataJob()
                {
                    ECBuffer = eCBuffer,
                    entity = entities[e],
                    counts = faceCounts[e],
                    sector = sectors[e],

                    blocks = new NativeArray<Block>(blockAccessor[e].AsNativeArray(), Allocator.TempJob),
                    blockFaces = new NativeArray<BlockFaces>(facesAccessor[e].AsNativeArray(), Allocator.TempJob),
                    
                };

                JobHandle thisHandle = meshDataJob.Schedule(previousHandle);
                allHandles = JobHandle.CombineDependencies(thisHandle, allHandles);

                previousHandle = thisHandle;
            }
        }

        runningCommandBuffer = eCBuffer;
        runningJobHandle = allHandles;

        dataChunks.Dispose();
    }
}
