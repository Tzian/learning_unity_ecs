using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


namespace Terrain
{
    [UpdateInGroup(typeof(TerrainGroup))]
    public class TerrainMainStartSystem : JobComponentSystem
    {
       // TerrainBuffer tBuffer;
        EntityQuery myQuery;
        public Material material;


        protected override void OnCreateManager()
        {
           // tBuffer = World.GetOrCreateSystem<TerrainBuffer>();
            material = TerrainSettings.terrainMaterial;
            myQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(Voxel), typeof(ReadyToMesh), typeof(Verts), typeof(Tris), typeof(Uvs) },
            });
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.Temp);

            NativeArray<ArchetypeChunk> dataChunks = myQuery.CreateArchetypeChunkArray(Allocator.TempJob);

            ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();
            ArchetypeChunkBufferType<Verts> vertType = GetArchetypeChunkBufferType<Verts>(true);
            ArchetypeChunkBufferType<Tris> triType = GetArchetypeChunkBufferType<Tris>(true);
            ArchetypeChunkBufferType<Uvs> uvType = GetArchetypeChunkBufferType<Uvs>(true);


            for (int c = 0; c < dataChunks.Length; c++)
            {
                ArchetypeChunk dataChunk = dataChunks[c];

                NativeArray<Entity> entities = dataChunk.GetNativeArray(entityType);

                BufferAccessor<Verts> vertBuffers = dataChunk.GetBufferAccessor(vertType);
                BufferAccessor<Tris> triBuffers = dataChunk.GetBufferAccessor(triType);
                BufferAccessor<Uvs> uvBuffers = dataChunk.GetBufferAccessor(uvType);

                for (int e = 0; e < entities.Length; e++)
                {
                    Entity entity = entities[e];
                    eCBuffer.AddComponent(entity, new LocalToWorld());


                    Mesh mesh = MakeMesh(vertBuffers[e], triBuffers[e], uvBuffers[e]);
                    SetMeshComponent(mesh, entity, eCBuffer);
                    eCBuffer.RemoveComponent(entity, typeof(ReadyToMesh));
                }
            }
            eCBuffer.Playback(World.EntityManager);
            eCBuffer.Dispose();

            dataChunks.Dispose();
            return inputDeps;
        }

        Mesh MakeMesh(DynamicBuffer<Verts> vertices, DynamicBuffer<Tris> triangles, DynamicBuffer<Uvs> uvs)
        {
            //	Convert vertices and uvs 
            Vector3[] verticesArray = new Vector3[vertices.Length];
            int[] trianglesArray = new int[triangles.Length];
            Vector2[] uvsArray = new Vector2[uvs.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                verticesArray[i] = vertices[i].vertex;
                uvsArray[i] = uvs[i].uv;
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                trianglesArray[i] = triangles[i].triangle;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = verticesArray;
            mesh.uv = uvsArray;
            mesh.SetTriangles(trianglesArray, 0);

          //  mesh.RecalculateNormals();
          //  mesh.RecalculateBounds();
            return mesh;
        }

        // Apply mesh
        void SetMeshComponent(Mesh mesh, Entity entity, EntityCommandBuffer eCBuffer)
        {
            RenderMesh renderer = new RenderMesh();
            renderer.mesh = mesh;
            renderer.material = material;

            eCBuffer.AddSharedComponent(entity, renderer);
        }
    }
}

