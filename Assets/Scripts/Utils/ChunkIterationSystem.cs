using Unity.Collections;
using Unity.Entities;

[DisableAutoCreation]
public struct ChunkIterationSystem 
{
    // this takes in the compGroup and entityType, and gives us out all entities from all chunks within the group. 
    public NativeArray<Entity> GetEntities (ComponentGroup compGroup, ArchetypeChunkEntityType entityType)
    {
        NativeArray<ArchetypeChunk> dataChunks = compGroup.CreateArchetypeChunkArray (Allocator.TempJob);

        NativeList<ArchetypeChunk> chunkList = new NativeList<ArchetypeChunk> (Allocator.Temp);

        NativeList<Entity> entityList = new NativeList<Entity> (Allocator.Temp);

        for (int d = 0; d < dataChunks.Length; d++)
        {
            chunkList.Add (dataChunks [d]);
        }
        dataChunks.Dispose ();

        for (int c = 0; c < chunkList.Length; c++)
        {
            ArchetypeChunk dataChunk = chunkList [c];

            NativeArray<Entity> entityHolder = dataChunk.GetNativeArray (entityType);

            for (int i = 0; i < entityHolder.Length; i++)
            {
                entityList.Add (entityHolder [i]);
            }
        }
        chunkList.Dispose ();

        NativeArray<Entity> chunkEntities = new NativeArray<Entity> (entityList.Length, Allocator.Persistent);
        chunkEntities.CopyFrom (entityList);

        entityList.Dispose ();
        return chunkEntities;
    }
}