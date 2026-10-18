using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal sealed class GeometryPool : IDisposable
{
	private static class GeoPoolShaderIDs
	{
		public static readonly int _InputIBBaseOffset = Shader.PropertyToID("_InputIBBaseOffset");

		public static readonly int _DispatchIndexOffset = Shader.PropertyToID("_DispatchIndexOffset");

		public static readonly int _InputIBCount = Shader.PropertyToID("_InputIBCount");

		public static readonly int _OutputIBOffset = Shader.PropertyToID("_OutputIBOffset");

		public static readonly int _InputFirstVertex = Shader.PropertyToID("_InputFirstVertex");

		public static readonly int _InputIndexBuffer = Shader.PropertyToID("_InputIndexBuffer");

		public static readonly int _OutputIndexBuffer = Shader.PropertyToID("_OutputIndexBuffer");

		public static readonly int _InputVBCount = Shader.PropertyToID("_InputVBCount");

		public static readonly int _InputBaseVertexOffset = Shader.PropertyToID("_InputBaseVertexOffset");

		public static readonly int _DispatchVertexOffset = Shader.PropertyToID("_DispatchVertexOffset");

		public static readonly int _OutputVBSize = Shader.PropertyToID("_OutputVBSize");

		public static readonly int _OutputVBOffset = Shader.PropertyToID("_OutputVBOffset");

		public static readonly int _InputPosBufferStride = Shader.PropertyToID("_InputPosBufferStride");

		public static readonly int _InputPosBufferOffset = Shader.PropertyToID("_InputPosBufferOffset");

		public static readonly int _InputUv0BufferStride = Shader.PropertyToID("_InputUv0BufferStride");

		public static readonly int _InputUv0BufferOffset = Shader.PropertyToID("_InputUv0BufferOffset");

		public static readonly int _InputUv1BufferStride = Shader.PropertyToID("_InputUv1BufferStride");

		public static readonly int _InputUv1BufferOffset = Shader.PropertyToID("_InputUv1BufferOffset");

		public static readonly int _InputNormalBufferStride = Shader.PropertyToID("_InputNormalBufferStride");

		public static readonly int _InputNormalBufferOffset = Shader.PropertyToID("_InputNormalBufferOffset");

		public static readonly int _PosBuffer = Shader.PropertyToID("_PosBuffer");

		public static readonly int _Uv0Buffer = Shader.PropertyToID("_Uv0Buffer");

		public static readonly int _Uv1Buffer = Shader.PropertyToID("_Uv1Buffer");

		public static readonly int _NormalBuffer = Shader.PropertyToID("_NormalBuffer");

		public static readonly int _OutputVB = Shader.PropertyToID("_OutputVB");

		public static readonly int _AttributesMask = Shader.PropertyToID("_AttributesMask");
	}

	public struct MeshChunk
	{
		public BlockAllocator.Allocation vertexAlloc;

		public BlockAllocator.Allocation indexAlloc;

		public static MeshChunk Invalid
		{
			get
			{
				MeshChunk result = default(MeshChunk);
				result.vertexAlloc = BlockAllocator.Allocation.Invalid;
				result.indexAlloc = BlockAllocator.Allocation.Invalid;
				return result;
			}
		}

		public GeoPoolMeshChunk EncodeGPUEntry()
		{
			GeoPoolMeshChunk result = default(GeoPoolMeshChunk);
			result.indexOffset = indexAlloc.block.offset;
			result.indexCount = indexAlloc.block.count;
			result.vertexOffset = vertexAlloc.block.offset;
			result.vertexCount = vertexAlloc.block.count;
			return result;
		}
	}

	public struct GeometrySlot
	{
		public uint refCount;

		public uint hash;

		public BlockAllocator.Allocation meshChunkTableAlloc;

		public NativeArray<MeshChunk> meshChunks;

		public bool hasGPUData;

		public static readonly GeometrySlot Invalid = new GeometrySlot
		{
			meshChunkTableAlloc = BlockAllocator.Allocation.Invalid,
			hasGPUData = false
		};

		public bool valid => meshChunkTableAlloc.valid;
	}

	private struct GeoPoolEntrySlot
	{
		public uint refCount;

		public uint hash;

		public int geoSlotHandle;

		public static readonly GeoPoolEntrySlot Invalid = new GeoPoolEntrySlot
		{
			refCount = 0u,
			hash = 0u,
			geoSlotHandle = -1
		};

		public bool valid => geoSlotHandle != -1;
	}

	private struct VertexBufferAttribInfo
	{
		public GraphicsBuffer buffer;

		public int stride;

		public int offset;

		public int byteCount;

		public bool valid => buffer != null;
	}

	private const int kMaxThreadGroupsPerDispatch = 65535;

	private const int kThreadGroupSize = 256;

	private const int InvalidHandle = -1;

	private const GraphicsBuffer.Target VertexBufferTarget = GraphicsBuffer.Target.Structured;

	private const GraphicsBuffer.Target IndexBufferTarget = GraphicsBuffer.Target.Structured;

	private GraphicsBuffer m_GlobalIndexBuffer;

	private GraphicsBuffer m_GlobalVertexBuffer;

	private GraphicsBuffer m_GlobalMeshChunkTableEntryBuffer;

	private readonly GraphicsBuffer m_DummyBuffer;

	private int m_MaxVertCounts;

	private int m_MaxIndexCounts;

	private int m_MaxMeshChunkTableEntriesCount;

	private BlockAllocator m_VertexAllocator;

	private BlockAllocator m_IndexAllocator;

	private BlockAllocator m_MeshChunkTableAllocator;

	private NativeParallelHashMap<uint, int> m_MeshHashToGeoSlot;

	private List<GeometrySlot> m_GeoSlots;

	private NativeList<int> m_FreeGeoSlots;

	private NativeParallelHashMap<uint, GeometryPoolHandle> m_GeoPoolEntryHashToSlot;

	private NativeList<GeoPoolEntrySlot> m_GeoPoolEntrySlots;

	private NativeList<GeometryPoolHandle> m_FreeGeoPoolEntrySlots;

	private readonly List<GraphicsBuffer> m_InputBufferReferences;

	private readonly ComputeShader m_CopyShader;

	private ComputeShader m_GeometryPoolKernelsCS;

	private int m_KernelMainUpdateIndexBuffer16;

	private int m_KernelMainUpdateIndexBuffer32;

	private int m_KernelMainUpdateVertexBuffer;

	private readonly CommandBuffer m_CmdBuffer;

	private bool m_MustClearCmdBuffer;

	private int m_PendingCmds;

	public GraphicsBuffer globalIndexBuffer => m_GlobalIndexBuffer;

	public GraphicsBuffer globalVertexBuffer => m_GlobalVertexBuffer;

	public int globalVertexBufferStrideBytes => GetVertexByteSize();

	public GraphicsBuffer globalMeshChunkTableEntryBuffer => m_GlobalMeshChunkTableEntryBuffer;

	public int indicesCount => m_MaxIndexCounts;

	public int verticesCount => m_MaxVertCounts;

	public int meshChunkTablesEntryCount => m_MaxMeshChunkTableEntriesCount;

	public static int GetVertexByteSize()
	{
		return 32;
	}

	public static int GetIndexByteSize()
	{
		return 4;
	}

	public static int GetMeshChunkTableEntryByteSize()
	{
		return Marshal.SizeOf<GeoPoolMeshChunk>();
	}

	private int GetFormatByteCount(VertexAttributeFormat format)
	{
		return format switch
		{
			VertexAttributeFormat.Float32 => 4, 
			VertexAttributeFormat.Float16 => 2, 
			VertexAttributeFormat.UNorm8 => 1, 
			VertexAttributeFormat.SNorm8 => 1, 
			VertexAttributeFormat.UNorm16 => 2, 
			VertexAttributeFormat.SNorm16 => 2, 
			VertexAttributeFormat.UInt8 => 1, 
			VertexAttributeFormat.SInt8 => 1, 
			VertexAttributeFormat.UInt16 => 2, 
			VertexAttributeFormat.SInt16 => 2, 
			VertexAttributeFormat.UInt32 => 4, 
			VertexAttributeFormat.SInt32 => 4, 
			_ => 4, 
		};
	}

	private static int DivUp(int x, int y)
	{
		return (x + y - 1) / y;
	}

	public GeometryPool(in GeometryPoolDesc desc, ComputeShader geometryPoolShader, ComputeShader copyShader)
	{
		m_CopyShader = copyShader;
		LoadKernels(geometryPoolShader);
		m_CmdBuffer = new CommandBuffer();
		m_InputBufferReferences = new List<GraphicsBuffer>();
		m_MustClearCmdBuffer = false;
		m_PendingCmds = 0;
		m_MaxVertCounts = CalcVertexCount(desc.vertexPoolByteSize);
		m_MaxIndexCounts = CalcIndexCount(desc.indexPoolByteSize);
		m_MaxMeshChunkTableEntriesCount = CalcMeshChunkTablesCount(desc.meshChunkTablesByteSize);
		m_GlobalVertexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, DivUp(m_MaxVertCounts * GetVertexByteSize(), 4), 4);
		m_GlobalIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, m_MaxIndexCounts, 4);
		m_GlobalMeshChunkTableEntryBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, m_MaxMeshChunkTableEntriesCount, GetMeshChunkTableEntryByteSize());
		m_DummyBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 16, 4);
		int capacity = 4096;
		m_MeshHashToGeoSlot = new NativeParallelHashMap<uint, int>(capacity, Allocator.Persistent);
		m_GeoSlots = new List<GeometrySlot>();
		m_FreeGeoSlots = new NativeList<int>(Allocator.Persistent);
		m_GeoPoolEntryHashToSlot = new NativeParallelHashMap<uint, GeometryPoolHandle>(capacity, Allocator.Persistent);
		m_GeoPoolEntrySlots = new NativeList<GeoPoolEntrySlot>(Allocator.Persistent);
		m_FreeGeoPoolEntrySlots = new NativeList<GeometryPoolHandle>(Allocator.Persistent);
		m_VertexAllocator = default(BlockAllocator);
		m_VertexAllocator.Initialize(m_MaxVertCounts);
		m_IndexAllocator = default(BlockAllocator);
		m_IndexAllocator.Initialize(m_MaxIndexCounts);
		m_MeshChunkTableAllocator = default(BlockAllocator);
		m_MeshChunkTableAllocator.Initialize(m_MaxMeshChunkTableEntriesCount);
	}

	private void DisposeInputBuffers()
	{
		if (m_InputBufferReferences.Count == 0)
		{
			return;
		}
		foreach (GraphicsBuffer inputBufferReference in m_InputBufferReferences)
		{
			inputBufferReference.Dispose();
		}
		m_InputBufferReferences.Clear();
	}

	public void Dispose()
	{
		m_IndexAllocator.Dispose();
		m_VertexAllocator.Dispose();
		m_MeshChunkTableAllocator.Dispose();
		m_DummyBuffer.Dispose();
		m_MeshHashToGeoSlot.Dispose();
		foreach (GeometrySlot geoSlot in m_GeoSlots)
		{
			if (geoSlot.valid)
			{
				NativeArray<MeshChunk> meshChunks = geoSlot.meshChunks;
				meshChunks.Dispose();
			}
		}
		m_GeoSlots = null;
		m_FreeGeoSlots.Dispose();
		m_GeoPoolEntryHashToSlot.Dispose();
		m_GeoPoolEntrySlots.Dispose();
		m_FreeGeoPoolEntrySlots.Dispose();
		m_GlobalIndexBuffer.Dispose();
		m_GlobalVertexBuffer.Release();
		m_GlobalMeshChunkTableEntryBuffer.Dispose();
		m_CmdBuffer.Release();
		DisposeInputBuffers();
	}

	private void LoadKernels(ComputeShader geometryPoolShader)
	{
		m_GeometryPoolKernelsCS = geometryPoolShader;
		m_KernelMainUpdateIndexBuffer16 = m_GeometryPoolKernelsCS.FindKernel("MainUpdateIndexBuffer16");
		m_KernelMainUpdateIndexBuffer32 = m_GeometryPoolKernelsCS.FindKernel("MainUpdateIndexBuffer32");
		m_KernelMainUpdateVertexBuffer = m_GeometryPoolKernelsCS.FindKernel("MainUpdateVertexBuffer");
	}

	private int CalcVertexCount(int bufferByteSize)
	{
		return DivUp(bufferByteSize, GetVertexByteSize());
	}

	private int CalcIndexCount(int bufferByteSize)
	{
		return DivUp(bufferByteSize, GetIndexByteSize());
	}

	private int CalcMeshChunkTablesCount(int bufferByteSize)
	{
		return DivUp(bufferByteSize, GetMeshChunkTableEntryByteSize());
	}

	private void DeallocateGeometrySlot(ref GeometrySlot slot)
	{
		if (slot.meshChunkTableAlloc.valid)
		{
			m_MeshChunkTableAllocator.FreeAllocation(in slot.meshChunkTableAlloc);
			if (slot.meshChunks.IsCreated)
			{
				for (int i = 0; i < slot.meshChunks.Length; i++)
				{
					MeshChunk meshChunk = slot.meshChunks[i];
					if (meshChunk.vertexAlloc.valid)
					{
						m_VertexAllocator.FreeAllocation(in meshChunk.vertexAlloc);
					}
					if (meshChunk.indexAlloc.valid)
					{
						m_IndexAllocator.FreeAllocation(in meshChunk.indexAlloc);
					}
				}
				slot.meshChunks.Dispose();
			}
		}
		slot = GeometrySlot.Invalid;
	}

	private void DeallocateGeometrySlot(int geoSlotHandle)
	{
		GeometrySlot slot = m_GeoSlots[geoSlotHandle];
		slot.refCount--;
		if (slot.refCount == 0)
		{
			m_MeshHashToGeoSlot.Remove(slot.hash);
			DeallocateGeometrySlot(ref slot);
			m_FreeGeoSlots.Add(in geoSlotHandle);
		}
		m_GeoSlots[geoSlotHandle] = slot;
	}

	private bool AllocateGeo(Mesh mesh, out int allocationHandle)
	{
		uint hashCode = (uint)mesh.GetHashCode();
		int num = 0;
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			num += (int)mesh.GetIndexCount(i);
		}
		if (m_MeshHashToGeoSlot.TryGetValue(hashCode, out allocationHandle))
		{
			GeometrySlot value = m_GeoSlots[allocationHandle];
			value.refCount++;
			m_GeoSlots[allocationHandle] = value;
			return true;
		}
		allocationHandle = -1;
		GeometrySlot slot = GeometrySlot.Invalid;
		slot.refCount = 1u;
		slot.hash = hashCode;
		bool flag = true;
		if (mesh.subMeshCount > 0)
		{
			slot.meshChunkTableAlloc = m_MeshChunkTableAllocator.Allocate(mesh.subMeshCount);
			if (!slot.meshChunkTableAlloc.valid)
			{
				slot.meshChunkTableAlloc = m_MeshChunkTableAllocator.GrowAndAllocate(mesh.subMeshCount, int.MaxValue / GetMeshChunkTableEntryByteSize(), out var oldCapacity, out var newCapacity);
				if (!slot.meshChunkTableAlloc.valid)
				{
					throw new UnifiedRayTracingException("Can't allocate a GraphicsBuffer bigger than 2GB", UnifiedRayTracingError.OutOfGraphicsBufferMemory);
				}
				GraphicsHelpers.ReallocateBuffer(m_CopyShader, oldCapacity, newCapacity, GetMeshChunkTableEntryByteSize(), ref m_GlobalMeshChunkTableEntryBuffer);
				m_MaxMeshChunkTableEntriesCount = newCapacity;
			}
			slot.meshChunks = new NativeArray<MeshChunk>(mesh.subMeshCount, Allocator.Persistent);
			for (int j = 0; j < mesh.subMeshCount; j++)
			{
				SubMeshDescriptor subMesh = mesh.GetSubMesh(j);
				MeshChunk invalid = MeshChunk.Invalid;
				invalid.vertexAlloc = m_VertexAllocator.Allocate(subMesh.vertexCount);
				if (!invalid.vertexAlloc.valid)
				{
					invalid.vertexAlloc = m_VertexAllocator.GrowAndAllocate(subMesh.vertexCount, int.MaxValue / GetVertexByteSize(), out var oldCapacity2, out var newCapacity2);
					if (!invalid.vertexAlloc.valid)
					{
						throw new UnifiedRayTracingException("Can't allocate a GraphicsBuffer bigger than 2GB", UnifiedRayTracingError.OutOfGraphicsBufferMemory);
					}
					GraphicsHelpers.ReallocateBuffer(m_CopyShader, oldCapacity2, newCapacity2, GetVertexByteSize(), ref m_GlobalVertexBuffer);
					m_MaxVertCounts = newCapacity2;
				}
				invalid.indexAlloc = m_IndexAllocator.Allocate(subMesh.indexCount);
				if (!invalid.indexAlloc.valid)
				{
					invalid.indexAlloc = m_IndexAllocator.GrowAndAllocate(subMesh.indexCount, 536870911, out var oldCapacity3, out var newCapacity3);
					if (!invalid.indexAlloc.valid)
					{
						throw new UnifiedRayTracingException("Can't allocate a GraphicsBuffer bigger than 2GB", UnifiedRayTracingError.OutOfGraphicsBufferMemory);
					}
					GraphicsHelpers.ReallocateBuffer(m_CopyShader, oldCapacity3, newCapacity3, 4, ref m_GlobalIndexBuffer);
					m_MaxIndexCounts = newCapacity3;
				}
				slot.meshChunks[j] = invalid;
			}
		}
		if (!flag)
		{
			DeallocateGeometrySlot(ref slot);
			return false;
		}
		if (m_FreeGeoSlots.IsEmpty)
		{
			allocationHandle = m_GeoSlots.Count;
			m_GeoSlots.Add(slot);
		}
		else
		{
			allocationHandle = m_FreeGeoSlots[m_FreeGeoSlots.Length - 1];
			m_FreeGeoSlots.RemoveAtSwapBack(m_FreeGeoSlots.Length - 1);
			m_GeoSlots[allocationHandle] = slot;
		}
		m_MeshHashToGeoSlot.Add(slot.hash, allocationHandle);
		return true;
	}

	private void DeallocateGeoPoolEntrySlot(GeometryPoolHandle handle)
	{
		GeoPoolEntrySlot geoPoolEntrySlot = m_GeoPoolEntrySlots[handle.index];
		geoPoolEntrySlot.refCount--;
		if (geoPoolEntrySlot.refCount == 0)
		{
			m_GeoPoolEntryHashToSlot.Remove(geoPoolEntrySlot.hash);
			DeallocateGeoPoolEntrySlot(ref geoPoolEntrySlot);
			m_FreeGeoPoolEntrySlots.Add(in handle);
		}
		m_GeoPoolEntrySlots[handle.index] = geoPoolEntrySlot;
	}

	private void DeallocateGeoPoolEntrySlot(ref GeoPoolEntrySlot geoPoolEntrySlot)
	{
		if (geoPoolEntrySlot.geoSlotHandle != -1)
		{
			DeallocateGeometrySlot(geoPoolEntrySlot.geoSlotHandle);
		}
		geoPoolEntrySlot = GeoPoolEntrySlot.Invalid;
	}

	public GeometryPoolEntryInfo GetEntryInfo(GeometryPoolHandle handle)
	{
		if (!handle.valid)
		{
			return GeometryPoolEntryInfo.NewDefault();
		}
		GeoPoolEntrySlot geoPoolEntrySlot = m_GeoPoolEntrySlots[handle.index];
		if (!geoPoolEntrySlot.valid)
		{
			return GeometryPoolEntryInfo.NewDefault();
		}
		if (geoPoolEntrySlot.geoSlotHandle == -1)
		{
			Debug.LogErrorFormat("Found invalid geometry slot handle with handle id {0}.", handle.index);
		}
		GeometryPoolEntryInfo result = default(GeometryPoolEntryInfo);
		result.valid = geoPoolEntrySlot.valid;
		result.refCount = geoPoolEntrySlot.refCount;
		return result;
	}

	public GeometrySlot GetEntryGeomAllocation(GeometryPoolHandle handle)
	{
		GeoPoolEntrySlot geoPoolEntrySlot = m_GeoPoolEntrySlots[handle.index];
		return m_GeoSlots[geoPoolEntrySlot.geoSlotHandle];
	}

	public int GetInstanceGeometryIndex(Mesh mesh)
	{
		return GetEntryGeomAllocation(GetHandle(mesh)).meshChunkTableAlloc.block.offset;
	}

	private void UpdateGeoGpuState(Mesh mesh, GeometryPoolHandle handle)
	{
		GeoPoolEntrySlot geoPoolEntrySlot = m_GeoPoolEntrySlots[handle.index];
		GeometrySlot value = m_GeoSlots[geoPoolEntrySlot.geoSlotHandle];
		CommandBuffer commandBuffer = AllocateCommandBuffer();
		if (!value.hasGPUData)
		{
			GraphicsBuffer inputBuffer = LoadIndexBuffer(mesh);
			LoadVertexAttribInfo(mesh, VertexAttribute.Position, out var output);
			LoadVertexAttribInfo(mesh, VertexAttribute.TexCoord0, out var output2);
			LoadVertexAttribInfo(mesh, VertexAttribute.TexCoord1, out var output3);
			LoadVertexAttribInfo(mesh, VertexAttribute.Normal, out var output4);
			NativeArray<GeoPoolMeshChunk> data = new NativeArray<GeoPoolMeshChunk>(value.meshChunks.Length, Allocator.Temp);
			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				SubMeshDescriptor subMesh = mesh.GetSubMesh(i);
				MeshChunk meshChunk = value.meshChunks[i];
				AddVertexUpdateCommand(commandBuffer, subMesh.baseVertex + subMesh.firstVertex, in output, in output2, in output3, in output4, in meshChunk.vertexAlloc, m_GlobalVertexBuffer);
				AddIndexUpdateCommand(commandBuffer, mesh.indexFormat, in inputBuffer, in meshChunk.indexAlloc, subMesh.firstVertex, subMesh.indexStart, subMesh.indexCount, 0, m_GlobalIndexBuffer);
				data[i] = meshChunk.EncodeGPUEntry();
			}
			commandBuffer.SetBufferData(m_GlobalMeshChunkTableEntryBuffer, data, 0, value.meshChunkTableAlloc.block.offset, data.Length);
			data.Dispose();
			value.hasGPUData = true;
			m_GeoSlots[geoPoolEntrySlot.geoSlotHandle] = value;
		}
	}

	private uint FNVHash(uint prevHash, uint dword)
	{
		for (int i = 0; i < 4; i++)
		{
			prevHash ^= (dword >> i * 8) & 0xFFu;
			prevHash *= 2166136261u;
		}
		return prevHash;
	}

	private uint CalculateClusterHash(Mesh mesh, GeometryPoolSubmeshData[] submeshData)
	{
		uint num = (uint)mesh.GetHashCode();
		if (submeshData != null)
		{
			for (int i = 0; i < submeshData.Length; i++)
			{
				GeometryPoolSubmeshData geometryPoolSubmeshData = submeshData[i];
				num = FNVHash(num, (uint)geometryPoolSubmeshData.submeshIndex);
				num = FNVHash(num, (!(geometryPoolSubmeshData.material == null)) ? ((uint)geometryPoolSubmeshData.material.GetHashCode()) : 0u);
			}
		}
		return num;
	}

	public GeometryPoolHandle GetHandle(Mesh mesh)
	{
		uint key = CalculateClusterHash(mesh, null);
		if (m_GeoPoolEntryHashToSlot.TryGetValue(key, out var item))
		{
			return item;
		}
		return GeometryPoolHandle.Invalid;
	}

	private static int FindSubmeshEntryInDesc(int submeshIndex, in GeometryPoolSubmeshData[] submeshData)
	{
		if (submeshData == null)
		{
			return -1;
		}
		for (int i = 0; i < submeshData.Length; i++)
		{
			if (submeshData[i].submeshIndex == submeshIndex)
			{
				return i;
			}
		}
		return -1;
	}

	public bool Register(Mesh mesh, out GeometryPoolHandle outHandle)
	{
		GeometryPoolEntryDesc entryDesc = new GeometryPoolEntryDesc
		{
			mesh = mesh,
			submeshData = null
		};
		return Register(in entryDesc, out outHandle);
	}

	public bool Register(in GeometryPoolEntryDesc entryDesc, out GeometryPoolHandle outHandle)
	{
		outHandle = GeometryPoolHandle.Invalid;
		if (entryDesc.mesh == null)
		{
			return false;
		}
		Mesh mesh = entryDesc.mesh;
		uint num = CalculateClusterHash(entryDesc.mesh, entryDesc.submeshData);
		if (m_GeoPoolEntryHashToSlot.TryGetValue(num, out outHandle))
		{
			GeoPoolEntrySlot value = m_GeoPoolEntrySlots[outHandle.index];
			_ = m_GeoSlots[value.geoSlotHandle];
			value.refCount++;
			m_GeoPoolEntrySlots[outHandle.index] = value;
			return true;
		}
		GeoPoolEntrySlot value2 = GeoPoolEntrySlot.Invalid;
		value2.refCount = 1u;
		value2.hash = num;
		List<GeometryPoolSubmeshData> list = new List<GeometryPoolSubmeshData>(mesh.subMeshCount);
		if (mesh.subMeshCount > 0 && entryDesc.submeshData != null)
		{
			for (int i = 0; i < mesh.subMeshCount; i++)
			{
				int num2 = FindSubmeshEntryInDesc(i, in entryDesc.submeshData);
				if (num2 == -1)
				{
					Debug.LogErrorFormat("Could not find submesh index {0} for mesh entry descriptor of mesh {1}.", i, mesh.name);
				}
				else
				{
					list.Add(entryDesc.submeshData[num2]);
				}
			}
		}
		if (!AllocateGeo(mesh, out value2.geoSlotHandle))
		{
			DeallocateGeoPoolEntrySlot(ref value2);
			return false;
		}
		if (m_FreeGeoPoolEntrySlots.IsEmpty)
		{
			outHandle = new GeometryPoolHandle
			{
				index = m_GeoPoolEntrySlots.Length
			};
			m_GeoPoolEntrySlots.Add(in value2);
		}
		else
		{
			outHandle = m_FreeGeoPoolEntrySlots[m_FreeGeoPoolEntrySlots.Length - 1];
			m_FreeGeoPoolEntrySlots.RemoveAtSwapBack(m_FreeGeoPoolEntrySlots.Length - 1);
			m_GeoPoolEntrySlots[outHandle.index] = value2;
		}
		m_GeoPoolEntryHashToSlot.Add(value2.hash, outHandle);
		UpdateGeoGpuState(mesh, outHandle);
		return true;
	}

	public void Unregister(GeometryPoolHandle handle)
	{
		_ = m_GeoPoolEntrySlots[handle.index];
		DeallocateGeoPoolEntrySlot(handle);
	}

	public void SendGpuCommands()
	{
		if (m_PendingCmds != 0)
		{
			Graphics.ExecuteCommandBuffer(m_CmdBuffer);
			m_MustClearCmdBuffer = true;
			m_PendingCmds = 0;
		}
		DisposeInputBuffers();
	}

	private GraphicsBuffer LoadIndexBuffer(Mesh mesh)
	{
		mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
		mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;
		GraphicsBuffer indexBuffer = mesh.GetIndexBuffer();
		m_InputBufferReferences.Add(indexBuffer);
		return indexBuffer;
	}

	private void LoadVertexAttribInfo(Mesh mesh, VertexAttribute attribute, out VertexBufferAttribInfo output)
	{
		if (!mesh.HasVertexAttribute(attribute))
		{
			output.buffer = null;
			output.stride = (output.offset = (output.byteCount = 0));
			return;
		}
		int vertexAttributeStream = mesh.GetVertexAttributeStream(attribute);
		output.stride = mesh.GetVertexBufferStride(vertexAttributeStream);
		output.offset = mesh.GetVertexAttributeOffset(attribute);
		output.byteCount = GetFormatByteCount(mesh.GetVertexAttributeFormat(attribute)) * mesh.GetVertexAttributeDimension(attribute);
		output.buffer = mesh.GetVertexBuffer(vertexAttributeStream);
		m_InputBufferReferences.Add(output.buffer);
	}

	private CommandBuffer AllocateCommandBuffer()
	{
		if (m_MustClearCmdBuffer)
		{
			m_CmdBuffer.Clear();
			m_MustClearCmdBuffer = false;
		}
		m_PendingCmds++;
		return m_CmdBuffer;
	}

	private void AddIndexUpdateCommand(CommandBuffer cmdBuffer, IndexFormat inputFormat, in GraphicsBuffer inputBuffer, in BlockAllocator.Allocation location, int firstVertex, int inputOffset, int indexCount, int outputOffset, GraphicsBuffer outputIdxBuffer)
	{
		if (location.block.count != 0)
		{
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputIBBaseOffset, inputOffset);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputIBCount, indexCount);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputFirstVertex, firstVertex);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._OutputIBOffset, location.block.offset + outputOffset);
			int kernelIndex = ((inputFormat == IndexFormat.UInt16) ? m_KernelMainUpdateIndexBuffer16 : m_KernelMainUpdateIndexBuffer32);
			cmdBuffer.SetComputeBufferParam(m_GeometryPoolKernelsCS, kernelIndex, GeoPoolShaderIDs._InputIndexBuffer, inputBuffer);
			cmdBuffer.SetComputeBufferParam(m_GeometryPoolKernelsCS, kernelIndex, GeoPoolShaderIDs._OutputIndexBuffer, outputIdxBuffer);
			int num = DivUp(location.block.count, 256);
			int num2 = DivUp(num, 65535);
			for (int i = 0; i < num2; i++)
			{
				int val = i * 65535 * 256;
				int threadGroupsX = Math.Min(65535, num - i * 65535);
				cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._DispatchIndexOffset, val);
				cmdBuffer.DispatchCompute(m_GeometryPoolKernelsCS, kernelIndex, threadGroupsX, 1, 1);
			}
		}
	}

	private void AddVertexUpdateCommand(CommandBuffer cmdBuffer, int baseVertexOffset, in VertexBufferAttribInfo pos, in VertexBufferAttribInfo uv0, in VertexBufferAttribInfo uv1, in VertexBufferAttribInfo n, in BlockAllocator.Allocation location, GraphicsBuffer outputVertexBuffer)
	{
		if (location.block.count != 0)
		{
			GeoPoolVertexAttribs geoPoolVertexAttribs = (GeoPoolVertexAttribs)0;
			if (pos.valid)
			{
				geoPoolVertexAttribs |= GeoPoolVertexAttribs.Position;
			}
			if (uv0.valid)
			{
				geoPoolVertexAttribs |= GeoPoolVertexAttribs.Uv0;
			}
			if (uv1.valid)
			{
				geoPoolVertexAttribs |= GeoPoolVertexAttribs.Uv1;
			}
			if (n.valid)
			{
				geoPoolVertexAttribs |= GeoPoolVertexAttribs.Normal;
			}
			int count = location.block.count;
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputVBCount, count);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputBaseVertexOffset, baseVertexOffset);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._OutputVBSize, m_MaxVertCounts);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._OutputVBOffset, location.block.offset);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputPosBufferStride, pos.stride);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputPosBufferOffset, pos.offset);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputUv0BufferStride, uv0.stride);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputUv0BufferOffset, uv0.offset);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputUv1BufferStride, uv1.stride);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputUv1BufferOffset, uv1.offset);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputNormalBufferStride, n.stride);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._InputNormalBufferOffset, n.offset);
			cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._AttributesMask, (int)geoPoolVertexAttribs);
			int kernelMainUpdateVertexBuffer = m_KernelMainUpdateVertexBuffer;
			cmdBuffer.SetComputeBufferParam(m_GeometryPoolKernelsCS, kernelMainUpdateVertexBuffer, GeoPoolShaderIDs._PosBuffer, pos.valid ? pos.buffer : m_DummyBuffer);
			cmdBuffer.SetComputeBufferParam(m_GeometryPoolKernelsCS, kernelMainUpdateVertexBuffer, GeoPoolShaderIDs._Uv0Buffer, uv0.valid ? uv0.buffer : m_DummyBuffer);
			cmdBuffer.SetComputeBufferParam(m_GeometryPoolKernelsCS, kernelMainUpdateVertexBuffer, GeoPoolShaderIDs._Uv1Buffer, uv1.valid ? uv1.buffer : m_DummyBuffer);
			cmdBuffer.SetComputeBufferParam(m_GeometryPoolKernelsCS, kernelMainUpdateVertexBuffer, GeoPoolShaderIDs._NormalBuffer, n.valid ? n.buffer : m_DummyBuffer);
			cmdBuffer.SetComputeBufferParam(m_GeometryPoolKernelsCS, kernelMainUpdateVertexBuffer, GeoPoolShaderIDs._OutputVB, outputVertexBuffer);
			int num = DivUp(count, 256);
			int num2 = DivUp(num, 65535);
			for (int i = 0; i < num2; i++)
			{
				int val = i * 65535 * 256;
				int threadGroupsX = Math.Min(65535, num - i * 65535);
				cmdBuffer.SetComputeIntParam(m_GeometryPoolKernelsCS, GeoPoolShaderIDs._DispatchVertexOffset, val);
				cmdBuffer.DispatchCompute(m_GeometryPoolKernelsCS, kernelMainUpdateVertexBuffer, threadGroupsX, 1, 1);
			}
		}
	}
}
