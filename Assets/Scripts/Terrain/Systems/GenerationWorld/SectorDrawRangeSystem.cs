using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace TerrainGen
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(TerrainGenStartSystem))]
    public class SectorDrawRangeSystem : JobComponentSystem
    {
        EntityManager entityManager;
        public Entity playerEntity;
        int sectorSize;
        Util util;
        int range;

        protected override void OnCreateManager()
        {
            playerEntity = Bootstrapped.playerEntity;
            entityManager = World.GetOrCreateManager<EntityManager>();
            sectorSize = TerrainSettings.sectorSize;
            util = new Util();
            range = TerrainSettings.sectorGenerationRange;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.TempJob);
            var tagRemovalQueue = new NativeQueue<Entity>(Allocator.TempJob);

            new RemoveGetSectorDrawRangeTagJob
            {
                EntitiesForTagRemoval = tagRemovalQueue,
                ECBuffer = eCBuffer

            }.Schedule(new SectorDrawRangeJob
            {
                TagRemovalQueue = tagRemovalQueue.ToConcurrent(),
                Util = util,
                SectorSize = sectorSize,
                Range = range,
                PlayersCurrentSector = GetPlayersCurrentSector()

            }.Schedule(this, inputDeps)).Complete();
            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();
            tagRemovalQueue.Dispose();
            return inputDeps;
        }

        public int3 GetPlayersCurrentSector()
        {
            EntityManager eM = Bootstrapped.defaultWorld.GetExistingManager<EntityManager>();
            float3 playerPosition = eM.GetComponentData<Translation>(playerEntity).Value;
            float3 currentSector = util.GetSectorPosition(playerPosition, sectorSize);
            return (int3)currentSector;
        }
    }
}