using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace TerrainGeneration
{
    [CreateInWorld("TerrainGenerationWorld")]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(GenerateMeshDataSystem))]
    public class MoveVoxelsToMainWorldSystem : JobComponentSystem
    {
        EntityQuery myQuery;
        EntityManager tGenEntityManager;
        ChunkIterationSystem chunkIterationSystem;
        NativeArray<Entity> entities;


        protected override void OnCreateManager()
        {
            tGenEntityManager = World.EntityManager;
            chunkIterationSystem = new ChunkIterationSystem();

            myQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Voxel), typeof (VoxelIsInDrawRange), typeof(ReadyToMesh) },
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();
            entities = chunkIterationSystem.GetEntities(myQuery, entityType);

            NativeArray<EntityRemapUtility.EntityRemapInfo> remapping = tGenEntityManager.CreateEntityRemapArray(Allocator.TempJob);

            EntityManager entityManager = Bootstrapped.DefaultWorld.EntityManager;
            entityManager.MoveEntitiesFrom(tGenEntityManager, myQuery, remapping);

            for (int i = 0; i < entities.Length; i++)
            {
                entities[i] = EntityRemapUtility.RemapEntity(ref remapping, entities[i]);
            }

            remapping.Dispose();
            entities.Dispose();
            return inputDeps;
        }
    }
}