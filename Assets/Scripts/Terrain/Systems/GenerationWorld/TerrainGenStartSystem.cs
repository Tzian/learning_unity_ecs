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

        public static Entity playerEntity;
        int3 playersCurrentSector;
        int3 playersPreviousSector;

        int sectorSize;
        Util util;
        bool firstRun;

        protected override void OnCreateManager()
        {
            tGenEntityManager = World.GetOrCreateManager<EntityManager>();
            sectorEntityArchetype = TerrainEntityFactory.CreateSectorArchetype(tGenEntityManager);
            playerEntity = TerrainSystem.playerEntity;
            sectorSize = TerrainSettings.sectorSize;
            util = new Util();
            firstRun = true;

           // Debug.Log("system1's created with world    " + this.World);
        }

        protected override void OnStartRunning()
        {
            //Debug.Log("system1's onStartRunning ");
            playersCurrentSector = GetPlayersCurrentSector();
            playersPreviousSector = playersCurrentSector + (100 * sectorSize);
        }

        protected override void OnUpdate()
        {
            //Debug.Log("in system 1 update ");
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
            EntityManager eM = Worlds.defaultWorld.GetExistingManager<EntityManager>();
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
                        if (util.Float3sMatchXYZ(sectorWorldPos, playersCurrentSector)) continue;  // players first sector is created in main world

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
            return newSectorEntity;
        }
    }
}