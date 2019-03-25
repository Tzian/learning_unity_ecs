using PhysicsEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public class Bootstrapped
{
    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize ()
    {
        float3 playerStartPos = new float3 (0, 130f, 0);

        Entity playerEntity = CreatePlayer (playerStartPos);

        TerrainSystem.playerEntity = playerEntity;
    }

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene ()
    {

    }

    private static Entity CreatePlayer (float3 startPos)
    {
        EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager> ();

        EntityArchetype playerSetup = PhysicsEntityFactory.CreatePlayerArchetype (entityManager);
        Entity playerEntity = entityManager.CreateEntity (playerSetup);

        entityManager.SetComponentData (playerEntity, new Translation { Value = startPos });
        entityManager.SetComponentData (playerEntity, new Rotation { Value = quaternion.identity });
        entityManager.SetComponentData (playerEntity, new Velocity { Value = new float3 (0, 0, 0) });

        RenderMesh renderer = new RenderMesh ();
        GameObject capsule = GameObject.CreatePrimitive (PrimitiveType.Capsule);
        renderer.mesh = capsule.GetComponent<MeshFilter> ().mesh;
        GameObject.Destroy (capsule);
        renderer.material = AssetDatabase.LoadAssetAtPath<Material> ("Assets/Materials/PlayerCapsuleMaterial.mat");

        entityManager.AddSharedComponentData (playerEntity, renderer);
        return playerEntity;
    }
}