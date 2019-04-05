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
            // ComponentType.ReadWrite<RenderMeshProxy> (),      // add this later as cannot move entities between worlds if it is on them
            ComponentType.ReadWrite<LocalToWorld>(),
            ComponentType.ReadWrite<WorleySurfaceNoise> (),    // surface noise buffer for the sectors XZ
            ComponentType.ReadWrite<SurfaceCell> (),
            ComponentType.ReadWrite<Topography> (),            // surface Height buffer
            ComponentType.ReadWrite<Block> (),

            // tags
            ComponentType.ReadOnly<GetSectorDrawRange>(),
            ComponentType.ReadOnly<GetSectorNoise> (),  // removed after sector has its noise and buffer of individual cells
            ComponentType.ReadWrite<GetSectorTopography>(),
            ComponentType.ReadOnly<GenerateSectorGeology> (),
            ComponentType.ReadOnly<GetAdjacentSectors> (),
            ComponentType.ReadOnly<DrawMeshTag> ()

            );
    }
}