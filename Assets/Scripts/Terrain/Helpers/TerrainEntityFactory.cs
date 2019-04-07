using Unity.Entities;
using Unity.Transforms;

namespace TerrainGeneration
{
    public static class TerrainEntityFactory
    {
        public static EntityArchetype CreateVoxelArchetype (EntityManager entityManager)
        {
            return entityManager.CreateArchetype(
                ComponentType.ReadWrite<Voxel>(),               // type of entity identifier, plus world pos
                ComponentType.ReadWrite<Translation>(),         // voxel's position
                ComponentType.ReadWrite<SurfaceTopography>(),   // topography type for the voxel's x/z
                ComponentType.ReadWrite<VoxelNeighbours>(),      // get all the 6 main neighbours of the voxel, maybe need more later if we want smoother terrain
                ComponentType.ReadWrite<VoxelVisibleFaces>()
                
             




                );
        }
    }
}