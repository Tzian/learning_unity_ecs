using Unity.Collections;
using Unity.Entities;

namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(InnerRangeMeshGenSystem))]
    public class MoveEntitiesToDefaultWorldSystem : ComponentSystem
    {
        EntityManager tGenEntityManager;

        ChunkIterationSystem chunkIterationSystem;

        ComponentGroup entitiesForMovingGroup;
        NativeArray<Entity> entities;

        protected override void OnCreateManager()
        {
            tGenEntityManager = World.GetOrCreateManager<EntityManager>();
            chunkIterationSystem = new ChunkIterationSystem();

            EntityArchetypeQuery entitiesForMovingQuery = new EntityArchetypeQuery
            {
                All = new ComponentType[] { typeof(Sector), typeof(InnerDrawRangeSectorTag), typeof(ReadyForWorldMove) }
            };
            entitiesForMovingGroup = GetComponentGroup(entitiesForMovingQuery);
        }

        protected override void OnUpdate()
        {
            ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();
            entities = chunkIterationSystem.GetEntities(entitiesForMovingGroup, entityType);

            NativeArray<EntityRemapUtility.EntityRemapInfo> remapping = tGenEntityManager.CreateEntityRemapArray(Allocator.TempJob);

            EntityManager entityManager = Bootstrapped.defaultWorld.GetExistingManager<EntityManager>();
            entityManager.MoveEntitiesFrom(tGenEntityManager, entitiesForMovingGroup, remapping);

            for (int i = 0; i < entities.Length; i++)
            {
                entities[i] = EntityRemapUtility.RemapEntity(ref remapping, entities[i]);

            }

            remapping.Dispose();
            entities.Dispose();
        }
    }



}