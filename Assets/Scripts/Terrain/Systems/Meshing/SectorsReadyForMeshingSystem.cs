

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(SectorAdjacentSystem))]
public class SectorsReadyForMeshingSystem : ComponentSystem
{
    EntityManager entityManager;
    ComponentGroup meshReadyGroup;

    protected override void OnCreateManager()
    {
        entityManager = World.GetOrCreateManager<EntityManager>();

        EntityArchetypeQuery squareQuery = new EntityArchetypeQuery
        {
            None = new ComponentType[] { typeof(NotInDrawRangeSectorTag), typeof(NeighboursAreReady) },
            All = new ComponentType[] { typeof(Sector), typeof(DrawMeshTag), typeof(AdjacentSectors) }
        };
        meshReadyGroup = GetComponentGroup(squareQuery);
    }


    protected override void OnUpdate()
    {
        CheckNeighboursAreReady();
    }

    void CheckNeighboursAreReady()
    {
        NativeArray<ArchetypeChunk> dataChunks = meshReadyGroup.CreateArchetypeChunkArray(Allocator.TempJob);

        ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();
        ArchetypeChunkComponentType<AdjacentSectors> adjacentType = GetArchetypeChunkComponentType<AdjacentSectors>(true);

        for (int c = 0; c < dataChunks.Length; c++)
        {
            ArchetypeChunk dataChunk = dataChunks[c];

            NativeArray<Entity> entities = dataChunk.GetNativeArray(entityType);
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

                if (northNeighbour.Length == 0 || southNeighbour.Length == 0 || eastNeighbour.Length == 0 ||
                    westNeighbour.Length == 0 || upNeighbour.Length == 0 || downNeighbour.Length == 0)
                {
                    current.Dispose();
                    northNeighbour.Dispose();
                    southNeighbour.Dispose();
                    eastNeighbour.Dispose();
                    westNeighbour.Dispose();
                    upNeighbour.Dispose();
                    downNeighbour.Dispose();
                    continue;
                }
                else
                {
                    PostUpdateCommands.AddComponent(entity, new NeighboursAreReady());
                }
                current.Dispose();
                northNeighbour.Dispose();
                southNeighbour.Dispose();
                eastNeighbour.Dispose();
                westNeighbour.Dispose();
                upNeighbour.Dispose();
                downNeighbour.Dispose();
            }
        }
        dataChunks.Dispose();
    }
}