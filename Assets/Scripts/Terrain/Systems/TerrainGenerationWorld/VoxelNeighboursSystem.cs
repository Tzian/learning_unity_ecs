using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace TerrainGeneration
{
    [CreateInWorld("TerrainGenerationWorld")]
    [UpdateInGroup(typeof(TerrainGenerationGroup))]
    [UpdateAfter(typeof(GeologySystem))]
    public class VoxelNeighboursSystem : JobComponentSystem
    {
        TerrainGenerationBuffer tGBuffer;
        EntityQuery myQuery;
        NativeQueue<Entity> EntitiesForTagChange;
        CubeDirections cubeDirections;


        protected override void OnCreateManager()
        {
            tGBuffer = World.GetOrCreateSystem<TerrainGenerationBuffer>();
            cubeDirections = new CubeDirections();
            myQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(VoxelIsNotInDrawRange) },
                All = new ComponentType[] { typeof(Voxel), typeof(VoxelNeighbours), typeof(GetAdjacentVoxels) },
            });
        }

        protected void NativeCleanUp()
        {
            if (EntitiesForTagChange.IsCreated)
            {
                EntitiesForTagChange.Dispose();
            }
        }

        protected override void OnStopRunning()
        {
            NativeCleanUp();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeCleanUp();

            EntitiesForTagChange = new NativeQueue<Entity>(Allocator.TempJob);


            var handle =
            new RemoveGetAdjacentVoxelsTagJob
            {
                EntitiesForTagChange = EntitiesForTagChange,
                ECBuffer = tGBuffer.CreateCommandBuffer()

            }.Schedule(new GetVoxelNeighboursJob
            {
                EntitiesForTagChange = EntitiesForTagChange.ToConcurrent(),
                Matrix = Data.Store.viewZoneMatrix,
                CubeDirections = cubeDirections,

            }.Schedule(myQuery, inputDeps));

            tGBuffer.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}

[BurstCompile]
[ExcludeComponent(typeof(VoxelIsNotInDrawRange))]
[RequireComponentTag(typeof(GetAdjacentVoxels))]
public struct GetVoxelNeighboursJob : IJobForEachWithEntity<Voxel, VoxelNeighbours>
{
    public NativeQueue<Entity>.Concurrent EntitiesForTagChange;
    [ReadOnly]
    public Matrix3D<Entity> Matrix;
    [ReadOnly]
    public CubeDirections CubeDirections;

    public void Execute(Entity entity, int index, ref Voxel voxel, ref VoxelNeighbours voxelNeighbours)
    {
        voxelNeighbours.north = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[0]);
        voxelNeighbours.south = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[1]);
        voxelNeighbours.east = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[2]);
        voxelNeighbours.west = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[3]);
        voxelNeighbours.up = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[4]);
        voxelNeighbours.down = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[5]);

        EntitiesForTagChange.Enqueue(entity);
    }
}

// Not burstable
[RequireComponentTag(typeof(GetAdjacentVoxels))]
public struct RemoveGetAdjacentVoxelsTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForTagChange;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagChange.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GetAdjacentVoxels));
            ECBuffer.AddComponent(myEntity, new GetMeshData());
        }
    }
}