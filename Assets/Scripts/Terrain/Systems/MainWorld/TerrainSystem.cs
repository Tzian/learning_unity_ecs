using PhysicsEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;


namespace Terrain
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGroup))]
    public class TerrainSystem : ComponentSystem
    {
        EntityManager entityManager;
        Util util;
        EntityArchetype sectorEntityArchetype;
        int sectorSize;

        float3 playerStartPos;
        public static Entity playerEntity;
        public int3 playersCurrentSector;
        int3 playersPreviousSector;

        public Matrix3D<Entity> sectorMatrix;
        int matrixWidth;

        bool firstRun;


        protected override void OnCreateManager()
        {
            entityManager = Worlds.defaultWorld.GetOrCreateManager<EntityManager>();  
            sectorEntityArchetype = TerrainEntityFactory.CreateSectorArchetype(entityManager);
            util = new Util();
            sectorSize = TerrainSettings.sectorSize;
            matrixWidth = TerrainSettings.sectorGenerationRange * 2 + 1;

            firstRun = true;
            playerStartPos = new float3(0, TerrainSettings.playerStartHeight, 0);
        }

        protected override void OnStartRunning()
        {
            StartSectorMatrix((int3)playerStartPos);
            playersPreviousSector = playersCurrentSector + (100 * sectorSize);
        }

        protected override void OnDestroyManager()
        {
            sectorMatrix.Dispose();
        }

        protected override void OnUpdate()
        {
            if (firstRun == true)
            {
                playerEntity = CreatePlayer(playerStartPos);
                firstRun = false;
            }

            playersCurrentSector = GetPlayersCurrentSector();

            if (playersCurrentSector.Equals(playersPreviousSector))
            {
                return;
            }

            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.Temp);
            CheckSectorDrawRange(playersCurrentSector, eCBuffer);

            playersPreviousSector = playersCurrentSector;
        }

        public int3 GetPlayersCurrentSector()
        {
            float3 playerPosition = entityManager.GetComponentData<Translation>(playerEntity).Value;
            float3 currentSector = util.GetSectorPosition(playerPosition, sectorSize);
            return (int3)currentSector;
        }

        private static Entity CreatePlayer(float3 startPos)
        {
            EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

            EntityArchetype playerSetup = PhysicsEntityFactory.CreatePlayerArchetype(entityManager);
            Entity playerEntity = entityManager.CreateEntity(playerSetup);

            entityManager.SetComponentData(playerEntity, new Translation { Value = startPos });
            entityManager.SetComponentData(playerEntity, new Rotation { Value = quaternion.identity });
            entityManager.SetComponentData(playerEntity, new Velocity { Value = new float3(0, 0, 0) });

            RenderMesh renderer = new RenderMesh();
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            renderer.mesh = capsule.GetComponent<MeshFilter>().mesh;
            GameObject.Destroy(capsule);
            renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PlayerCapsuleMaterial.mat");

            entityManager.AddSharedComponentData(playerEntity, renderer);
            return playerEntity;
        }
        void StartSectorMatrix(int3 playersCurrentSector)
        {
            sectorMatrix = new Matrix3D<Entity>(matrixWidth, Allocator.Persistent, playersCurrentSector, sectorSize);
        }

        void CheckSectorDrawRange(int3 playersCurrentSector, EntityCommandBuffer eCBuffer)
        {
            int range = TerrainSettings.sectorGenerationRange;
            for (int i = 0; i < sectorMatrix.Length; i++)
            {
                if (!sectorMatrix.ItemIsSet(i))
                    continue;

                int3 sectorPosToCheck = sectorMatrix.IndexToWorldPosition(i);
                Entity sectorEntity = sectorMatrix.GetItem(i);

                if (sectorMatrix.InRangeFromWorldPosition(playersCurrentSector, sectorPosToCheck, range - 5))
                {

                    if (!entityManager.HasComponent(sectorEntity, typeof(InnerDrawRangeSectorTag)))
                    {
                        eCBuffer.AddComponent(sectorEntity, new InnerDrawRangeSectorTag());
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(OuterDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(OuterDrawRangeSectorTag));
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag));
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(NotInDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(NotInDrawRangeSectorTag));
                    }
                }
                else if (sectorMatrix.InRangeFromWorldPosition(playersCurrentSector, sectorPosToCheck, range - 3))
                {
                    if (!entityManager.HasComponent(sectorEntity, typeof(OuterDrawRangeSectorTag)))
                    {
                        eCBuffer.AddComponent(sectorEntity, new OuterDrawRangeSectorTag());
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(InnerDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(InnerDrawRangeSectorTag));
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag));
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(NotInDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(NotInDrawRangeSectorTag));
                    }
                }
                else if (sectorMatrix.InRangeFromWorldPosition(playersCurrentSector, sectorPosToCheck, range - 1))
                {
                    if (!entityManager.HasComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag)))
                    {
                        eCBuffer.AddComponent(sectorEntity, new EdgeOfDrawRangeSectorTag());
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(InnerDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(InnerDrawRangeSectorTag));
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(OuterDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(OuterDrawRangeSectorTag));
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(NotInDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(NotInDrawRangeSectorTag));
                    }
                }
                else
                {
                    if (!entityManager.HasComponent(sectorEntity, typeof(NotInDrawRangeSectorTag)))
                    {
                        eCBuffer.AddComponent(sectorEntity, new NotInDrawRangeSectorTag());
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(InnerDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(InnerDrawRangeSectorTag));
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(OuterDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(OuterDrawRangeSectorTag));
                    }
                    if (entityManager.HasComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag)))
                    {
                        eCBuffer.RemoveComponent(sectorEntity, typeof(EdgeOfDrawRangeSectorTag));
                    }
                }
            }
            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();
        }
    }
}