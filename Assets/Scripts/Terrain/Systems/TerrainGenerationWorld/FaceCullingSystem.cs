using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


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
        NativeQueue<Entity> EntitiesForMeshComponentsAdding;
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
            if (EntitiesForMeshComponentsAdding.IsCreated)
            {
                EntitiesForMeshComponentsAdding.Dispose();
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
            EntitiesForMeshComponentsAdding = new NativeQueue<Entity>(Allocator.TempJob);


            var handle =
            new RemoveGetVisibleFacesTagJob
            {
                EntitiesForTagChange = EntitiesForTagChange,
                EntitiesForMeshComponentsAdding = EntitiesForMeshComponentsAdding,
                ECBuffer = tGBuffer.CreateCommandBuffer()

            }.Schedule(new GetVisibleFacesJob
            {
                EntitiesForTagChange = EntitiesForTagChange.ToConcurrent(),
                EntitiesForMeshComponentsAdding = EntitiesForMeshComponentsAdding.ToConcurrent(),
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
    public NativeQueue<Entity>.Concurrent EntitiesForMeshComponentsAdding;

    public TextureAtlasSettings TextureAtlasSettings;
    [ReadOnly]
    public Matrix3D<Entity> Matrix;
    [ReadOnly]
    public CubeDirections CubeDirections;
    [ReadOnly]
    public BufferFromEntity<VoxelGeology> VoxelGeographyBufferFromVoxelEntity;


    public void Execute(Entity entity, int index, ref Voxel voxel, ref VoxelVisibleFaces visibleFaces)
    {
        //	Count vertices and triangles
        int facesCount = 0;
        int vertCount = 0;
        int triCount = 0;
        int uvCount = 0;

        if (voxel.GeologyID == 1) // our current voxel is Air, therefore it needs no faces
        {
            visibleFaces.north = 0;
            visibleFaces.south = 0;
            visibleFaces.east = 0;
            visibleFaces.west = 0;
            visibleFaces.up = 0;
            visibleFaces.down = 0;

            visibleFaces.faceCount = 0;
            visibleFaces.vertexCount = 0;
            visibleFaces.triangleCount = 0;
            visibleFaces.uvsCount = 0;
        }
        else
        {
            visibleFaces.north = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[0])][0].ID);
            if (visibleFaces.north == 1)
            {
                facesCount = facesCount + 1;
                vertCount = vertCount + 4;
                triCount = triCount + 6;
                uvCount = uvCount + 4;
            }
            visibleFaces.south = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[1])][0].ID);
            if (visibleFaces.south == 1)
            {
                facesCount = facesCount + 1;
                vertCount = vertCount + 4;
                triCount = triCount + 6;
                uvCount = uvCount + 4;
            }
            visibleFaces.east = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[2])][0].ID);
            if (visibleFaces.east == 1)
            {
                facesCount = facesCount + 1;
                vertCount = vertCount + 4;
                triCount = triCount + 6;
                uvCount = uvCount + 4;
            }
            visibleFaces.west = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[3])][0].ID);
            if (visibleFaces.west == 1)
            {
                facesCount = facesCount + 1;
                vertCount = vertCount + 4;
                triCount = triCount + 6;
                uvCount = uvCount + 4;
            }
            visibleFaces.up = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[4])][0].ID);
            if (visibleFaces.up == 1)
            {
                facesCount = facesCount + 1;
                vertCount = vertCount + 4;
                triCount = triCount + 6;
                uvCount = uvCount + 4;
            }
            visibleFaces.down = TextureAtlasSettings.IsTransparent(VoxelGeographyBufferFromVoxelEntity[Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[5])][0].ID);
            if (visibleFaces.down == 1)
            {
                facesCount = facesCount + 1;
                vertCount = vertCount + 4;
                triCount = triCount + 6;
                uvCount = uvCount + 4;
            }

            visibleFaces.faceCount = facesCount;
            visibleFaces.vertexCount = vertCount;
            visibleFaces.triangleCount = triCount;
            visibleFaces.uvsCount = uvCount;

            if (facesCount > 0)
            {
                EntitiesForMeshComponentsAdding.Enqueue(entity);
            }
        }
        EntitiesForTagChange.Enqueue(entity);
    }
}

// Not burstable
[RequireComponentTag(typeof(GetVisibleFaces))]
public struct RemoveGetVisibleFacesTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForTagChange;
    public NativeQueue<Entity> EntitiesForMeshComponentsAdding;

    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagChange.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GetVisibleFaces));
        }
        while (EntitiesForMeshComponentsAdding.TryDequeue(out Entity meshingEntity))
        {
            ECBuffer.AddBuffer<Verts>(meshingEntity);
            ECBuffer.AddBuffer<Tris>(meshingEntity);
            ECBuffer.AddBuffer<Uvs>(meshingEntity);
            ECBuffer.AddComponent(meshingEntity, new GetMeshData());

        }
    }
}