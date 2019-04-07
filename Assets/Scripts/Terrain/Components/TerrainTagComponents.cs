using Unity.Entities;


public struct GetVoxelDrawRange : IComponentData { }
public struct GetSurfaceTopography : IComponentData { }


public struct GetVoxelNeighbours : IComponentData { }


public struct VoxelIsNotInDrawRange : IComponentData { }
public struct VoxelIsInDrawRange : IComponentData { }
public struct VoxelIsInDrawBufferZone : IComponentData { }