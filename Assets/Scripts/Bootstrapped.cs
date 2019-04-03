using PhysicsEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public class Bootstrapped : ICustomBootstrap
{
    public static TerrainGenerationGroup tGenGroup;

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
    public List<Type> Initialize(List<Type> systems)
    {
        float3 playerStartPos = new float3(0, TerrainSettings.playerStartHeight, 0);
        Entity playerEntity = CreatePlayer(playerStartPos);
        TerrainSystem.playerEntity = playerEntity;


        //Debug.Log("world before we create custom worlds " + World.Active);
        Worlds worlds = new Worlds();
        Worlds.defaultWorld = World.Active;

        World tGenWorld = new World("TerrainGenWorld");
        tGenGroup = tGenWorld.GetOrCreateManager<TerrainGenerationGroup>();
        WorldCreator.FindAndCreateWorldFromNamespace(tGenWorld, "TerrainGen", tGenGroup);
        Worlds.tGenWorld = tGenWorld;

        //Debug.Log(" all worlds in Worlds - default world = " + worlds.defaultWorld + "custom1 = " + worlds.tGenWorld);
        var simGroup = Worlds.defaultWorld.GetOrCreateManager<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(tGenGroup);
        simGroup.SortSystemUpdateList();
        return systems;
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

public class WorldCreator
{
    public static void FindAndCreateWorldFromNamespace(World customWorld, string name, ComponentSystemGroup customGroup)
    {

        World.Active = customWorld;

        foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (ass.ManifestModule.ToString() == "Microsoft.CodeAnalysis.Scripting.dll")
                continue;
            var allTypes = ass.GetTypes();

            var systemTypes = allTypes.Where(
                t => t.IsSubclassOf(typeof(ComponentSystemBase)) &&
                !t.IsAbstract &&
                !t.ContainsGenericParameters &&
                (t.Namespace != null && t.Namespace == name));


            foreach (var type in systemTypes)
            {
                customGroup.AddSystemToUpdateList(GetOrCreateManagerAndLogException(customWorld, type) as ComponentSystemBase);
            }

            customGroup.SortSystemUpdateList();
        }
    }

    public static ScriptBehaviourManager GetOrCreateManagerAndLogException(World world, Type type)
    {
        try
        {
            // Debug.Log("Found System" + type.FullName);
            return world.GetOrCreateManager(type);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }
}

[DisableAutoCreation]
[AlwaysUpdateSystem]
public class TerrainGenerationGroup : ComponentSystemGroup
{ }