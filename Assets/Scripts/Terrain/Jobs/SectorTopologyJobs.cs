using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;



[BurstCompile]
[ExcludeComponent(typeof(GetSectorNoise))]
[RequireComponentTag(typeof(GetSectorTopography))]
public struct GetStartingTopograpnyJob : IJobProcessComponentDataWithEntity<Sector>
{
    public NativeQueue<Entity>.Concurrent EntitiesForTagRemoval;

    [ReadOnly]
    public BufferFromEntity<WorleySurfaceNoise> SurfaceNoiseBufferFrom;

    [NativeDisableParallelForRestriction]
    public BufferFromEntity<Topography> TopographyBufferFrom;

    public TopographyTypeUtil TTUtil;
    public Util Util;
    public int SectorSize;
    public int MinSurfaceHeight;
    public int MaxSurfaceHeight;

    public void Execute(Entity sectorEntity, int index, ref Sector sector)
    {

        DynamicBuffer<WorleySurfaceNoise> surfaceCellBuffer = SurfaceNoiseBufferFrom[sectorEntity];
        DynamicBuffer<Topography> topographyBuffer = TopographyBufferFrom[sectorEntity];
        topographyBuffer.ResizeUninitialized((int)math.pow(SectorSize, 2));

        for (int i = 0; i < topographyBuffer.Length; i++)
        {
            int3 worldPosition = (int3)(sector.worldPosition + Util.Unflatten2D(i, SectorSize));
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

// Not burstable
[RequireComponentTag(typeof(GetSectorTopography))]
public struct RemoveGetSectorTopographyTagJob : IJob
{
    public NativeQueue<Entity> EntitiesForTagRemoval;
    public EntityCommandBuffer ECBuffer;

    public void Execute()
    {
        while (EntitiesForTagRemoval.TryDequeue(out Entity myEntity))
        {
            ECBuffer.RemoveComponent(myEntity, typeof(GetSectorTopography));
        }
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

