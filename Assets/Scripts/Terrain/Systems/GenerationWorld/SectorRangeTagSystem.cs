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
    [UpdateAfter(typeof(TerrainGenStartSystem))]
    public class SectorRangeTagSystem : ComponentSystem
    {
        EntityManager tGenEntityManager;
        TerrainSystem terrainSystem;
        public Entity playerEntity;
        int3 playersCurrentSector;
        int3 playersPreviousSector;

        int sectorSize;
        Util util;
        int range;

        ComponentGroup sectorRangeCheckGroup;
        
        protected override void OnCreateManager()
        {
            tGenEntityManager = World.GetOrCreateManager<EntityManager>();
            terrainSystem = Worlds.defaultWorld.GetExistingManager<TerrainSystem>();
            playerEntity = Bootstrapped.playerEntity;

            sectorSize = TerrainSettings.sectorSize;
            util = new Util();
            range = TerrainSettings.sectorGenerationRange;

            EntityArchetypeQuery sectorRangeCheckQuery = new EntityArchetypeQuery
            {
                All = new ComponentType[] { typeof(Sector) }
            };
            sectorRangeCheckGroup = GetComponentGroup(sectorRangeCheckQuery);
        }

        protected override void OnUpdate()
        {
            playersCurrentSector = GetPlayersCurrentSector();

            NativeArray<ArchetypeChunk> dataChunks = sectorRangeCheckGroup.CreateArchetypeChunkArray(Allocator.TempJob);

            if (dataChunks.Length == 0)
            {
                dataChunks.Dispose();
                return;
            }

            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.Temp);

            ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();
            ArchetypeChunkComponentType<Translation> positionType = GetArchetypeChunkComponentType<Translation>(true);

            for (int d = 0; d < dataChunks.Length; d++)
            {
                ArchetypeChunk dataChunk = dataChunks[d];

                NativeArray<Entity> entities = dataChunk.GetNativeArray(entityType);
                NativeArray<Translation> positions = dataChunk.GetNativeArray(positionType);

                for (int e = 0; e < entities.Length; e++)
                {
                    Entity sectorEntity = entities[e];
                    int3 position = (int3)positions[e].Value;

                    if (util.SectorMatrixInRangeFromWorldPositionCheck(playersCurrentSector, position, playersCurrentSector, range - 5, sectorSize))
                    {

                        if (!tGenEntityManager.HasComponent(sectorEntity, typeof(InnerDrawRangeSectorTag)))
                        {
                            eCBuffer.AddComponent(sectorEntity, new InnerDrawRangeSectorTag());
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(OuterDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(OuterDrawRangeSectorTag));
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag));
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(NotInDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(NotInDrawRangeSectorTag));
                        }
                    }
                    else if (util.SectorMatrixInRangeFromWorldPositionCheck(playersCurrentSector, position, playersCurrentSector, range - 3, sectorSize))
                    {
                        if (!tGenEntityManager.HasComponent(sectorEntity, typeof(OuterDrawRangeSectorTag)))
                        {
                            eCBuffer.AddComponent(sectorEntity, new OuterDrawRangeSectorTag());
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(InnerDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(InnerDrawRangeSectorTag));
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag));
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(NotInDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(NotInDrawRangeSectorTag));
                        }
                    }
                    else if (util.SectorMatrixInRangeFromWorldPositionCheck(playersCurrentSector, position, playersCurrentSector, range - 1, sectorSize))
                    {
                        if (!tGenEntityManager.HasComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag)))
                        {
                            eCBuffer.AddComponent(sectorEntity, new EdgeOfDrawRangeSectorTag());
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(InnerDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(InnerDrawRangeSectorTag));
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(OuterDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(OuterDrawRangeSectorTag));
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(NotInDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(NotInDrawRangeSectorTag));
                        }
                    }
                    else
                    {
                        if (!tGenEntityManager.HasComponent(sectorEntity, typeof(NotInDrawRangeSectorTag)))
                        {
                            eCBuffer.AddComponent(sectorEntity, new NotInDrawRangeSectorTag());
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(InnerDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(InnerDrawRangeSectorTag));
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(OuterDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(OuterDrawRangeSectorTag));
                        }
                        if (tGenEntityManager.HasComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag)))
                        {
                            eCBuffer.RemoveComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag));
                        }
                    }
                }
                eCBuffer.Playback(tGenEntityManager);
                eCBuffer.Dispose();
                dataChunks.Dispose();
            }
            playersPreviousSector = playersCurrentSector;
        }

        public int3 GetPlayersCurrentSector()
        {
            EntityManager eM = Worlds.defaultWorld.GetExistingManager<EntityManager>();
            float3 playerPosition = eM.GetComponentData<Translation>(playerEntity).Value;
            float3 currentSector = util.GetSectorPosition(playerPosition, sectorSize);
            return (int3)currentSector;
        }
    }
}