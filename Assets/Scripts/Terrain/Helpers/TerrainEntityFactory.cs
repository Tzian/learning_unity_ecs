using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

public static class TerrainEntityFactory
{
    // sector archetype (think chunk in minecraft, info on blocks within the defined sectorSize)
    public static EntityArchetype CreateSectorArchetype (EntityManager entityManager)
    {
        return entityManager.CreateArchetype (
            ComponentType.ReadWrite<Sector> (),
            ComponentType.ReadWrite<Translation> (),
            ComponentType.ReadWrite<RenderMeshProxy> (),
            ComponentType.ReadWrite<WorleySurfaceNoise> (),    // surface noise buffer for the sectors XZ
            ComponentType.ReadWrite<SurfaceCell> (),
            ComponentType.ReadWrite<Topography> (),            // surface Height buffer
            ComponentType.ReadWrite<Block> (),
            ComponentType.ReadWrite<AdjacentSectors>(),

            // tags
            ComponentType.ReadOnly<GetSectorNoiseTag> (),  // removed after sector has its noise and buffer of individual cells
            ComponentType.ReadOnly<GetUniqueSurfaceCellsTag> (),  // remove after sector has its individual cells reduced down to unique only
            ComponentType.ReadOnly<GetSectorTopographyTag> (),
            ComponentType.ReadOnly<GetSectorDrawTypeTag>(),
            ComponentType.ReadOnly<GenerateSectorGeologyTag> (),
            ComponentType.ReadOnly<GetAdjacentSectorsTag> (),
            ComponentType.ReadOnly<DrawMeshTag> ()

            );
    }
}