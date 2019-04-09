﻿using Unity.Entities;


public struct VoxelIsNotInDrawRange : IComponentData { }
public struct VoxelIsInDrawRange : IComponentData { }
public struct VoxelIsInDrawBufferZone : IComponentData { }


public struct GetVoxelDrawRange : IComponentData { }
public struct GetSurfaceTopography : IComponentData { }
public struct GetSurfaceHeight : IComponentData { }
public struct GetVoxelGeology : IComponentData { }
public struct GetAdjacentVoxels : IComponentData { }
public struct GetVisibleFaces : IComponentData { }
public struct GetMeshData : IComponentData { }
public struct ReadyToMesh : IComponentData { }

