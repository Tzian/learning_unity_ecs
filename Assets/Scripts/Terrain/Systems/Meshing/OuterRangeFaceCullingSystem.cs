using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(InnerRangeFaceCullingSystem))]
public class OuterRangeFaceCullingSystem : ComponentSystem
{
    EntityManager entityManager;
    ComponentGroup meshGroup;

    int sectorSize;
    Util util;
    CubeDirections cubeDirections;

    JobHandle runningJobHandle;
    EntityCommandBuffer runningECBuffer;

    protected override void OnCreateManager()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        util = new Util();
        cubeDirections = new CubeDirections();
        sectorSize = TerrainSettings.sectorSize;

        EntityArchetypeQuery sectorQuery = new EntityArchetypeQuery
        {
            None = new ComponentType[] { typeof(NotInDrawRangeSectorTag), typeof(InnerDrawRangeSectorTag) },
            All = new ComponentType[] { typeof(Sector), typeof(DrawMeshTag), typeof(AdjacentSectors), typeof(NeighboursAreReady) }
        };
        meshGroup = GetComponentGroup(sectorQuery);

        runningJobHandle = new JobHandle();
        runningECBuffer = new EntityCommandBuffer(Allocator.TempJob);
    }

    protected override void OnDestroyManager()
    {
        if (runningECBuffer.IsCreated) runningECBuffer.Dispose();
    }

    protected override void OnUpdate()
    {
        if (!runningJobHandle.IsCompleted)
            return;

        JobCompleteAndBufferPlayback();
        ScheduleMoreJobs();
    }

    void ScheduleMoreJobs()
    {
        NativeArray<ArchetypeChunk> dataChunks = meshGroup.CreateArchetypeChunkArray(Allocator.TempJob);

        if (dataChunks.Length != 0)
        {
            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);
            JobHandle allHandles = new JobHandle();
            JobHandle previousHandle = new JobHandle();

            ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();
            ArchetypeChunkComponentType<Translation> positionType = GetArchetypeChunkComponentType<Translation>(true);
            ArchetypeChunkComponentType<AdjacentSectors> adjacentType = GetArchetypeChunkComponentType<AdjacentSectors>(true);

            for (int c = 0; c < dataChunks.Length; c++)
            {
                ArchetypeChunk dataChunk = dataChunks[c];

                NativeArray<Entity> entities = dataChunk.GetNativeArray(entityType);
                NativeArray<Translation> positions = dataChunk.GetNativeArray(positionType);
                NativeArray<AdjacentSectors> adjacents = dataChunk.GetNativeArray(adjacentType);


                for (int e = 0; e < entities.Length; e++)
                {
                    Entity entity = entities[e];
                    AdjacentSectors adjacentSquares = adjacents[e];

                    NativeArray<Block> current = new NativeArray<Block>(entityManager.GetBuffer<Block>(entity).AsNativeArray(), Allocator.TempJob);
                    NativeArray<Block> northNeighbour = new NativeArray<Block>(entityManager.GetBuffer<Block>(adjacentSquares[0]).AsNativeArray(), Allocator.TempJob);
                    NativeArray<Block> southNeighbour = new NativeArray<Block>(entityManager.GetBuffer<Block>(adjacentSquares[1]).AsNativeArray(), Allocator.TempJob);
                    NativeArray<Block> eastNeighbour = new NativeArray<Block>(entityManager.GetBuffer<Block>(adjacentSquares[2]).AsNativeArray(), Allocator.TempJob);
                    NativeArray<Block> westNeighbour = new NativeArray<Block>(entityManager.GetBuffer<Block>(adjacentSquares[3]).AsNativeArray(), Allocator.TempJob);
                    NativeArray<Block> upNeighbour = new NativeArray<Block>(entityManager.GetBuffer<Block>(adjacentSquares[4]).AsNativeArray(), Allocator.TempJob);
                    NativeArray<Block> downNeighbour = new NativeArray<Block>(entityManager.GetBuffer<Block>(adjacentSquares[5]).AsNativeArray(), Allocator.TempJob);


                    NativeArray<float3> directions = new NativeArray<float3>(6, Allocator.TempJob);
                    for (int i = 0; i < directions.Length; i++)
                    {
                        directions[i] = cubeDirections[i];
                    }

                    FacesJob job = new FacesJob()
                    {
                        ECBuffer = eCBuffer,
                        entity = entity,

                        sectorSize = sectorSize,
                        directions = directions,
                        util = util,

                        current = current,
                        northNeighbour = northNeighbour,
                        southNeighbour = southNeighbour,
                        eastNeighbour = eastNeighbour,
                        westNeighbour = westNeighbour,
                        upNeighbour = upNeighbour,
                        downNeighbour = downNeighbour

                    };

                    JobHandle thisHandle = job.Schedule(previousHandle);
                    allHandles = JobHandle.CombineDependencies(thisHandle, allHandles);

                    previousHandle = thisHandle;
                }
            }
            runningECBuffer = eCBuffer;
            runningJobHandle = allHandles;
        }
        dataChunks.Dispose();
    }

    void JobCompleteAndBufferPlayback()
    {
        runningJobHandle.Complete();

        runningECBuffer.Playback(entityManager);
        runningECBuffer.Dispose();
    }
}