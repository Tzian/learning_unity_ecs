//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;


//namespace TerrainGeneration
//{
//    [CreateInWorld("TerrainGenerationWorld")]
//    [UpdateInGroup(typeof(TerrainGenerationGroup))]
//    [UpdateAfter(typeof(VoxelNeighboursSystem))]
//    public class FaceCullingSystem : JobComponentSystem
//    {
//        TerrainGenerationBuffer tGBuffer;
//        EntityQuery myQuery;
//        NativeQueue<Entity> EntitiesForTagChange;


//        protected override void OnCreateManager()
//        {
//            tGBuffer = World.GetOrCreateSystem<TerrainGenerationBuffer>();
//            myQuery = GetEntityQuery(new EntityQueryDesc
//            {
//                All = new ComponentType[] { typeof(Voxel), typeof(VoxelVisibleFaces), typeof(GetMeshData), typeof() },
//            });
//        }

//        protected void NativeCleanUp()
//        {
//            if (EntitiesForTagChange.IsCreated)
//            {
//                EntitiesForTagChange.Dispose();
//            }
//        }

//        protected override void OnStopRunning()
//        {
//            NativeCleanUp();
//        }

//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//            NativeCleanUp();

//            EntitiesForTagChange = new NativeQueue<Entity>(Allocator.TempJob);

//            var handle =
//            new Job2
//            {
//                EntitiesForTagChange = EntitiesForTagChange,
//                ECBuffer = tGBuffer.CreateCommandBuffer()

//            }.Schedule(new GetVisibleFacesJob
//            {
//                EntitiesForTagChange = EntitiesForTagChange.ToConcurrent(),

//            }.Schedule(myQuery, inputDeps));

//            tGBuffer.AddJobHandleForProducer(handle);
//            return handle;
//        }
//    }
//}

//[BurstCompile]
//[RequireComponentTag(typeof(GetMeshData))]
//public struct GetVisibleFacesJob : IJobForEachWithEntity<Voxel, VoxelVisibleFaces>
//{
//    public NativeQueue<Entity>.Concurrent EntitiesForTagChange;
//    public TextureAtlasSettings TextureAtlasSettings;
//    [ReadOnly]
//    public Matrix3D<Entity> Matrix;
//    [ReadOnly]
//    public CubeDirections CubeDirections;

//    public void Execute(Entity entity, int index, ref Voxel voxel, ref VoxelVisibleFaces voxelVisibleFaces)
//    {

//        voxelVisibleFaces.north = TextureAtlasSettings.IsTransparent(Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[0]));
//        voxelVisibleFaces.south = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[1]);
//        voxelVisibleFaces.east = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[2]);
//        voxelVisibleFaces.west = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[3]);
//        voxelVisibleFaces.up = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[4]);
//        voxelVisibleFaces.down = Matrix.GetItem((int3)voxel.WorldPosition + CubeDirections[5])

//        EntitiesForTagChange.Enqueue(entity);
//    }
//}

//// Not burstable
//[RequireComponentTag(typeof())]
//public struct Job2 : IJob
//{
//    public NativeQueue<Entity> EntitiesForTagChange;
//    public EntityCommandBuffer ECBuffer;

//    public void Execute()
//    {
//        while (EntitiesForTagChange.TryDequeue(out Entity myEntity))
//        {
//            ECBuffer.RemoveComponent(myEntity, typeof(tag1));
//            ECBuffer.AddComponent(myEntity, new (tag2 ));
//        }
//    }
//}