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
[RequireComponentTag (typeof (GetSectorNoise))]
public struct SectorNoiseJob : IJobProcessComponentDataWithEntity<Sector, Translation>
{
    [NativeDisableParallelForRestriction]
    public BufferFromEntity<WorleySurfaceNoise> SurfaceNoiseBufferFrom;

    [ReadOnly] public Util Util;
    [ReadOnly] public int SectorSize;
    [ReadOnly] public WorleyNoiseGenerator Noise;


    public void Execute (Entity sectorEntity, int index, ref Sector sector, ref Translation position)
    {
        DynamicBuffer<WorleySurfaceNoise> surfaceNoiseBuffer = SurfaceNoiseBufferFrom [sectorEntity];

        int requiredSize = (int) math.pow (SectorSize, 2);

        if (surfaceNoiseBuffer.Length < requiredSize)
            surfaceNoiseBuffer.ResizeUninitialized (requiredSize);

        float3 sectorPos = position.Value;

        for (int i = 0; i < surfaceNoiseBuffer.Length; i++)
        {
            float3 sCellWPos = Util.Unflatten2D (i, SectorSize) + new float3 (sectorPos.x, 0, sectorPos.z);  // remove Y

            WorleySurfaceNoise worleySurfaceNoise = Noise.GetEdgeData (sCellWPos.x, sCellWPos.z);
            surfaceNoiseBuffer [i] = worleySurfaceNoise;
        }
    }
}


[BurstCompile]
[RequireComponentTag (typeof (GetSectorNoise))]
public struct SectorGetIndividualCellsJob : IJobProcessComponentDataWithEntity<Sector, Translation>
{
    [NativeDisableParallelForRestriction]
    public BufferFromEntity<SurfaceCell> SurfaceCellBufferFrom;
    [NativeDisableParallelForRestriction]
    public NativeQueue<Entity> SectorEntities;

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


[BurstCompile]
public struct GetUniqueSurfaceCellsJob : IJob
{
    [ReadOnly]
    public NativeArray<Entity> Entities;

    [NativeDisableParallelForRestriction]
    public BufferFromEntity<SurfaceCell> SurfaceCellBufferFrom;
    [NativeDisableParallelForRestriction]
    public NativeList<SurfaceCell> UniqueCellList;
    [NativeDisableParallelForRestriction]
    public NativeQueue<Entity> EntitiesForTagRemoval;

    public void Execute ()
    {
        for (int e = 0; e < Entities.Length; e++)
        {
            Entity sectorEntity = Entities[e];

            DynamicBuffer<SurfaceCell> surfaceCellBuffer = SurfaceCellBufferFrom[sectorEntity];

            for (int i = 0; i < surfaceCellBuffer.Length; i++)
            {
                SurfaceCell currentCell = surfaceCellBuffer[i];

                if (!UniqueSurfaceCellsBufferContainsCurrentCell(UniqueCellList, currentCell))
                {
                    UniqueCellList.Add(currentCell);
                }
            }

            surfaceCellBuffer.CopyFrom(UniqueCellList);
            UniqueCellList.Clear();

            EntitiesForTagRemoval.Enqueue(sectorEntity);
        }
    }

    bool UniqueSurfaceCellsBufferContainsCurrentCell (NativeList<SurfaceCell> UniqueCellList, SurfaceCell currentCell)
    {
        for (int i = 0; i < UniqueCellList.Length; i++)
        {
            SurfaceCell checkCell = UniqueCellList[i];
            if (checkCell.indexInBuffer.Equals(currentCell.indexInBuffer))
            {
                return true;
            }
        }
        return false;
    }
}


[BurstCompile]
[ExcludeComponent(typeof(GetSectorNoise))]
[RequireComponentTag (typeof (GetSectorTopography))]
public struct SectorTopographyJob : IJobProcessComponentDataWithEntity<Sector>
{
    [ReadOnly]
    public BufferFromEntity<WorleySurfaceNoise> SurfaceNoiseBufferFrom;
    [NativeDisableParallelForRestriction]
    public BufferFromEntity<Topography> TopographyBufferFrom;
    [NativeDisableParallelForRestriction]
    public NativeQueue<Entity> EntitiesForTagRemoval;

    public Util Util;
    public TopographyTypeUtil TTUtil;
    public int SectorSize;
    public int SeaLevel;
    public int MinSurfaceHeight;
    public int MaxSurfaceHeight;

    public void Execute (Entity sectorEntity, int index, ref Sector sector)
    {
        DynamicBuffer<WorleySurfaceNoise> surfaceCellBuffer = SurfaceNoiseBufferFrom [sectorEntity];
        DynamicBuffer<Topography> topographyBuffer = TopographyBufferFrom [sectorEntity];
        topographyBuffer.ResizeUninitialized ((int) math.pow (SectorSize, 2));

        for (int i = 0; i < topographyBuffer.Length; i++)
        {
            int3 worldPosition = (int3) (sector.worldPosition + Util.Unflatten2D (i, SectorSize));

            Topography heightComponent = GetStartingSurfaceCellHeight (surfaceCellBuffer [i], worldPosition);

            topographyBuffer [i] = heightComponent;  
        }
        EntitiesForTagRemoval.Enqueue(sectorEntity);
    }

    public Topography GetStartingSurfaceCellHeight (WorleySurfaceNoise surfaceCell, int3 worldPosition)
    {
        float surfaceHeight = 128;  // start min surface height
        float adjSurfaceHeight = 128;

        //float modifier = (MaxSurfaceHeight - MinSurfaceHeight) * surfaceCell.currentSurfaceCellValue;
        //float adjModifier = (MaxSurfaceHeight - MinSurfaceHeight) * surfaceCell.adjacentSurfaceCellValue;

        //surfaceHeight += modifier;
        //adjSurfaceHeight += adjModifier;

        return new Topography
        {
            surfaceHeight = surfaceHeight,
            dist2Edge = surfaceCell.distance2Edge,
            adjSurfaceHeight = adjSurfaceHeight,
        };
    }
}


// moved SectorGeologyJob to its own script as access to it may be frequent for updating/modifying methods







