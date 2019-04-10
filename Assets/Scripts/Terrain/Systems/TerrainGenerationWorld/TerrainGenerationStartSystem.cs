﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace TerrainGeneration
{
    [CreateInWorld("TerrainGenerationWorld")]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    public class TerrainGenerationStartSystem : ComponentSystem
    {
        EntityManager tGEntityManager;
        EntityArchetype voxelEntityArchetype;
        Entity playerEntity;
        int3 playersCurrentPosition;
        int3 playersPreviousPosition;

        bool firstRun;

        protected override void OnCreateManager()
        {
            tGEntityManager = World.EntityManager;
            voxelEntityArchetype = TerrainEntityFactory.CreateVoxelArchetype(tGEntityManager);
            firstRun = true;
        }

        protected override void OnStartRunning()
        {
            playerEntity = Bootstrapped.playerEntity;
            playersCurrentPosition = GetPlayersCurrentPosition();
            playersPreviousPosition = playersCurrentPosition + (100);
        }

        protected override void OnStopRunning()
        {
            World.EntityManager.CompleteAllJobs();
        }

        protected override void OnDestroyManager()
        {
            Data.Store.viewZoneMatrix.Dispose();
        }

        protected override void OnUpdate()
        {
            playersCurrentPosition = GetPlayersCurrentPosition();

            if (firstRun == true)
            {
                firstRun = false;
                CreateStartArea(playersCurrentPosition);
            }
            playersPreviousPosition = playersCurrentPosition;
        }

        public int3 GetPlayersCurrentPosition()
        {
            EntityManager eM = Bootstrapped.DefaultWorld.EntityManager;
            int3 playerPosition = (int3)eM.GetComponentData<Translation>(playerEntity).Value;
            return playerPosition;
        }

        void CreateStartArea(int3 playersCurrentPosition)
        {
            int range = TerrainSettings.areaGenerationRange;

            for (int x = playersCurrentPosition.x - range; x <= playersCurrentPosition.x + range; x++)
            {
                for (int y = playersCurrentPosition.y - range; y <= playersCurrentPosition.y + range; y++)
                {
                    for (int z = playersCurrentPosition.z - range; z <= playersCurrentPosition.z + range; z++)
                    {
                        int3 voxelPosition = new int3(x, y, z);

                        CreateNewVoxel(voxelPosition);
                    }
                }
            }
        }

        Entity CreateNewVoxel(int3 voxelPosition)
        {
            Entity newVoxelEntity = tGEntityManager.CreateEntity(voxelEntityArchetype);
            tGEntityManager.SetComponentData(newVoxelEntity, new Voxel { WorldPosition = voxelPosition });
            tGEntityManager.SetComponentData(newVoxelEntity, new Translation { Value = voxelPosition });
            tGEntityManager.AddComponent(newVoxelEntity, typeof(GetVoxelDrawRange));
            Data.Store.viewZoneMatrix.AddItem(newVoxelEntity, voxelPosition);
            return newVoxelEntity;
        }
    }
}