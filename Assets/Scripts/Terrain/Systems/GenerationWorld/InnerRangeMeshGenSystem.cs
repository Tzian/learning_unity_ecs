using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;


namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(InnerRangeFaceCullingSystem))]
    public class InnerRangeMeshGenSystem : JobComponentSystem
    {
        EntityManager entityManager;
        ComponentGroup meshDataGroup;

        protected override void OnCreateManager()
        {
            entityManager = World.GetOrCreateManager<EntityManager>();

            EntityArchetypeQuery meshDataQuery = new EntityArchetypeQuery
            {
                None = new ComponentType[] { typeof(NotInDrawRangeSectorTag), typeof(OuterDrawRangeSectorTag) },
                All = new ComponentType[] { typeof(Sector), typeof(SectorVisFacesCount) }
            };
            meshDataGroup = GetComponentGroup(meshDataQuery);
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<ArchetypeChunk> dataChunks = meshDataGroup.CreateArchetypeChunkArray(Allocator.TempJob);

            if (dataChunks.Length == 0)
            {
                dataChunks.Dispose();
                return inputDeps;
            }

            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);

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
                    var meshDataJob = new MeshDataJob()
                    {
                        ECBuffer = eCBuffer,
                        entity = entities[e],
                        counts = faceCounts[e],
                        sector = sectors[e],

                        blocks = new NativeArray<Block>(blockAccessor[e].AsNativeArray(), Allocator.TempJob),
                        blockFaces = new NativeArray<BlockFaces>(facesAccessor[e].AsNativeArray(), Allocator.TempJob),

                    }.Schedule(inputDeps);
                    meshDataJob.Complete();
                }
            }
            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();

            dataChunks.Dispose();
            return inputDeps;
        }

    }
}