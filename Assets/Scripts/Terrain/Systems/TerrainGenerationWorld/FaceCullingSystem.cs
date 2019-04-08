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
    [UpdateAfter(typeof(VoxelNeighboursSystem))]
    public class FaceCullingSystem : JobComponentSystem
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
                All = new ComponentType[] { typeof(Voxel), typeof(VoxelVisibleFaces), typeof(GetVisibleFaces) },
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
            new RemoveGetVisibleFacesTagJob
            {
                EntitiesForTagChange = EntitiesForTagChange,
                ECBuffer = tGBuffer.CreateCommandBuffer()

            }.Schedule(new GetVisibleFacesJob
            {
                EntitiesForTagChange = EntitiesForTagChange.ToConcurrent(),
                TextureAtlasSettings = new TextureAtlasSettings(),
                Matrix = Data.Store.viewZoneMatrix,
                CubeDirections = cubeDirections,
                VoxelGeographyBufferFromVoxelEntity = GetBufferFromEntity<VoxelGeology>(true)

            }.Schedule(myQuery, inputDeps));

            tGBuffer.AddJobHandleForProducer(handle);
            return handle;
        }
    }
}

//[BurstCompile]
[ExcludeComponent(typeof(VoxelIsNotInDrawRange))]
[RequireComponentTag(typeof(GetVisibleFaces))]
public struct GetVisibleFacesJob : IJobForEachWithEntity<Voxel, VoxelVisibleFaces>
{
    public NativeQueue<Entity>.Concurrent EntitiesForTagChange;
    public TextureAtlasSettings TextureAtlasSettings;
    [ReadOnly]
    public Matrix3D<Entity> Matrix;
    [ReadOnly]
    public CubeDirections CubeDirections;
    [ReadOnly]
    public BufferFromEntity<VoxelGeology> VoxelGeographyBufferFromVoxelEntity;


    public void Execute(Entity entity, int index, ref Voxel voxel, ref VoxelVisibleFaces visibleFaces)
    {
        if (voxel.GeologyID == 1) // our current voxel is Air, therefore it needs no faces
        {
            visibleFaces.north = 0;
            visibleFaces.south = 0;
            visibleFaces.east = 0;
            visibleFaces.west = 0;
            visibleFaces.up = 0;
            visibleFaces.down = 0;
        }
        else
        {
            visibleFaces.north = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[0])][0].ID);
            visibleFaces.south = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[1])][0].ID);
            visibleFaces.east = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[2])][0].ID);
            visibleFaces.west = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[3])][0].ID);
            visibleFaces.up = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[4])][0].ID);
            visibleFaces.down = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[5])][0].ID);
        }

        EntitiesForTagChange.Enqueue(entity);
    }
}

// Not burstable
[RequireComponentTag(typeof(GetVisibleFaces))]
public struct RemoveGetVisibleFacesTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForTagChange;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagChange.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GetVisibleFaces));
            ECBuffer.AddComponent(myEntity, new GetMeshData());
        }
    }
}