using Unity.Entities;

// sector tags for job filtering from generation to drawn
public struct GetSectorNoiseTag : IComponentData { }
public struct GetUniqueSurfaceCellsTag : IComponentData { }
public struct GetSectorTopographyTag : IComponentData { }
public struct GetSectorDrawTypeTag : IComponentData { }
public struct GenerateSectorGeologyTag : IComponentData { }
public struct GetAdjacentSectorsTag : IComponentData { }
public struct DrawMeshTag : IComponentData { }


public struct InDrawRangeSectorTag : IComponentData { }
public struct OutOfDrawRangeSectorTag : IComponentData { }

public struct SectorRemoveTag : IComponentData { }