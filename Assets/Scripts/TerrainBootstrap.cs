using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public class TerrainBootstrap : ICustomBootstrap
{
    public static TerrainGenerationGroup tGenGroup;
    public static TerrainGroup terrainGroup;

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
    public List<Type> Initialize(List<Type> systems)
    {
       // Debug.Log("world before we create custom worlds " + World.Active);
        Worlds worlds = new Worlds();
        Worlds.defaultWorld = World.Active;

        // setup custom world and its systems
        World tGenWorld = new World("TerrainGenWorld");
        tGenGroup = tGenWorld.GetOrCreateManager<TerrainGenerationGroup>();
        UpdateGroupCreator.FindandCreateGroup(tGenWorld, "TerrainGen", tGenGroup);
        Worlds.tGenWorld = tGenWorld;

        // setup terrainSystems for default world
        terrainGroup = Worlds.defaultWorld.GetOrCreateManager<TerrainGroup>();
        UpdateGroupCreator.FindandCreateGroup(Worlds.defaultWorld, "Terrain", terrainGroup);

        var simGroup = Worlds.defaultWorld.GetOrCreateManager<SimulationSystemGroup>();
        simGroup.AddSystemToUpdateList(terrainGroup);
        simGroup.AddSystemToUpdateList(tGenGroup);
        simGroup.SortSystemUpdateList();
        return systems;
    }

    [RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene () { }
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
          //  Debug.Log("Found System    " + type.FullName   +" for world   " + world);
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
[UpdateAfter(typeof(TerrainGroup))]
public class TerrainGenerationGroup : ComponentSystemGroup
{ }

[DisableAutoCreation]
[AlwaysUpdateSystem]
[UpdateBefore(typeof(TerrainGenerationGroup))]
public class TerrainGroup : ComponentSystemGroup
{ }