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


public struct InnerDrawRangeSectorTag : IComponentData { }
public struct OuterDrawRangeSectorTag : IComponentData { }
public struct EdgeOfDrawRangeSectorTag : IComponentData { }
public struct NotInDrawRangeSectorTag : IComponentData { }


public struct ReadyForWorldMove : IComponentData { }
public struct SectorRemoveTag : IComponentData { }