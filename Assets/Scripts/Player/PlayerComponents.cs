using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;




    public struct Stats : IComponentData
    {
        public float speed;
    }

    public struct PhysicsEntity : IComponentData
    {
        public float3 positionChangePerSecond;
        public float3 size;
        public Entity currentVoxel;
    }

