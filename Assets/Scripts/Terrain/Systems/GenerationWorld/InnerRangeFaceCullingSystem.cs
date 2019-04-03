using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(SectorsReadyForMeshingSystem))]
    public class InnerRangeFaceCullingSystem : JobComponentSystem
    {
        EntityManager entityManager;
        ComponentGroup meshGroup;

        int sectorSize;
        Util util;
        CubeDirections cubeDirections;

        protected override void OnCreateManager()
        {
           // Debug.Log(" this system InnerRangeFaceCullingSystem  " + World);

            entityManager = World.GetOrCreateManager<EntityManager>();
            util = new Util();
            cubeDirections = new CubeDirections();
            sectorSize = TerrainSettings.sectorSize;

            EntityArchetypeQuery sectorQuery = new EntityArchetypeQuery
            {
                None = new ComponentType[] { typeof(NotInDrawRangeSectorTag), typeof(OuterDrawRangeSectorTag) },
                All = new ComponentType[] { typeof(Sector), typeof(DrawMeshTag), typeof(AdjacentSectors), typeof(NeighboursAreReady) }
            };
            meshGroup = GetComponentGroup(sectorQuery);


        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<ArchetypeChunk> dataChunks = meshGroup.CreateArchetypeChunkArray(Allocator.TempJob);

            if (dataChunks.Length == 0)
            {
                dataChunks.Dispose();
                return inputDeps;
            }

            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);

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

                    var facesJob = new FacesJob()
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

                    }.Schedule(inputDeps);
                    facesJob.Complete();
                }
            }
            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();
            dataChunks.Dispose();
            return inputDeps;
        }
    }
}