using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(CameraFollowPlayerSystem))]
public class PlayerInputSystem : ComponentSystem
{
    EntityManager entityManager;

    public Entity playerEntity;
    Camera camera;


    protected override void OnCreateManager()
    {
        entityManager = World.EntityManager;

    }

    protected override void OnStartRunning()
    {
        camera = GameObject.FindObjectOfType<Camera>();
        playerEntity = Bootstrapped.playerEntity;
    }

    protected override void OnUpdate()
    {
        if (Time.fixedTime < 0.5) return;

        MovePlayer();

    }

    void MovePlayer()
    {
        float3 playerPosition = entityManager.GetComponentData<Translation>(playerEntity).Value;
        PhysicsEntity physicsComponent = entityManager.GetComponentData<PhysicsEntity>(playerEntity);
        Stats stats = entityManager.GetComponentData<Stats>(playerEntity);

        //  Camera forward ignoring x axis tilt
        float3 forward = math.normalize(playerPosition - new float3(camera.transform.position.x, playerPosition.y, camera.transform.position.z));

        //  Move relative to camera angle
        float3 x = UnityEngine.Input.GetAxis("Horizontal") * (float3)camera.transform.right;
        float3 z = UnityEngine.Input.GetAxis("Vertical") * forward;

        //  Update movement component
        float3 move = (x + z) * stats.speed;
        physicsComponent.positionChangePerSecond = new float3(move.x, 0, move.z);
        entityManager.SetComponentData(playerEntity, physicsComponent);

        float3 nextPosition = playerPosition + new float3(move.x, 0, move.z);
        entityManager.SetComponentData(playerEntity, new Translation{  Value = nextPosition});

    }
}