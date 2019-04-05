using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
[ExcludeComponent(typeof(GetSectorTopography))]
[RequireComponentTag (typeof(GenerateSectorGeology))]
public struct SectorGeologyJob : IJobProcessComponentDataWithEntity<Sector>
{
    public NativeQueue<Entity>.Concurrent EntitiesForTagRemoval;

    [ReadOnly]
    public BufferFromEntity<Topography> TopographyBufferFromSectorEntity;

    [NativeDisableParallelForRestriction]
    public BufferFromEntity<Block> BlockBufferFromSectorEntity;

    public Util Util;
    public int SectorSize;
    public int DirtLayerThickness;


    public void Execute(Entity sectorEntity, int index, ref Sector sector)
    {
        DynamicBuffer<Topography> topographyBuffer = TopographyBufferFromSectorEntity[sectorEntity];
        DynamicBuffer<Block> blockBuffer = BlockBufferFromSectorEntity[sectorEntity];
        blockBuffer.ResizeUninitialized((int)math.pow(SectorSize, 3));

        float3 sectorWorldPosition = sector.worldPosition;

        for (int b = 0; b < blockBuffer.Length; b++)
        {
            float3 blockLocalPosition = Util.UnflattenToFloat3(b, SectorSize);
            float3 blockWorldPosition = blockLocalPosition + sector.worldPosition;

            int topographyIndex = Util.Flatten2D(blockLocalPosition, SectorSize);

            int SurfaceHeight = (int)topographyBuffer[topographyIndex].surfaceHeight;

            ushort atlasID;

            if (blockWorldPosition.y < 5)
            {
                atlasID = (ushort)TextureAtlasSettings.ID.BEDROCK;
            }
            else if (blockWorldPosition.y < SurfaceHeight - DirtLayerThickness)
            {
                atlasID = (ushort)TextureAtlasSettings.ID.GRANITE;
            }
            else if (blockWorldPosition.y >= SurfaceHeight - DirtLayerThickness && blockWorldPosition.y < SurfaceHeight)
            {
                atlasID = (ushort)TextureAtlasSettings.ID.MUD;
            }
            else if (blockWorldPosition.y == SurfaceHeight)
            {
                atlasID = (ushort)TextureAtlasSettings.ID.GRASS;
            }
            else
            {
                atlasID = (ushort)TextureAtlasSettings.ID.AIR;
            }

            Block newBlock = new Block
            {
                atlasID = atlasID,
                blockIndex = b,
                localPosition = blockLocalPosition,
                worldPosition = blockWorldPosition,
                parentSectorEntity = sectorEntity,
                parentSectorWorldPosition = sectorWorldPosition
            };

            blockBuffer[b] = newBlock;
        }
        EntitiesForTagRemoval.Enqueue(sectorEntity);
    }
}

// Not burstable
[RequireComponentTag(typeof(GenerateSectorGeology))]
public struct RemoveGenerateSectorGeologyTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForTagRemoval;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagRemoval.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GenerateSectorGeology));
        }
    }
}