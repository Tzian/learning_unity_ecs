using Terrain;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    public class TerrainGenStartSystem : ComponentSystem
    {
        EntityManager tGenEntityManager;
        EntityArchetype sectorEntityArchetype;
        TerrainSystem terrainSystem;
        Entity playerEntity;
        int3 playersCurrentSector;
        int3 playersPreviousSector;

        int sectorSize;
        Util util;
        bool firstRun;

        protected override void OnCreateManager()
        {
            tGenEntityManager = World.GetOrCreateManager<EntityManager>();
            sectorEntityArchetype = TerrainEntityFactory.CreateSectorArchetype(tGenEntityManager);
            sectorSize = TerrainSettings.sectorSize;
            playerEntity = Bootstrapped.playerEntity;
            terrainSystem = Bootstrapped.defaultWorld.GetExistingManager<TerrainSystem>();

            util = new Util();
            firstRun = true;
        }

        protected override void OnStartRunning()
        {
            //Debug.Log("system1's onStartRunning ");
            playersCurrentSector = GetPlayersCurrentSector();
            playersPreviousSector = playersCurrentSector + (100 * sectorSize);
        }

        protected override void OnUpdate()
        {
            if (terrainSystem.sectorMatrix.Length == 0)
                return;

            playersCurrentSector = GetPlayersCurrentSector();

            if (firstRun)
            {
                firstRun = false;
                CreateStartingSectors(playersCurrentSector);
            }
            playersPreviousSector = playersCurrentSector;
        }

        public int3 GetPlayersCurrentSector()
        {
            EntityManager eM = Bootstrapped.defaultWorld.GetExistingManager<EntityManager>();
            float3 playerPosition = eM.GetComponentData<Translation>(playerEntity).Value;
            float3 currentSector = util.GetSectorPosition(playerPosition, sectorSize);
            return (int3)currentSector;
        }

        void CreateStartingSectors(int3 playersCurrentSector)
        {
            int range = (TerrainSettings.sectorGenerationRange);
            int3 startPos = playersCurrentSector / sectorSize;

            for (int x = startPos.x - range; x <= startPos.x + range; x++)
            {
                for (int y = startPos.y - range; y <= startPos.y + range; y++)
                {
                    for (int z = startPos.z - range; z <= startPos.z + range; z++)
                    {
                        int3 sectorWorldPos = new int3(x, y, z) * sectorSize;

                        CreateNewSector(sectorWorldPos);
                    }
                }
            }
        }

        Entity CreateNewSector(int3 sectorWorldPos)
        {
            Entity newSectorEntity = tGenEntityManager.CreateEntity(sectorEntityArchetype);
            tGenEntityManager.SetComponentData(newSectorEntity, new Translation { Value = sectorWorldPos });
            tGenEntityManager.SetComponentData(newSectorEntity, new Sector { entity = newSectorEntity, worldPosition = sectorWorldPos });
            tGenEntityManager.AddComponentData(newSectorEntity, new SectorDrawRange { sectorDrawRange = -1 });
            terrainSystem.sectorMatrix.AddItem(newSectorEntity, sectorWorldPos);
            return newSectorEntity;
        }
    }
}