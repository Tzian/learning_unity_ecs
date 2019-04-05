using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace Terrain
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TerrainGroup))]
    [UpdateAfter(typeof(TerrainSystem))]
    public class InnerRangeApplyMeshSystem : ComponentSystem
    {
        EntityManager entityManager;

        ComponentGroup applyMeshGroup;
        public static Material material;

        protected override void OnCreateManager()
        {
            entityManager = World.GetOrCreateManager<EntityManager>();
            material = TerrainSettings.terrainMaterial;
            EntityArchetypeQuery applyMeshQuery = new EntityArchetypeQuery
            {
                All = new ComponentType[] { typeof(Sector), typeof(MeshVert), typeof(InnerDrawRangeSectorTag), typeof(ReadyForWorldMove) }
            };
            applyMeshGroup = GetComponentGroup(applyMeshQuery);
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer eCBuffer = new EntityCommandBuffer(Allocator.Temp);
            NativeArray<ArchetypeChunk> dataChunks = applyMeshGroup.CreateArchetypeChunkArray(Allocator.TempJob);

            ArchetypeChunkEntityType entityType = GetArchetypeChunkEntityType();
            ArchetypeChunkBufferType<MeshVert> vertType = GetArchetypeChunkBufferType<MeshVert>(true);
            ArchetypeChunkBufferType<MeshTri> triType = GetArchetypeChunkBufferType<MeshTri>(true);
            ArchetypeChunkBufferType<MeshUv> uvType = GetArchetypeChunkBufferType<MeshUv>(true);


            for (int c = 0; c < dataChunks.Length; c++)
            {
                ArchetypeChunk dataChunk = dataChunks[c];

                NativeArray<Entity> entities = dataChunk.GetNativeArray(entityType);

                BufferAccessor<MeshVert> vertBuffers = dataChunk.GetBufferAccessor(vertType);
                BufferAccessor<MeshTri> triBuffers = dataChunk.GetBufferAccessor(triType);
                BufferAccessor<MeshUv> uvBuffers = dataChunk.GetBufferAccessor(uvType);

                for (int e = 0; e < entities.Length; e++)
                {
                    Entity entity = entities[e];

                    bool redraw = entityManager.HasComponent<MeshRedraw>(entity);

                    Mesh mesh = MakeMesh(vertBuffers[e], triBuffers[e], uvBuffers[e]);
                    SetMeshComponent(redraw, mesh, entity, eCBuffer);

                    if (redraw) eCBuffer.RemoveComponent(entity, typeof(MeshRedraw));

                    eCBuffer.RemoveComponent(entity, typeof(MeshVert));
                    eCBuffer.RemoveComponent(entity, typeof(MeshTri));
                    eCBuffer.RemoveComponent(entity, typeof(MeshUv));
                    eCBuffer.RemoveComponent(entity, typeof(ReadyForWorldMove));
                }
            }

            eCBuffer.Playback(entityManager);
            eCBuffer.Dispose();

            dataChunks.Dispose();
        }

        Mesh MakeMesh(DynamicBuffer<MeshVert> vertices, DynamicBuffer<MeshTri> triangles, DynamicBuffer<MeshUv> uvs)
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

            mesh.RecalculateNormals();
            return mesh;
        }

        // Apply mesh
        void SetMeshComponent(bool redraw, Mesh mesh, Entity entity, EntityCommandBuffer eCBuffer)
        {
            if (redraw) eCBuffer.RemoveComponent<RenderMesh>(entity);

            RenderMesh renderer = new RenderMesh();
            renderer.mesh = mesh;
            renderer.material = material;

            eCBuffer.AddSharedComponent(entity, renderer);
        }
    }
}