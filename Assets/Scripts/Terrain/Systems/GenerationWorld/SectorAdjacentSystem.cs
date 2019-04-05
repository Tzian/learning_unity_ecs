using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Terrain;
using UnityEngine;

namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(SectorGeologySystem))]
    public class SectorAdjacentSystem : ComponentSystem
    {
        EntityManager entityManager;
        TerrainSystem terrainSystem;
        Matrix3D<Entity> sectorMatrix;
        CubeDirections cubeDirections;

        int sectorSize;

        ComponentGroup adjSectorsGroup;

        protected override void OnCreateManager()
        {
            // Debug.Log(" this system SectorAdjacentSystem  " + World);

            entityManager = World.GetOrCreateManager<EntityManager>();
            terrainSystem = Bootstrapped.defaultWorld.GetExistingManager<TerrainSystem>();
            sectorSize = TerrainSettings.sectorSize;
            cubeDirections = new CubeDirections();

            EntityArchetypeQuery adjSectorsQuery = new EntityArchetypeQuery
            {
                Any = new ComponentType[] { typeof(InnerDrawRangeSectorTag), typeof(OuterDrawRangeSectorTag), typeof(EdgeOfDrawRangeSectorTag) },
                None = new ComponentType[] { typeof(NotInDrawRangeSectorTag), typeof(GetSectorTopography), typeof(GenerateSectorGeology) },
                All = new ComponentType[] { typeof(Sector), typeof(GetAdjacentSectors) }
            };
            adjSectorsGroup = GetComponentGroup(adjSectorsQuery);
        }

        protected override void OnUpdate()
        {
            NativeArray<ArchetypeChunk> dataChunks = adjSectorsGroup.CreateArchetypeChunkArray(Allocator.TempJob);

            if (dataChunks.Length == 0)
            {
                dataChunks.Dispose();
                return;
            }

            ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();
            ArchetypeChunkComponentType<Translation> positionType = GetArchetypeChunkComponentType<Translation>(true);

            NativeArray<int3> adjacentPositions = new NativeArray<int3>(6, Allocator.TempJob);
            for (int i = 0; i < adjacentPositions.Length; i++)
            {
                adjacentPositions[i] = cubeDirections[i] * sectorSize;
            }

            sectorMatrix = terrainSystem.sectorMatrix;

            for (int d = 0; d < dataChunks.Length; d++)
            {
                ArchetypeChunk dataChunk = dataChunks[d];

                NativeArray<Entity> entities = dataChunk.GetNativeArray(entityType);
                NativeArray<Translation> positions = dataChunk.GetNativeArray(positionType);

                for (int e = 0; e < entities.Length; e++)
                {
                    Entity entity = entities[e];
                    int3 sectorWPos = (int3)positions[e].Value;

                    Entity north = sectorMatrix.GetItem(sectorWPos + adjacentPositions[0]);
                    Entity south = sectorMatrix.GetItem(sectorWPos + adjacentPositions[1]);
                    Entity east = sectorMatrix.GetItem(sectorWPos + adjacentPositions[2]);
                    Entity west = sectorMatrix.GetItem(sectorWPos + adjacentPositions[3]);
                    Entity up = sectorMatrix.GetItem(sectorWPos + adjacentPositions[4]);
                    Entity down = sectorMatrix.GetItem(sectorWPos + adjacentPositions[5]);

                    AdjacentSectors adjSectors = new AdjacentSectors
                    {
                        north = north,
                        south = south,
                        east = east,
                        west = west,
                        up = up,
                        down = down
                    };

                    if (entityManager.HasComponent<AdjacentSectors>(entity))
                    {
                        PostUpdateCommands.SetComponent(entity, adjSectors);
                    }
                    else
                    {
                        PostUpdateCommands.AddComponent(entity, adjSectors);
                    }

                    PostUpdateCommands.RemoveComponent<GetAdjacentSectors>(entity);
                }
            }
            adjacentPositions.Dispose();
            dataChunks.Dispose();
        }
    }
}