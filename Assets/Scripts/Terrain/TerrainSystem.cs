using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[AlwaysUpdateSystem]
public class TerrainSystem : JobComponentSystem
{
    EntityManager entityManager;
    Util util;
    EntityArchetype sectorEntityArchetype;
    int sectorSize;

    public static Entity playerEntity;
    int3 playersCurrentSector;
    int3 playersPreviousSector;

    public Matrix3D<Entity> sectorMatrix;
    int matrixWidth;

    public bool update;

    protected override void OnCreateManager()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        sectorEntityArchetype = TerrainEntityFactory.CreateSectorArchetype(entityManager);
        util = new Util();
        sectorSize = TerrainSettings.sectorSize;
        matrixWidth = TerrainSettings.sectorGenerationRange * 2 + 1;
        update = true;
    }

    protected override void OnStartRunning()
    {
        playersCurrentSector = GetPlayersCurrentSector();

        StartSectorMatrix(playersCurrentSector);

        playersPreviousSector = playersCurrentSector + (100 * sectorSize);
    }

    protected override void OnDestroyManager()
    {
        sectorMatrix.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        playersCurrentSector = GetPlayersCurrentSector();

        if (playersCurrentSector.Equals(playersPreviousSector))
        {
            update = false;
            return inputDeps;
        }
        update = true;
        playersPreviousSector = playersCurrentSector;

        GenerateSectorsInRange(playersCurrentSector);
        RemoveOutOfRangeSectors(playersCurrentSector);

        return inputDeps;
    }

    public int3 GetPlayersCurrentSector()
    {
        float3 playerPosition = entityManager.GetComponentData<Translation>(playerEntity).Value;
        float3 currentSector = util.GetSectorPosition(playerPosition, sectorSize);
        return (int3)currentSector;
    }

    void StartSectorMatrix(int3 playersCurrentSector)
    {
        sectorMatrix = new Matrix3D<Entity>(matrixWidth, Allocator.Persistent, playersCurrentSector, sectorSize);
    }

    void GenerateSectorsInRange(int3 playersCurrentSector)
    {
        int range = TerrainSettings.sectorGenerationRange * sectorSize;

        for (int x = playersCurrentSector.x - range; x <= playersCurrentSector.x + range; x++)
        {
            for (int y = playersCurrentSector.y - range; y <= playersCurrentSector.y + range; y++)
            {
                for (int z = playersCurrentSector.z - range; z <= playersCurrentSector.z + range; z++)
                {
                    int3 sectorWorldPos = new int3(x, y, z);
                    if (sectorMatrix.ItemIsSet(sectorWorldPos))
                        continue;

                    CreateNewSector(sectorWorldPos);
                }
            }
        }
    }

    Entity CreateNewSector(int3 sectorWorldPos)
    {
        Entity newSectorEntity = entityManager.CreateEntity(sectorEntityArchetype);
        entityManager.SetComponentData(newSectorEntity, new Translation { Value = sectorWorldPos });
        entityManager.SetComponentData(newSectorEntity, new Sector { entity = newSectorEntity, worldPosition = sectorWorldPos });
        sectorMatrix.AddItem(newSectorEntity, sectorWorldPos);
        return newSectorEntity;
    }

    void RemoveOutOfRangeSectors(int3 playersCurrentSector)
    {
        int range = TerrainSettings.sectorGenerationRange;
        for (int i = 0; i < sectorMatrix.Length; i++)
        {
            if (!sectorMatrix.ItemIsSet(i))
                continue;

            int3 sectorPosToCheck = sectorMatrix.IndexToWorldPosition(i);

            if (!sectorMatrix.InRangeFromWorldPosition(playersCurrentSector, sectorPosToCheck, range))
            {
                Entity entityForRemoval = sectorMatrix.GetItem(i);
                entityManager.AddComponentData(entityForRemoval, new SectorRemoveTag());
                sectorMatrix.UnsetItem(i);
            }
        }
    }
}