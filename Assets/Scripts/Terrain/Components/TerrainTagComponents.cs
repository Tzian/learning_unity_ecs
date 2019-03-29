using Unity.Entities;

// sector tags for job filtering from generation to drawn

public struct GetSectorNoise : IComponentData { }
public struct GetUniqueSurfaceCells : IComponentData { }
public struct GetSectorTopography : IComponentData { }
public struct GenerateSectorGeology : IComponentData { }
public struct GetAdjacentSectors : IComponentData { }



public struct NeighboursAreReady : IComponentData { }
public struct DrawMeshTag : IComponentData { }
public struct MeshRedraw : IComponentData { }


public struct InDrawRangeSectorTag : IComponentData { }
public struct OutOfDrawRangeSectorTag : IComponentData { }

public struct SectorRemoveTag : IComponentData { }