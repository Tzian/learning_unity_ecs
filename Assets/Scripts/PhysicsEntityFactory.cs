using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;

namespace PhysicsEngine
{
    public static class PhysicsEntityFactory
    {
        // player archetype
        public static EntityArchetype CreatePlayerArchetype(EntityManager entityManager)
        {
            return entityManager.CreateArchetype(
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadWrite<Rotation>(),
                ComponentType.ReadWrite<Velocity>(),
                ComponentType.ReadWrite<RenderMeshProxy>(),
                ComponentType.ReadWrite<LocalToWorld>(),
                ComponentType.ReadWrite<PhysicsEntity>(),
                ComponentType.ReadWrite<Stats>()
                // ComponentType.Create<PlayerInput>()



                );
        }
    }
}

public struct Velocity : IComponentData
{
    public float3 Value;
}

public struct LocalPlayer : ISharedComponentData
{
    public GameObjectEntity Value;
}