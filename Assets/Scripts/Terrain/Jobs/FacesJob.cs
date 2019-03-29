using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

struct FacesJob : IJob
{
    public EntityCommandBuffer ECBuffer;

    [ReadOnly] public Entity entity;
    [ReadOnly] public Util util;
    [ReadOnly] public int sectorSize;

    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Block> current;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Block> northNeighbour;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Block> southNeighbour;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Block> eastNeighbour;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Block> westNeighbour;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Block> upNeighbour;
    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Block> downNeighbour;

    [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<float3> directions;


    public void Execute()
    {
        // add up all the blockFaces counts, for a total sector faces count
        SectorVisFacesCount sectorFacesCount;
        NativeArray<BlockFaces> facesCount = CheckBlockFaces(entity, out sectorFacesCount);

        ECBuffer.AddComponent(entity, sectorFacesCount);
        DynamicBuffer<BlockFaces> facesBuffer = ECBuffer.AddBuffer<BlockFaces>(entity);
        facesBuffer.CopyFrom(facesCount);

        ECBuffer.RemoveComponent(entity, typeof(DrawMeshTag));
        facesCount.Dispose();
    }

    NativeArray<BlockFaces> CheckBlockFaces(Entity entity, out SectorVisFacesCount counts)
    {
        BlockFaceChecker faceChecker = new BlockFaceChecker()
        {
            exposedFaces = new NativeArray<BlockFaces>(current.Length, Allocator.Temp),

            current = current,
            northNeighbour = northNeighbour,
            southNeighbour = southNeighbour,
            eastNeighbour = eastNeighbour,
            westNeighbour = westNeighbour,
            upNeighbour = upNeighbour,
            downNeighbour = downNeighbour,

            sectorSize = sectorSize,
            directions = directions,
            util = util
        };

        for (int i = 0; i < current.Length; i++)
        {
            faceChecker.Execute(i);
        }

        counts = CountExposedFaces(current, faceChecker.exposedFaces);
        return faceChecker.exposedFaces;
    }

    // goes over the array of block exposed faces counts adds them all up, returns total in an IComponentData to attach to the sector
    SectorVisFacesCount CountExposedFaces(NativeArray<Block> blocks, NativeArray<BlockFaces> exposedFaces)
    {
        //	Count vertices and triangles	
        int faceCount = 0;
        int vertCount = 0;
        int triCount = 0;
        int uvCount = 0;

        for (int i = 0; i < exposedFaces.Length; i++)
        {
            int count = exposedFaces[i].count;
            if (count > 0)
            {
                BlockFaces blockFaces = exposedFaces[i];

                //	Starting indices in mesh arrays
                blockFaces.faceIndex = faceCount;
                blockFaces.vertIndex = vertCount;
                blockFaces.triIndex = triCount;
                blockFaces.uvIndex = uvCount;

                exposedFaces[i] = blockFaces;

                for (int f = 0; f < 6; f++)
                {
                    switch (f)
                    {
                        case 0:  // Northface
                            if (blockFaces.north == 0) break;
                            
                            vertCount += 4;
                            triCount += 6;
                            uvCount += 4;
                            break;

                        case 1:  // Southface
                            if (blockFaces.south == 0) break;

                            vertCount += 4;
                            triCount += 6;
                            uvCount += 4;
                            break;

                        case 2:  // Eastface
                            if (blockFaces.east == 0) break;

                            vertCount += 4;
                            triCount += 6;
                            uvCount += 4;
                            break;

                        case 3:  // Westface
                            if (blockFaces.west == 0) break;

                            vertCount += 4;
                            triCount += 6;
                            uvCount += 4;
                            break;

                        case 4:  // Upface
                            if (blockFaces.up == 0) break;

                            vertCount += 4;
                            triCount += 6;
                            uvCount += 4;
                            break;

                        case 5:  // Downface
                            if (blockFaces.down == 0) break;

                            vertCount += 4;
                            triCount += 6;
                            uvCount += 4;
                            break;
                    }
                }
                faceCount += count;
            }
        }
        return new SectorVisFacesCount(faceCount, vertCount, triCount, uvCount);
    }
}