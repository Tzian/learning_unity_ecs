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
public struct GetStartingTopograpnyJob : IJobProcessComponentDataWithEntity<Sector>
{
    [NativeDisableParallelForRestriction]
    public NativeQueue<Entity> EntitiesForTagRemoval;

    [ReadOnly]
    public BufferFromEntity<WorleySurfaceNoise> SurfaceNoiseBufferFrom;
    [NativeDisableParallelForRestriction]
    public BufferFromEntity<Topography> TopographyBufferFrom;

    public TopographyTypeUtil TTUtil;
    public Util Util;
    public int SectorSize;
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
            Topography heightComponent = GetStartingSurfaceCellHeight(surfaceCellBuffer[i], worldPosition);
            topographyBuffer[i] = heightComponent;
        }
        EntitiesForTagRemoval.Enqueue(sectorEntity);
    }

    public Topography GetStartingSurfaceCellHeight(WorleySurfaceNoise surfaceCell, int3 worldPosition)
    {
        float cellHeight = surfaceCell.distance2Edge;
        float scale = MaxSurfaceHeight * (cellHeight / 2);

        float surfaceHeight = scale;  // start min surface height
        float adjSurfaceHeight = scale;

        if (surfaceHeight < MinSurfaceHeight)
            surfaceHeight = MinSurfaceHeight;
        if (surfaceHeight > MaxSurfaceHeight)
            surfaceHeight = MaxSurfaceHeight;

        return new Topography
        {
            surfaceHeight = surfaceHeight,
            dist2Edge = surfaceCell.distance2Edge,
            adjSurfaceHeight = adjSurfaceHeight,
            topopgraphyType = (int)TTUtil.TerrainType(surfaceCell.currentSurfaceCellValue),
            adjTopographyType = (int)TTUtil.TerrainType(surfaceCell.adjacentSurfaceCellValue),

        };
    }
}

//[ExcludeComponent(typeof(GetSectorNoise))]
//[RequireComponentTag(typeof(GetSectorTopography))]
//public struct SectorAddNoiseToSurfaceHeightJob : IJobProcessComponentDataWithEntity<Sector>
//{

//    [NativeDisableParallelForRestriction]
//    public BufferFromEntity<Topography> TopographyBufferFrom;

//    public TopographyTypeUtil TTUtil;
//    public Util Util;
//    public int SectorSize;

//    public void Execute(Entity sectorEntity, int index, ref Sector sector)
//    {
//        DynamicBuffer<Topography> topographyBuffer = TopographyBufferFrom[sectorEntity];

//        for (int i = 0; i < topographyBuffer.Length; i++)
//        {
//            int3 worldPosition = (int3)(sector.worldPosition + Util.Unflatten2D(i, SectorSize));
//            Topography heightComponent = topographyBuffer[i];

//            TopographyTypes currentTopographyType = (TopographyTypes)heightComponent.topopgraphyType;
//            TopographyTypes adjacentTopographyType = (TopographyTypes)heightComponent.adjTopographyType;

//            float surfaceHeight = heightComponent.surfaceHeight;
//            float adjSurfaceHeight = heightComponent.adjSurfaceHeight;

//            float heightDifference = math.abs(surfaceHeight - adjSurfaceHeight);
//            float blockDistToEdge = heightComponent.dist2Edge;

//            float heightIncrement = 1 / heightDifference;
//            float heightAdjust = heightIncrement / blockDistToEdge;

//            surfaceHeight += heightAdjust;
//            //float modifier = GetNoise(currentTopographyType, worldPosition);
//            //float adjModifier = GetNoise(adjacentTopographyType, worldPosition);

//            ////surfaceHeight += modifier;
//            //adjSurfaceHeight += adjModifier;

            
//            Topography newTopography = new Topography
//            {
//                surfaceHeight = surfaceHeight,
//                topopgraphyType = (int)currentTopographyType,
//                adjSurfaceHeight = adjSurfaceHeight,
//                adjTopographyType = (int)adjacentTopographyType,
//                dist2Edge = heightComponent.dist2Edge
//            };
//            topographyBuffer[i] = newTopography;

//        }
//        
//    }

//    float GetNoise(TopographyTypes topographyType, float3 worldPosition)
//    {
//        float modifier = 0;

//        TopographyTypeStats topographyStats = TTUtil.GetTerrainTypeStats(topographyType);

//        modifier += TTUtil.AddNoise(topographyStats, (int)worldPosition.x, (int)worldPosition.z);

//        return modifier;
//    }
//}






