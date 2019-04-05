//using System.Collections.Generic;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;

//namespace Terrain
//{
//    [DisableAutoCreation]
//    [UpdateInGroup(typeof(TerrainGroup))]
//    [UpdateAfter(typeof(SectorRemovalSystem))]
//    public class DebugSystem : ComponentSystem
//    {
//        EntityManager entityManager;
//        ComponentGroup allSectorsGroup;
//        DebugLineUtil lineUtil;
//        int sectorSize;

//        public List<Dictionary<Entity, List<DebugLineUtil.DebugLine>>> sectorLines = new List<Dictionary<Entity, List<DebugLineUtil.DebugLine>>>()
//    {
//        new Dictionary<Entity, List<DebugLineUtil.DebugLine>>(),  //  Horizontal Buffer
//        new Dictionary<Entity, List<DebugLineUtil.DebugLine>>(),  //  Block buffer
//        new Dictionary<Entity, List<DebugLineUtil.DebugLine>>(),  //  Mark error
//        new Dictionary<Entity, List<DebugLineUtil.DebugLine>>()   //  Draw buffer
//    };

//        protected override void OnCreateManager()
//        {
//            entityManager = World.Active.GetOrCreateManager<EntityManager>();
//            sectorSize = TerrainSettings.sectorSize;
//            lineUtil = new DebugLineUtil(sectorSize);

//            EntityArchetypeQuery allSectorsQuery = new EntityArchetypeQuery
//            {
//                Any = new ComponentType[] { typeof(InnerDrawRangeSectorTag), typeof(OuterDrawRangeSectorTag), typeof(EdgeOfDrawRangeSectorTag) },
//                None = new ComponentType[] { typeof(SectorRemoveTag) },
//                All = new ComponentType[] { typeof(Sector) }
//            };
//            allSectorsGroup = GetComponentGroup(allSectorsQuery);
//        }

//        protected override void OnUpdate()
//        {
//            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.Temp);
//            NativeArray<ArchetypeChunk> dataChunks = allSectorsGroup.CreateArchetypeChunkArray(Allocator.Persistent);

//            ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();
//            ArchetypeChunkComponentType<Translation> positionType = GetArchetypeChunkComponentType<Translation>();


//            for (int c = 0; c < dataChunks.Length; c++)
//            {
//                ArchetypeChunk dataChunk = dataChunks[c];

//                NativeArray<Entity> entities = dataChunk.GetNativeArray(entityType);
//                NativeArray<Translation> positions = dataChunk.GetNativeArray(positionType);

//                for (int e = 0; e < entities.Length; e++)
//                {
//                    Entity entity = entities[e];
//                    float3 position = positions[e].Value;

//                    if (!entityManager.HasComponent<InnerDrawRangeSectorTag>(entity) && !entityManager.HasComponent<OuterDrawRangeSectorTag>(entity)
//                                && ! entityManager.HasComponent<EdgeOfDrawRangeSectorTag>(entity))
//                    {
//                        Debug.Log("I am trying to check a sector with NO draw range tag" + entity);
//                        continue;
//                    }
//                    else
//                        DebugSectorDrawType(entity, position);
//                }
//            }
//            eCBuffer.Playback(entityManager);
//            eCBuffer.Dispose();

//            dataChunks.Dispose();
//        }

//        void DebugSectorDrawType(Entity entity, float3 position)
//        {
//            sectorLines[0][entity] = lineUtil.CreateBox(
//                position,
//                sectorSize * 0.95f,
//                HorizontalBufferToColor(entity),
//                noSides: true,
//                topOnly: false
//            );
//        }

//        Color HorizontalBufferToColor(Entity entity)
//        {
//            if (entityManager.HasComponent<InnerDrawRangeSectorTag>(entity))
//            {
//                return new Color(0, 1, 0, 0.4f);
//            }
//            if (entityManager.HasComponent<OuterDrawRangeSectorTag>(entity))
//            {
//                return new Color(0, 0, 1, 0.2f);
//            }
//            if (entityManager.HasComponent<EdgeOfDrawRangeSectorTag>(entity))
//            {
//                return new Color(1, 0, 0, 0.2f);
//            }
//            else
//            {
//                Debug.Log("why dont i have a draw range tag " + entity);
//                return new Color(0, 0, 0, 1f);
//            }
//        }
//    }

//    //public void BlockBufferDebug (Entity entity, float3 position)
//    //{
//    //    sectorLines [1] [entity] = lineUtil.CreateBox (
//    //        new float3 (position.x, 0, position.z),
//    //        sectorSize * 0.99f,
//    //        new Color (0.8f, 0.8f, 0.8f, 0.1f),
//    //        noSides: false
//    //    );
//    //}

//    //public void DrawBufferDebug (Entity entity, float3 position)
//    //{
//    //    sectorLines [3] [entity] = lineUtil.CreateBox (
//    //        new float3 (position.x, 0, position.z),
//    //        sectorSize * 0.99f,
//    //        new Color (0.8f, 0.8f, 0.8f, 0.1f),
//    //        noSides: false
//    //    );
//    //}

//    //public void MarkError (Entity entity, float3 position, Color color)
//    //{
//    //    sectorLines [2] [entity] = lineUtil.CreateBox (
//    //        new float3 (position.x, 0, position.z),
//    //        sectorSize,
//    //        color,
//    //        noSides: false
//    //    );
//    //}
//}