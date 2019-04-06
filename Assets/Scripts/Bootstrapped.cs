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
    public static World defaultWorld;
    public static World tGenWorld;
    public static TerrainGenerationGroup tGenGroup;
    public static TerrainGroup terrainGroup;
    public static Entity playerEntity;
    float3 playerStartPos;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public List<Type> Initialize(List<Type> systems)
    {
        playerStartPos = new float3(0, TerrainSettings.playerStartHeight, 0);
        SetupPlayers(playerStartPos);

        SetupWorldsAndUdateGroups();
        return systems;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene() { }

    void SetupPlayers(float3 startPos)
    {
            EntityManager entityManager = World.Active.GetOrCreateManager<EntityManager>();

            EntityArchetype playerSetup = PhysicsEntityFactory.CreatePlayerArchetype(entityManager);
            playerEntity = entityManager.CreateEntity(playerSetup);

            entityManager.SetComponentData(playerEntity, new Translation { Value = startPos });
            entityManager.SetComponentData(playerEntity, new Rotation { Value = quaternion.identity });
            entityManager.SetComponentData(playerEntity, new Velocity { Value = new float3(0, 0, 0) });

            RenderMesh renderer = new RenderMesh();
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            renderer.mesh = capsule.GetComponent<MeshFilter>().mesh;
            GameObject.Destroy(capsule);
            renderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/PlayerCapsuleMaterial.mat");

            entityManager.AddSharedComponentData(playerEntity, renderer);
    }

    void SetupWorldsAndUdateGroups()
    {
        defaultWorld = World.Active;

        // setup terrainSystems for default world
        terrainGroup = defaultWorld.GetOrCreateManager<TerrainGroup>();
        UpdateGroupCreator.FindandCreateGroup(defaultWorld, "Terrain", terrainGroup);

        var simGroup = defaultWorld.GetOrCreateManager<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(terrainGroup);
        simGroup.SortSystemUpdateList();

        // setup custom world and its systems
        tGenWorld = new World("TerrainGenWorld");
        tGenGroup = tGenWorld.GetOrCreateManager<TerrainGenerationGroup>();
        UpdateGroupCreator.FindandCreateGroup(tGenWorld, "TerrainGeneration", tGenGroup);

        var customSimGroup = tGenWorld.GetOrCreateManager<SimulationSystemGroup>();
        customSimGroup.AddSystemToUpdateList(tGenGroup);
        customSimGroup.SortSystemUpdateList();

    }
}

public class UpdateGroupCreator
{
    public static void FindandCreateGroup(World world, string name, ComponentSystemGroup customGroup)
    {
        World.Active = world;

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
                customGroup.AddSystemToUpdateList(GetOrCreateManagerAndLogException(world, type) as ComponentSystemBase);
            }
            customGroup.SortSystemUpdateList();
        }
    }

    public static ScriptBehaviourManager GetOrCreateManagerAndLogException(World world, Type type)
    {
        try
        {
            //Debug.Log("Found System    " + type.FullName   +" for world   " + world);
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
public class TerrainGenerationGroup : ComponentSystemGroup
{ }

[DisableAutoCreation]
[AlwaysUpdateSystem]
public class TerrainGroup : ComponentSystemGroup
{ }