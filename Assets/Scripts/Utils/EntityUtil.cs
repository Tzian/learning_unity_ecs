using Unity.Entities;


public struct EntityUtil
{
    EntityManager entityManager;

    public EntityUtil (EntityManager entityManager)
    {
        this.entityManager = entityManager;
    }

    public void AddComponent<T> (Entity entity)
    where T : struct, IComponentData
    {
        entityManager.AddComponent (entity, typeof (T));
    }

    public void TryAddComponent<T> (Entity entity)
    where T : struct, IComponentData
    {
        if (!entityManager.HasComponent<T> (entity))
            entityManager.AddComponent (entity, typeof (T));
    }

    public void TryAddComponent<T> (Entity entity, EntityCommandBuffer commandBuffer)
    where T : struct, IComponentData
    {
        if (!entityManager.HasComponent<T> (entity))
            commandBuffer.AddComponent<T> (entity, new T ());
    }

    public void TryRemoveComponent<T> (Entity entity)
   where T : struct, IComponentData
    {
        if (entityManager.HasComponent<T> (entity))
            entityManager.RemoveComponent<T> (entity);
    }

    public void TryRemoveComponent<T> (Entity entity, EntityCommandBuffer commandBuffer)
    where T : struct, IComponentData
    {
        if (entityManager.HasComponent<T> (entity))
            commandBuffer.RemoveComponent<T> (entity);
    }

    public void TryRemoveSharedComponent<T> (Entity entity, EntityCommandBuffer commandBuffer)
   where T : struct, ISharedComponentData
    {
        if (entityManager.HasComponent<T> (entity))
            commandBuffer.RemoveComponent<T> (entity);
    }

    public bool TryReplaceComponent<TRemove, TAdd> (Entity entity)
    where TRemove : struct, IComponentData
    where TAdd : struct, IComponentData
    {
        if (entityManager.HasComponent<TRemove> (entity))
        {
            entityManager.RemoveComponent<TRemove> (entity);
            entityManager.AddComponentData<TAdd> (entity, new TAdd ());
            return true;
        }
        else return false;
    }

    public bool TryReplaceComponent<TRemove, TAdd> (Entity entity, EntityCommandBuffer commandBuffer)
    where TRemove : struct, IComponentData
    where TAdd : struct, IComponentData
    {
        if (entityManager.HasComponent<TRemove> (entity))
        {
            commandBuffer.RemoveComponent<TRemove> (entity);
            commandBuffer.AddComponent<TAdd> (entity, new TAdd ());
            return true;
        }
        else return false;
    }
}