using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[AlwaysUpdateSystem]
public class TerrainSystem : ComponentSystem
{
    EntityManager entityManager;
    Util util;
    EntityArchetype sectorEntityArchetype;
    int sectorSize;

    public static Entity playerEntity;
    public int3 playersCurrentSector;
    int3 playersPreviousSector;

    public Matrix3D<Entity> sectorMatrix;
    int matrixWidth;

    bool firstRun;


    protected override void OnCreateManager()
    {
        entityManager = World.GetOrCreateManager<EntityManager>();  // create entity manager for the world this componentsystem is attached to
        sectorEntityArchetype = TerrainEntityFactory.CreateSectorArchetype(entityManager);
        util = new Util();
        sectorSize = TerrainSettings.sectorSize;
        matrixWidth = TerrainSettings.sectorGenerationRange * 2 + 1;

        firstRun = true;

        // Debug.Log("Terrain systems world " + World);
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

    protected override void OnUpdate()
    {
        playersCurrentSector = GetPlayersCurrentSector();

        if (firstRun == true)
        {
            firstRun = false;
            CreateStartSector(playersCurrentSector);
        }

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

    void StartSectorMatrix(int3 playersCurrentSector)
    {
        sectorMatrix = new Matrix3D<Entity>(matrixWidth, Allocator.Persistent, playersCurrentSector, sectorSize);
    }

    Entity CreateStartSector(int3 sectorWorldPos)
    {
        Entity newSectorEntity = entityManager.CreateEntity(sectorEntityArchetype);
        entityManager.SetComponentData(newSectorEntity, new Translation { Value = sectorWorldPos });
        entityManager.SetComponentData(newSectorEntity, new Sector { entity = newSectorEntity, worldPosition = sectorWorldPos });
        sectorMatrix.AddItem(newSectorEntity, sectorWorldPos);
        return newSectorEntity;
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