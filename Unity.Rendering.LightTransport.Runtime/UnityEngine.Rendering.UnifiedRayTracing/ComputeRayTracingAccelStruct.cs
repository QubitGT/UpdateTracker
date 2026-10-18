using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Rendering.RadeonRays;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal class ComputeRayTracingAccelStruct : IRayTracingAccelStruct, IDisposable
{
	private struct Triangle
	{
		public float3 v0;

		public float3 v1;

		public float3 v2;
	}

	private sealed class RadeonRaysInstance
	{
		public (int mesh, int subMeshIndex) geomKey;

		public MeshBlas blas;

		public uint instanceMask;

		public bool triangleCullingEnabled;

		public bool invertTriangleCulling;

		public uint userInstanceID;

		public UnityEngine.Rendering.RadeonRays.Transform localToWorldTransform;
	}

	private sealed class MeshBlas
	{
		public MeshBuildInfo buildInfo;

		public BlockAllocator.Allocation bvhAlloc;

		public BlockAllocator.Allocation bvhLeavesAlloc;

		public BlockAllocator.Allocation blasVertices;

		public bool bvhBuilt;

		private uint refCount;

		public void IncRef()
		{
			refCount++;
		}

		public void DecRef()
		{
			refCount--;
		}

		public bool IsUnreferenced()
		{
			return refCount == 0;
		}
	}

	private readonly uint m_HandleObfuscation = (uint)Random.Range(int.MinValue, int.MaxValue);

	private readonly RadeonRaysAPI m_RadeonRaysAPI;

	private readonly BuildFlags m_BuildFlags;

	private readonly ReferenceCounter m_Counter;

	private readonly Dictionary<(int mesh, int subMeshIndex), MeshBlas> m_Blases;

	internal BlockAllocator m_BlasAllocator;

	private GraphicsBuffer m_BlasBuffer;

	internal BlockAllocator m_BlasLeavesAllocator;

	private GraphicsBuffer m_BlasLeavesBuffer;

	private readonly BLASPositionsPool m_BlasPositions;

	private TopLevelAccelStruct? m_TopLevelAccelStruct;

	private readonly ComputeShader m_CopyShader;

	private readonly Dictionary<int, RadeonRaysInstance> m_RadeonInstances = new Dictionary<int, RadeonRaysInstance>();

	private readonly Queue<uint> m_FreeHandles = new Queue<uint>();

	internal GraphicsBuffer topLevelBvhBuffer => m_TopLevelAccelStruct?.topLevelBvh;

	internal GraphicsBuffer bottomLevelBvhBuffer => m_TopLevelAccelStruct?.bottomLevelBvhs;

	internal GraphicsBuffer instanceInfoBuffer => m_TopLevelAccelStruct?.instanceInfos;

	internal ComputeRayTracingAccelStruct(AccelerationStructureOptions options, RayTracingResources resources, ReferenceCounter counter, int blasBufferInitialSizeBytes = 67108864)
	{
		m_CopyShader = resources.copyBuffer;
		m_RadeonRaysAPI = new RadeonRaysAPI(new RadeonRaysShaders
		{
			bitHistogram = resources.bitHistogram,
			blockReducePart = resources.blockReducePart,
			blockScan = resources.blockScan,
			buildHlbvh = resources.buildHlbvh,
			restructureBvh = resources.restructureBvh,
			scatter = resources.scatter
		});
		m_BuildFlags = options.buildFlags;
		m_Blases = new Dictionary<(int, int), MeshBlas>();
		int num = blasBufferInitialSizeBytes / RadeonRaysAPI.BvhInternalNodeSizeInBytes();
		m_BlasBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, num, RadeonRaysAPI.BvhInternalNodeSizeInBytes());
		m_BlasLeavesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, num, RadeonRaysAPI.BvhLeafNodeSizeInBytes());
		m_BlasPositions = new BLASPositionsPool(resources.copyPositions, resources.copyBuffer);
		m_BlasAllocator = default(BlockAllocator);
		m_BlasAllocator.Initialize(num);
		m_BlasLeavesAllocator = default(BlockAllocator);
		m_BlasLeavesAllocator.Initialize(num);
		m_Counter = counter;
		m_Counter.Inc();
	}

	public void Dispose()
	{
		foreach (MeshBlas value in m_Blases.Values)
		{
			if (value.buildInfo.triangleIndices != null)
			{
				value.buildInfo.triangleIndices.Dispose();
			}
		}
		m_Counter.Dec();
		m_RadeonRaysAPI.Dispose();
		m_BlasBuffer.Dispose();
		m_BlasLeavesBuffer.Dispose();
		m_BlasPositions.Dispose();
		m_BlasAllocator.Dispose();
		m_BlasLeavesAllocator.Dispose();
		m_TopLevelAccelStruct?.Dispose();
	}

	public int AddInstance(MeshInstanceDesc meshInstance)
	{
		MeshBlas orAllocateMeshBlas = GetOrAllocateMeshBlas(meshInstance.mesh, meshInstance.subMeshIndex);
		orAllocateMeshBlas.IncRef();
		FreeTopLevelAccelStruct();
		int num = NewHandle();
		m_RadeonInstances.Add(num, new RadeonRaysInstance
		{
			geomKey = (mesh: meshInstance.mesh.GetHashCode(), subMeshIndex: meshInstance.subMeshIndex),
			blas = orAllocateMeshBlas,
			instanceMask = meshInstance.mask,
			triangleCullingEnabled = meshInstance.enableTriangleCulling,
			invertTriangleCulling = meshInstance.frontTriangleCounterClockwise,
			userInstanceID = ((meshInstance.instanceID == uint.MaxValue) ? ((uint)num) : meshInstance.instanceID),
			localToWorldTransform = ConvertTranform(meshInstance.localToWorldMatrix)
		});
		return num;
	}

	public void RemoveInstance(int instanceHandle)
	{
		ReleaseHandle(instanceHandle);
		m_RadeonInstances.Remove(instanceHandle, out var value);
		MeshBlas blas = value.blas;
		blas.DecRef();
		if (blas.IsUnreferenced())
		{
			DeleteMeshBlas(value.geomKey, blas);
		}
		FreeTopLevelAccelStruct();
	}

	public void ClearInstances()
	{
		m_FreeHandles.Clear();
		m_RadeonInstances.Clear();
		foreach (MeshBlas value in m_Blases.Values)
		{
			if (value.buildInfo.triangleIndices != null)
			{
				value.buildInfo.triangleIndices.Dispose();
			}
		}
		m_Blases.Clear();
		m_BlasPositions.Clear();
		int capacity = m_BlasAllocator.capacity;
		m_BlasAllocator.Dispose();
		m_BlasAllocator = default(BlockAllocator);
		m_BlasAllocator.Initialize(capacity);
		capacity = m_BlasLeavesAllocator.capacity;
		m_BlasLeavesAllocator.Dispose();
		m_BlasLeavesAllocator = default(BlockAllocator);
		m_BlasLeavesAllocator.Initialize(capacity);
		FreeTopLevelAccelStruct();
	}

	public void UpdateInstanceTransform(int instanceHandle, Matrix4x4 localToWorldMatrix)
	{
		m_RadeonInstances[instanceHandle].localToWorldTransform = ConvertTranform(localToWorldMatrix);
		FreeTopLevelAccelStruct();
	}

	public void UpdateInstanceID(int instanceHandle, uint instanceID)
	{
		m_RadeonInstances[instanceHandle].userInstanceID = instanceID;
		FreeTopLevelAccelStruct();
	}

	public void UpdateInstanceMask(int instanceHandle, uint mask)
	{
		m_RadeonInstances[instanceHandle].instanceMask = mask;
		FreeTopLevelAccelStruct();
	}

	public void Build(CommandBuffer cmd, GraphicsBuffer scratchBuffer)
	{
		ulong buildScratchBufferRequiredSizeInBytes = GetBuildScratchBufferRequiredSizeInBytes();
		if (buildScratchBufferRequiredSizeInBytes != 0 && (scratchBuffer == null || (ulong)(scratchBuffer.count * scratchBuffer.stride) < buildScratchBufferRequiredSizeInBytes))
		{
			throw new ArgumentException("scratchBuffer size is too small");
		}
		if (buildScratchBufferRequiredSizeInBytes != 0 && scratchBuffer.stride != 4)
		{
			throw new ArgumentException("scratchBuffer stride must be 4");
		}
		if (!m_TopLevelAccelStruct.HasValue)
		{
			CreateBvh(cmd, scratchBuffer);
		}
	}

	public ulong GetBuildScratchBufferRequiredSizeInBytes()
	{
		return GetBvhBuildScratchBufferSizeInDwords() * 4;
	}

	private void FreeTopLevelAccelStruct()
	{
		m_TopLevelAccelStruct?.Dispose();
		m_TopLevelAccelStruct = null;
	}

	private MeshBlas GetOrAllocateMeshBlas(Mesh mesh, int subMeshIndex)
	{
		if (m_Blases.TryGetValue((mesh.GetHashCode(), subMeshIndex), out var value))
		{
			return value;
		}
		value = new MeshBlas();
		AllocateBlas(mesh, subMeshIndex, value);
		m_Blases[(mesh.GetHashCode(), subMeshIndex)] = value;
		return value;
	}

	private void AllocateBlas(Mesh mesh, int submeshIndex, MeshBlas blas)
	{
		int num = RadeonRaysAPI.BvhInternalNodeSizeInDwords();
		mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
		mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
		SubMeshDescriptor subMesh = mesh.GetSubMesh(submeshIndex);
		int stride;
		int offset;
		using GraphicsBuffer vertices = LoadPositionBuffer(mesh, out stride, out offset);
		GraphicsBuffer graphicsBuffer = null;
		graphicsBuffer = LoadIndexBuffer(mesh);
		VertexBufferChunk info = default(VertexBufferChunk);
		info.vertices = vertices;
		info.verticesStartOffset = offset;
		info.baseVertex = subMesh.baseVertex + subMesh.firstVertex;
		info.vertexCount = (uint)subMesh.vertexCount;
		info.vertexStride = (uint)stride;
		if (!m_BlasPositions.Add(info, out blas.blasVertices))
		{
			throw new UnifiedRayTracingException("Can't allocate a GraphicsBuffer bigger than 2GB", UnifiedRayTracingError.OutOfGraphicsBufferMemory);
		}
		MeshBuildInfo buildInfo = default(MeshBuildInfo);
		buildInfo.vertices = m_BlasPositions.VertexBuffer;
		buildInfo.verticesStartOffset = blas.blasVertices.block.offset * 3;
		buildInfo.baseVertex = 0;
		buildInfo.triangleIndices = graphicsBuffer;
		buildInfo.vertexCount = (uint)blas.blasVertices.block.count;
		buildInfo.triangleCount = (uint)subMesh.indexCount / 3u;
		buildInfo.indicesStartOffset = subMesh.indexStart;
		buildInfo.baseIndex = -subMesh.firstVertex;
		buildInfo.indexFormat = ((mesh.indexFormat != IndexFormat.UInt32) ? UnityEngine.Rendering.RadeonRays.IndexFormat.Int16 : UnityEngine.Rendering.RadeonRays.IndexFormat.Int32);
		buildInfo.vertexStride = 3u;
		blas.buildInfo = buildInfo;
		ulong num2 = m_RadeonRaysAPI.GetMeshBuildMemoryRequirements(buildInfo, ConvertFlagsToGpuBuild(m_BuildFlags)).bvhSizeInDwords / (ulong)num;
		if (num2 > int.MaxValue)
		{
			throw new UnifiedRayTracingException("Can't allocate a GraphicsBuffer bigger than 2GB", UnifiedRayTracingError.OutOfGraphicsBufferMemory);
		}
		blas.bvhAlloc = AllocateBlasInternalNodes((int)num2);
		if (!blas.bvhAlloc.valid)
		{
			m_BlasPositions.Remove(ref blas.blasVertices);
			throw new UnifiedRayTracingException("Can't allocate a GraphicsBuffer bigger than 2GB", UnifiedRayTracingError.OutOfGraphicsBufferMemory);
		}
		blas.bvhLeavesAlloc = AllocateBlasLeafNodes((int)buildInfo.triangleCount);
		if (!blas.bvhLeavesAlloc.valid)
		{
			m_BlasPositions.Remove(ref blas.blasVertices);
			m_BlasAllocator.FreeAllocation(in blas.bvhAlloc);
			throw new UnifiedRayTracingException("Can't allocate a GraphicsBuffer bigger than 2GB", UnifiedRayTracingError.OutOfGraphicsBufferMemory);
		}
	}

	private GraphicsBuffer LoadIndexBuffer(Mesh mesh)
	{
		return mesh.GetIndexBuffer();
	}

	private GraphicsBuffer LoadPositionBuffer(Mesh mesh, out int stride, out int offset)
	{
		VertexAttribute attr = VertexAttribute.Position;
		int vertexAttributeStream = mesh.GetVertexAttributeStream(attr);
		stride = mesh.GetVertexBufferStride(vertexAttributeStream) / 4;
		offset = mesh.GetVertexAttributeOffset(attr) / 4;
		return mesh.GetVertexBuffer(vertexAttributeStream);
	}

	private void DeleteMeshBlas((int mesh, int subMeshIndex) geomKey, MeshBlas blas)
	{
		m_BlasAllocator.FreeAllocation(in blas.bvhAlloc);
		blas.bvhAlloc = BlockAllocator.Allocation.Invalid;
		m_BlasLeavesAllocator.FreeAllocation(in blas.bvhLeavesAlloc);
		blas.bvhLeavesAlloc = BlockAllocator.Allocation.Invalid;
		m_BlasPositions.Remove(ref blas.blasVertices);
		if (blas.buildInfo.triangleIndices != null)
		{
			blas.buildInfo.triangleIndices.Dispose();
		}
		m_Blases.Remove(geomKey);
	}

	private ulong GetBvhBuildScratchBufferSizeInDwords()
	{
		RadeonRaysAPI.BvhInternalNodeSizeInDwords();
		ulong x = 0uL;
		foreach (KeyValuePair<(int, int), MeshBlas> blase in m_Blases)
		{
			if (!blase.Value.bvhBuilt)
			{
				x = math.max(x, m_RadeonRaysAPI.GetMeshBuildMemoryRequirements(blase.Value.buildInfo, ConvertFlagsToGpuBuild(m_BuildFlags)).buildScratchSizeInDwords);
			}
		}
		ulong buildScratchSizeInDwords = m_RadeonRaysAPI.GetSceneBuildMemoryRequirements((uint)m_RadeonInstances.Count).buildScratchSizeInDwords;
		x = math.max(x, buildScratchSizeInDwords);
		return math.max(4uL, x);
	}

	private void CreateBvh(CommandBuffer cmd, GraphicsBuffer scratchBuffer)
	{
		BuildMissingBottomLevelAccelStructs(cmd, scratchBuffer);
		BuildTopLevelAccelStruct(cmd, scratchBuffer);
	}

	private void BuildMissingBottomLevelAccelStructs(CommandBuffer cmd, GraphicsBuffer scratchBuffer)
	{
		foreach (MeshBlas value in m_Blases.Values)
		{
			if (!value.bvhBuilt)
			{
				value.buildInfo.vertices = m_BlasPositions.VertexBuffer;
				BottomLevelLevelAccelStruct bottomLevelLevelAccelStruct = default(BottomLevelLevelAccelStruct);
				bottomLevelLevelAccelStruct.bvh = m_BlasBuffer;
				bottomLevelLevelAccelStruct.bvhOffset = (uint)value.bvhAlloc.block.offset;
				bottomLevelLevelAccelStruct.bvhLeaves = m_BlasLeavesBuffer;
				bottomLevelLevelAccelStruct.bvhLeavesOffset = (uint)value.bvhLeavesAlloc.block.offset;
				BottomLevelLevelAccelStruct result = bottomLevelLevelAccelStruct;
				m_RadeonRaysAPI.BuildMeshAccelStruct(cmd, value.buildInfo, ConvertFlagsToGpuBuild(m_BuildFlags), scratchBuffer, in result);
				value.buildInfo.triangleIndices.Dispose();
				value.buildInfo.triangleIndices = null;
				value.bvhBuilt = true;
			}
		}
	}

	private void BuildTopLevelAccelStruct(CommandBuffer cmd, GraphicsBuffer scratchBuffer)
	{
		Instance[] array = new Instance[m_RadeonInstances.Count];
		int num = 0;
		foreach (RadeonRaysInstance value in m_RadeonInstances.Values)
		{
			array[num].meshAccelStructOffset = (uint)value.blas.bvhAlloc.block.offset;
			array[num].localToWorldTransform = value.localToWorldTransform;
			array[num].instanceMask = value.instanceMask;
			array[num].vertexOffset = (uint)(value.blas.blasVertices.block.offset * 3);
			array[num].meshAccelStructLeavesOffset = (uint)value.blas.bvhLeavesAlloc.block.offset;
			array[num].triangleCullingEnabled = value.triangleCullingEnabled;
			array[num].invertTriangleCulling = value.invertTriangleCulling;
			array[num].userInstanceID = value.userInstanceID;
			num++;
		}
		m_TopLevelAccelStruct?.Dispose();
		m_TopLevelAccelStruct = m_RadeonRaysAPI.BuildSceneAccelStruct(cmd, m_BlasBuffer, array, scratchBuffer);
	}

	private UnityEngine.Rendering.RadeonRays.BuildFlags ConvertFlagsToGpuBuild(BuildFlags flags)
	{
		if ((flags & BuildFlags.PreferFastBuild) != 0 && (flags & BuildFlags.PreferFastTrace) == 0)
		{
			return UnityEngine.Rendering.RadeonRays.BuildFlags.PreferFastBuild;
		}
		return UnityEngine.Rendering.RadeonRays.BuildFlags.None;
	}

	public void Bind(CommandBuffer cmd, string name, IRayTracingShader shader)
	{
		shader.SetBufferParam(cmd, Shader.PropertyToID(name + "bvh"), topLevelBvhBuffer);
		shader.SetBufferParam(cmd, Shader.PropertyToID(name + "bottomBvhs"), bottomLevelBvhBuffer);
		shader.SetBufferParam(cmd, Shader.PropertyToID(name + "bottomBvhLeaves"), m_BlasLeavesBuffer);
		shader.SetBufferParam(cmd, Shader.PropertyToID(name + "instanceInfos"), instanceInfoBuffer);
		shader.SetBufferParam(cmd, Shader.PropertyToID(name + "vertexBuffer"), m_BlasPositions.VertexBuffer);
		shader.SetIntParam(cmd, Shader.PropertyToID(name + "vertexStride"), 3);
	}

	private static UnityEngine.Rendering.RadeonRays.Transform ConvertTranform(Matrix4x4 input)
	{
		UnityEngine.Rendering.RadeonRays.Transform result = default(UnityEngine.Rendering.RadeonRays.Transform);
		result.row0 = input.GetRow(0);
		result.row1 = input.GetRow(1);
		result.row2 = input.GetRow(2);
		return result;
	}

	private static Matrix4x4 ConvertTranform(UnityEngine.Rendering.RadeonRays.Transform input)
	{
		Matrix4x4 result = default(Matrix4x4);
		result.SetRow(0, input.row0);
		result.SetRow(1, input.row1);
		result.SetRow(2, input.row2);
		result.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
		return result;
	}

	private static int3 GetFaceIndices(List<int> indices, int triangleIdx)
	{
		return new int3(indices[3 * triangleIdx], indices[3 * triangleIdx + 1], indices[3 * triangleIdx + 2]);
	}

	private static Triangle GetTriangle(List<Vector3> vertices, int3 idx)
	{
		Triangle result = default(Triangle);
		result.v0 = vertices[idx.x];
		result.v1 = vertices[idx.y];
		result.v2 = vertices[idx.z];
		return result;
	}

	private BlockAllocator.Allocation AllocateBlasInternalNodes(int allocationNodeCount)
	{
		BlockAllocator.Allocation result = m_BlasAllocator.Allocate(allocationNodeCount);
		if (!result.valid)
		{
			result = m_BlasAllocator.GrowAndAllocate(allocationNodeCount, int.MaxValue / RadeonRaysAPI.BvhInternalNodeSizeInBytes(), out var oldCapacity, out var newCapacity);
			if (!result.valid)
			{
				return result;
			}
			GraphicsHelpers.ReallocateBuffer(m_CopyShader, oldCapacity, newCapacity, RadeonRaysAPI.BvhInternalNodeSizeInBytes(), ref m_BlasBuffer);
		}
		return result;
	}

	private BlockAllocator.Allocation AllocateBlasLeafNodes(int allocationNodeCount)
	{
		BlockAllocator.Allocation result = m_BlasLeavesAllocator.Allocate(allocationNodeCount);
		if (!result.valid)
		{
			result = m_BlasLeavesAllocator.GrowAndAllocate(allocationNodeCount, int.MaxValue / RadeonRaysAPI.BvhLeafNodeSizeInBytes(), out var oldCapacity, out var newCapacity);
			if (!result.valid)
			{
				return result;
			}
			GraphicsHelpers.ReallocateBuffer(m_CopyShader, oldCapacity, newCapacity, RadeonRaysAPI.BvhLeafNodeSizeInBytes(), ref m_BlasLeavesBuffer);
		}
		return result;
	}

	private int NewHandle()
	{
		if (m_FreeHandles.Count != 0)
		{
			return (int)(m_FreeHandles.Dequeue() ^ m_HandleObfuscation);
		}
		return m_RadeonInstances.Count ^ (int)m_HandleObfuscation;
	}

	private void ReleaseHandle(int handle)
	{
		m_FreeHandles.Enqueue((uint)handle ^ m_HandleObfuscation);
	}
}
