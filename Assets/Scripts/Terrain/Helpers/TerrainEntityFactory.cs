using Unity.Entities;
using Unity.Transforms;

namespace TerrainGeneration
{
    public static class TerrainEntityFactory
    {
        public static EntityArchetype CreateVoxelArchetype (EntityManager entityManager)
        {
            return entityManager.CreateArchetype(
                ComponentType.ReadWrite<Voxel>(),
                ComponentType.ReadWrite<Translation>()


                
             




                );
        }
    }
}