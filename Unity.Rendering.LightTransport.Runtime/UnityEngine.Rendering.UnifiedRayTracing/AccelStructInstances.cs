using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal sealed class AccelStructInstances : IDisposable
{
	public struct RTInstance
	{
		public float4x4 localToWorld;

		public float4x4 previousLocalToWorld;

		public float4x4 localToWorldNormals;

		public uint renderingLayerMask;

		public uint instanceMask;

		public uint userMaterialID;

		public uint geometryIndex;
	}

	public class InstanceEntry
	{
		public GeometryPoolHandle geometryPoolHandle;

		public BlockAllocator.Allocation indexInInstanceBuffer;

		public uint instanceMask;

		public uint vertexOffset;

		public uint indexOffset;
	}

	private readonly GeometryPool m_GeometryPool;

	private readonly PersistentGpuArray<RTInstance> m_InstanceBuffer = new PersistentGpuArray<RTInstance>(100);

	private readonly Dictionary<int, InstanceEntry> m_Instances = new Dictionary<int, InstanceEntry>();

	private uint m_FrameTimestamp;

	private uint m_TransformTouchedLastTimestamp;

	public PersistentGpuArray<RTInstance> instanceBuffer => m_InstanceBuffer;

	public IReadOnlyCollection<InstanceEntry> instances => m_Instances.Values;

	public GeometryPool geometryPool => m_GeometryPool;

	public GraphicsBuffer indexBuffer => m_GeometryPool.globalIndexBuffer;

	public GraphicsBuffer vertexBuffer => m_GeometryPool.globalVertexBuffer;

	public bool instanceListValid => m_InstanceBuffer != null;

	internal AccelStructInstances(GeometryPool geometryPool)
	{
		m_GeometryPool = geometryPool;
	}

	public void Dispose()
	{
		foreach (InstanceEntry value in m_Instances.Values)
		{
			GeometryPoolHandle geometryPoolHandle = value.geometryPoolHandle;
			m_GeometryPool.Unregister(geometryPoolHandle);
		}
		m_GeometryPool.SendGpuCommands();
		m_InstanceBuffer?.Dispose();
		m_GeometryPool.Dispose();
	}

	public int AddInstance(MeshInstanceDesc meshInstance, uint materialID, uint renderingLayerMask)
	{
		BlockAllocator.Allocation slotAllocation = m_InstanceBuffer.Add(1)[0];
		AddInstance(slotAllocation, in meshInstance, materialID, renderingLayerMask);
		return slotAllocation.block.offset;
	}

	public int AddInstances(Span<MeshInstanceDesc> meshInstances, Span<uint> materialIDs, Span<uint> renderingLayerMask)
	{
		BlockAllocator.Allocation[] array = m_InstanceBuffer.Add(meshInstances.Length);
		for (int i = 0; i < meshInstances.Length; i++)
		{
			AddInstance(array[i], in meshInstances[i], materialIDs[i], renderingLayerMask[i]);
		}
		return array[0].block.offset;
	}

	private void AddInstance(BlockAllocator.Allocation slotAllocation, in MeshInstanceDesc meshInstance, uint materialID, uint renderingLayerMask)
	{
		if (!m_GeometryPool.Register(meshInstance.mesh, out var outHandle))
		{
			throw new InvalidOperationException("Failed to allocate geometry data for instance");
		}
		m_GeometryPool.SendGpuCommands();
		m_InstanceBuffer.Set(slotAllocation, new RTInstance
		{
			localToWorld = meshInstance.localToWorldMatrix,
			localToWorldNormals = NormalMatrix(meshInstance.localToWorldMatrix),
			previousLocalToWorld = meshInstance.localToWorldMatrix,
			userMaterialID = materialID,
			instanceMask = meshInstance.mask,
			renderingLayerMask = renderingLayerMask,
			geometryIndex = (uint)(m_GeometryPool.GetEntryGeomAllocation(outHandle).meshChunkTableAlloc.block.offset + meshInstance.subMeshIndex)
		});
		GeometryPool.MeshChunk meshChunk = m_GeometryPool.GetEntryGeomAllocation(outHandle).meshChunks[meshInstance.subMeshIndex];
		InstanceEntry value = new InstanceEntry
		{
			geometryPoolHandle = outHandle,
			indexInInstanceBuffer = slotAllocation,
			instanceMask = meshInstance.mask,
			vertexOffset = (uint)meshChunk.vertexAlloc.block.offset * ((uint)GeometryPool.GetVertexByteSize() / 4u),
			indexOffset = (uint)meshChunk.indexAlloc.block.offset
		};
		m_Instances.Add(slotAllocation.block.offset, value);
	}

	public GeometryPool.MeshChunk GetEntryGeomAllocation(GeometryPoolHandle handle, int submeshIndex)
	{
		return m_GeometryPool.GetEntryGeomAllocation(handle).meshChunks[submeshIndex];
	}

	public void RemoveInstance(int instanceHandle)
	{
		m_Instances.TryGetValue(instanceHandle, out var value);
		m_Instances.Remove(instanceHandle);
		m_InstanceBuffer.Remove(value.indexInInstanceBuffer);
		GeometryPoolHandle geometryPoolHandle = value.geometryPoolHandle;
		m_GeometryPool.Unregister(geometryPoolHandle);
		m_GeometryPool.SendGpuCommands();
	}

	public void ClearInstances()
	{
		foreach (InstanceEntry value in m_Instances.Values)
		{
			GeometryPoolHandle geometryPoolHandle = value.geometryPoolHandle;
			m_GeometryPool.Unregister(geometryPoolHandle);
		}
		m_GeometryPool.SendGpuCommands();
		m_Instances.Clear();
		m_InstanceBuffer.Clear();
	}

	public void UpdateInstanceTransform(int instanceHandle, Matrix4x4 localToWorldMatrix)
	{
		m_Instances.TryGetValue(instanceHandle, out var value);
		RTInstance element = m_InstanceBuffer.Get(value.indexInInstanceBuffer);
		element.localToWorld = localToWorldMatrix;
		element.localToWorldNormals = NormalMatrix(localToWorldMatrix);
		m_InstanceBuffer.Set(value.indexInInstanceBuffer, element);
		m_TransformTouchedLastTimestamp = m_FrameTimestamp;
	}

	public void UpdateInstanceMaterialID(int instanceHandle, uint materialID)
	{
		m_Instances.TryGetValue(instanceHandle, out var value);
		RTInstance element = m_InstanceBuffer.Get(value.indexInInstanceBuffer);
		element.userMaterialID = materialID;
		m_InstanceBuffer.Set(value.indexInInstanceBuffer, element);
	}

	public void UpdateRenderingLayerMask(int instanceHandle, uint renderingLayerMask)
	{
		m_Instances.TryGetValue(instanceHandle, out var value);
		RTInstance element = m_InstanceBuffer.Get(value.indexInInstanceBuffer);
		element.renderingLayerMask = renderingLayerMask;
		m_InstanceBuffer.Set(value.indexInInstanceBuffer, element);
	}

	public void UpdateInstanceMask(int instanceHandle, uint mask)
	{
		m_Instances.TryGetValue(instanceHandle, out var value);
		value.instanceMask = mask;
		RTInstance element = m_InstanceBuffer.Get(value.indexInInstanceBuffer);
		element.instanceMask = mask;
		m_InstanceBuffer.Set(value.indexInInstanceBuffer, element);
	}

	public void NextFrame()
	{
		if (m_FrameTimestamp - m_TransformTouchedLastTimestamp <= 1)
		{
			m_InstanceBuffer.ModifyForEach(delegate(RTInstance instance)
			{
				instance.previousLocalToWorld = instance.localToWorld;
				return instance;
			});
		}
		m_FrameTimestamp++;
	}

	public void Bind(CommandBuffer cmd, IRayTracingShader shader)
	{
		ComputeBuffer gpuBuffer = m_InstanceBuffer.GetGpuBuffer(cmd);
		shader.SetBufferParam(cmd, Shader.PropertyToID("g_AccelStructInstanceList"), gpuBuffer);
		shader.SetBufferParam(cmd, Shader.PropertyToID("g_globalIndexBuffer"), m_GeometryPool.globalIndexBuffer);
		shader.SetBufferParam(cmd, Shader.PropertyToID("g_globalVertexBuffer"), m_GeometryPool.globalVertexBuffer);
		shader.SetIntParam(cmd, Shader.PropertyToID("g_globalVertexBufferStride"), m_GeometryPool.globalVertexBufferStrideBytes / 4);
		shader.SetBufferParam(cmd, Shader.PropertyToID("g_MeshList"), m_GeometryPool.globalMeshChunkTableEntryBuffer);
	}

	public int GetInstanceCount()
	{
		return m_Instances.Count;
	}

	private static float4x4 NormalMatrix(float4x4 m)
	{
		return new float4x4(math.inverse(math.transpose(new float3x3(m))), new float3(0.0));
	}
}
