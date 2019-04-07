using PhysicsEngine;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public class Bootstrapped : CustomWorldBootstrap
{
    public static Entity playerEntity;
    float3 playerStartPos;


    public Bootstrapped()
    {
        //CreateDefaultWorld = true; // Disable default world creation
        //WorldOptions.Add(new WorldOption("My No System World"));
    }

    public override void PostInitialize()
    {
        DefaultWorld = World.Active;

        playerStartPos = new float3(0, TerrainSettings.playerStartHeight, 0);
        SetupPlayers(playerStartPos);
        StartViewZoneMatrix((int3)playerStartPos);

    }

    void SetupPlayers(float3 startPos)
    {
        EntityManager entityManager = World.Active.EntityManager;

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

    void StartViewZoneMatrix(int3 playersCurrentPosition)
    {
        int viewZoneWidth = TerrainSettings.areaGenerationRange * 2 + 1;
        Data.Store.viewZoneMatrix = new Matrix3D<Entity>(viewZoneWidth, Allocator.Persistent, playersCurrentPosition, 1);
    }
}


/// <summary>
/// Apply to a system, system group or buffer system 
/// to only create in specified world
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CreateInWorldAttribute : Attribute
{
    public string Name;
    public CreateInWorldAttribute(string name)
    {
        Name = name;
    }
}


[CreateInWorld("TerrainGenerationWorld")]
public class TerrainGenerationGroup : ComponentSystemGroup { }

[CreateInWorld("TerrainGenerationWorld")]
[UpdateAfter(typeof(TerrainGenerationGroup))]
public class TerrainGenerationBuffer : EntityCommandBufferSystem { }

public class TerrainGroup : ComponentSystemGroup { }
