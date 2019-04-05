using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[RequireComponentTag (typeof (SectorRemoveTag))]
public struct RemoveSectorJob : IJobProcessComponentDataWithEntity<Sector>
{
    [NativeDisableParallelForRestriction]
    public EntityCommandBuffer ECBuffer;

    public void Execute (Entity sectorEntity, int index, ref Sector sector)
    {
        ECBuffer.DestroyEntity (sectorEntity);
    }
}


[BurstCompile]
[RequireComponentTag(typeof(GetSectorDrawRange))]
public struct SectorDrawRangeJob : IJobProcessComponentDataWithEntity<Sector, Translation, SectorDrawRange>
{
    public NativeQueue<Entity>.Concurrent TagRemovalQueue;
    [ReadOnly] public Util Util;
    [ReadOnly] public int SectorSize;
    [ReadOnly] public int Range;
    [ReadOnly] public int3 PlayersCurrentSector;

    public void Execute(Entity entity, int index, ref Sector sector, ref Translation translation, ref SectorDrawRange drawRange)
    {
        int3 position = (int3)translation.Value;

        if (Util.SectorMatrixInRangeFromWorldPositionCheck(PlayersCurrentSector, position, PlayersCurrentSector, Range - 5, SectorSize))
        {
            if (drawRange.sectorDrawRange == 0) return;

            drawRange.sectorDrawRange = 0;
        }
        else if (Util.SectorMatrixInRangeFromWorldPositionCheck(PlayersCurrentSector, position, PlayersCurrentSector, Range - 3, SectorSize))
        {
            if (drawRange.sectorDrawRange == 1) return;

            drawRange.sectorDrawRange = 1;
        }
        else if (Util.SectorMatrixInRangeFromWorldPositionCheck(PlayersCurrentSector, position, PlayersCurrentSector, Range - 1, SectorSize))
        {
            if (drawRange.sectorDrawRange == 2) return;

            drawRange.sectorDrawRange = 2;
        }
        else
        {
            if (drawRange.sectorDrawRange == 3) return;

            drawRange.sectorDrawRange = 3;
        }

        TagRemovalQueue.Enqueue(entity);
    }
}

// Not burstable
public struct RemoveGetSectorDrawRangeTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForTagRemoval;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagRemoval.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GetSectorDrawRange));
        }
    }
}

[BurstCompile]
[ExcludeComponent(typeof(InnerDrawRangeSectorTag), typeof(OuterDrawRangeSectorTag), typeof(EdgeOfDrawRangeSectorTag), typeof(NotInDrawRangeSectorTag))]
public struct SectorDrawRangeTagJob : IJobProcessComponentDataWithEntity<Sector, SectorDrawRange>
{
    public NativeQueue<Entity>.Concurrent EntitiesForApplyInnerDrawRangeTag;
    public NativeQueue<Entity>.Concurrent EntitiesForApplyOuterDrawRangeTag;
    public NativeQueue<Entity>.Concurrent EntitiesForApplyEdgeDrawRangeTag;
    public NativeQueue<Entity>.Concurrent EntitiesForApplyNotInDrawRangeTag;

    public void Execute(Entity sectorEntity, int index, ref Sector sector, ref SectorDrawRange drawRange)
    {
        if (drawRange.sectorDrawRange == 0)
        {
            EntitiesForApplyInnerDrawRangeTag.Enqueue(sectorEntity);
        }
        else if (drawRange.sectorDrawRange == 1)
        {
            EntitiesForApplyOuterDrawRangeTag.Enqueue(sectorEntity);
        }
        else if (drawRange.sectorDrawRange == 2)
        {
            EntitiesForApplyEdgeDrawRangeTag.Enqueue(sectorEntity);
        }
        else
        {
            EntitiesForApplyNotInDrawRangeTag.Enqueue(sectorEntity);
        }
    }
}

// Not burstable
public struct ApplyNewSectorDrawRangeTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForApplyInnerDrawRangeTag;
    public NativeQueue<Entity> EntitiesForApplyOuterDrawRangeTag;
    public NativeQueue<Entity> EntitiesForApplyEdgeDrawRangeTag;
    public NativeQueue<Entity> EntitiesForApplyNotInDrawRangeTag;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForApplyInnerDrawRangeTag.TryDequeue(out Entity myEntity))
        {
            ECBuffer.AddComponent(myEntity, new InnerDrawRangeSectorTag());
        }
        while (EntitiesForApplyOuterDrawRangeTag.TryDequeue(out Entity myEntity))
        {
            ECBuffer.AddComponent(myEntity, new OuterDrawRangeSectorTag());

        }
        while (EntitiesForApplyEdgeDrawRangeTag.TryDequeue(out Entity myEntity))
        {
            ECBuffer.AddComponent(myEntity, new EdgeOfDrawRangeSectorTag());
        }
        while (EntitiesForApplyNotInDrawRangeTag.TryDequeue(out Entity myEntity))
        {
            ECBuffer.AddComponent(myEntity, new NotInDrawRangeSectorTag());
        }
    }
}

[BurstCompile]
[RequireComponentTag(typeof(GetSectorNoise))]
public struct SectorNoiseJob : IJobProcessComponentDataWithEntity<Sector, Translation>
{
    [NativeDisableParallelForRestriction]
    public BufferFromEntity<WorleySurfaceNoise> SurfaceNoiseBufferFrom;

    [ReadOnly] public Util Util;
    [ReadOnly] public int SectorSize;
    [ReadOnly] public WorleyNoiseGenerator Noise;


    public void Execute(Entity sectorEntity, int index, ref Sector sector, ref Translation position)
    {
        DynamicBuffer<WorleySurfaceNoise> surfaceNoiseBuffer = SurfaceNoiseBufferFrom[sectorEntity];

        int requiredSize = (int)math.pow(SectorSize, 2);

        if (surfaceNoiseBuffer.Length < requiredSize)
            surfaceNoiseBuffer.ResizeUninitialized(requiredSize);

        float3 sectorPos = position.Value;

        for (int i = 0; i < surfaceNoiseBuffer.Length; i++)
        {
            float3 sCellWPos = Util.Unflatten2D(i, SectorSize) + new float3(sectorPos.x, 0, sectorPos.z);  // remove Y

            WorleySurfaceNoise worleySurfaceNoise = Noise.GetEdgeData(sCellWPos.x, sCellWPos.z);
            surfaceNoiseBuffer[i] = worleySurfaceNoise;
        }
    }
}

[BurstCompile]
[RequireComponentTag (typeof (GetSectorNoise))]
public struct SectorGetIndividualCellsJob : IJobProcessComponentDataWithEntity<Sector, Translation>
{
    [NativeDisableParallelForRestriction]
    public BufferFromEntity<SurfaceCell> SurfaceCellBufferFrom;

    public NativeQueue<Entity>.Concurrent SectorEntities;

    [ReadOnly]
    public BufferFromEntity<WorleySurfaceNoise> SurfaceNoiseBufferFrom;

    public void Execute (Entity sectorEntity, int index, ref Sector sector, ref Translation sectorWPos)
    {
        DynamicBuffer<SurfaceCell> surfaceCellBuffer = SurfaceCellBufferFrom [sectorEntity];
        DynamicBuffer<WorleySurfaceNoise> surfaceNoiseBuffer = SurfaceNoiseBufferFrom [sectorEntity];

        if (surfaceCellBuffer.Length < surfaceNoiseBuffer.Length)
            surfaceCellBuffer.ResizeUninitialized (surfaceNoiseBuffer.Length);

        for (int c = 0; c < surfaceNoiseBuffer.Length; c++)
        {
            WorleySurfaceNoise worleyNoise = surfaceNoiseBuffer [c];

            SurfaceCell cell = new SurfaceCell
            {
                surfaceGenValue = worleyNoise.currentSurfaceCellValue,
                indexInBuffer = worleyNoise.currentCellIndexInMap,
                position = worleyNoise.currentCellPosition
            };
            surfaceCellBuffer [c] = cell;
        }
        SectorEntities.Enqueue (sectorEntity);
    }
}

// Not burstable
public struct RemoveGetSectorNoiseTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForTagRemoval;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagRemoval.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GetSectorNoise));
        }
    }
}