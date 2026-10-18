using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering;

internal struct GPUInstanceDataBufferBuilder : IDisposable
{
	private NativeList<GPUInstanceComponentDesc> m_Components;

	private MetadataValue CreateMetadataValue(int nameID, int gpuAddress, bool isOverridden)
	{
		MetadataValue result = default(MetadataValue);
		result.NameID = nameID;
		result.Value = (uint)gpuAddress | (isOverridden ? 2147483648u : 0u);
		return result;
	}

	public void AddComponent<T>(int propertyID, bool isOverriden, bool isPerInstance, InstanceType instanceType, InstanceComponentGroup componentGroup = InstanceComponentGroup.Default) where T : unmanaged
	{
		AddComponent(propertyID, isOverriden, UnsafeUtility.SizeOf<T>(), isPerInstance, instanceType, componentGroup);
	}

	public void AddComponent(int propertyID, bool isOverriden, int byteSize, bool isPerInstance, InstanceType instanceType, InstanceComponentGroup componentGroup)
	{
		if (!m_Components.IsCreated)
		{
			m_Components = new NativeList<GPUInstanceComponentDesc>(64, Allocator.Temp);
		}
		_ = m_Components.Length;
		_ = 0;
		ref NativeList<GPUInstanceComponentDesc> components = ref m_Components;
		GPUInstanceComponentDesc value = new GPUInstanceComponentDesc(propertyID, byteSize, isOverriden, isPerInstance, instanceType, componentGroup);
		components.Add(in value);
	}

	public unsafe GPUInstanceDataBuffer Build(in InstanceNumInfo instanceNumInfo)
	{
		int num = 0;
		NativeArray<int> data = new NativeArray<int>(m_Components.Length, Allocator.Temp);
		NativeArray<int> data2 = new NativeArray<int>(m_Components.Length, Allocator.Temp);
		NativeArray<int> data3 = new NativeArray<int>(m_Components.Length, Allocator.Temp);
		NativeArray<Vector2Int> data4 = new NativeArray<Vector2Int>(m_Components.Length, Allocator.Temp);
		GPUInstanceDataBuffer gPUInstanceDataBuffer = new GPUInstanceDataBuffer();
		gPUInstanceDataBuffer.instanceNumInfo = instanceNumInfo;
		gPUInstanceDataBuffer.instancesNumPrefixSum = new NativeArray<int>(2, Allocator.Persistent);
		gPUInstanceDataBuffer.instancesSpan = new NativeArray<int>(2, Allocator.Persistent);
		int num2 = 0;
		for (int i = 0; i < 2; i++)
		{
			gPUInstanceDataBuffer.instancesNumPrefixSum[i] = num2;
			num2 += instanceNumInfo.InstanceNums[i];
			gPUInstanceDataBuffer.instancesSpan[i] = instanceNumInfo.GetInstanceNumIncludingChildren((InstanceType)i);
		}
		gPUInstanceDataBuffer.layoutVersion = GPUInstanceDataBuffer.NextVersion();
		gPUInstanceDataBuffer.version = 0;
		gPUInstanceDataBuffer.defaultMetadata = new NativeArray<MetadataValue>(m_Components.Length, Allocator.Persistent);
		gPUInstanceDataBuffer.descriptions = new NativeArray<GPUInstanceComponentDesc>(m_Components.Length, Allocator.Persistent);
		gPUInstanceDataBuffer.nameToMetadataMap = new NativeParallelHashMap<int, int>(m_Components.Length, Allocator.Persistent);
		gPUInstanceDataBuffer.gpuBufferComponentAddress = new NativeArray<int>(m_Components.Length, Allocator.Persistent);
		int num3 = UnsafeUtility.SizeOf<Vector4>();
		int num4 = 4 * num3;
		for (int j = 0; j < m_Components.Length; j++)
		{
			GPUInstanceComponentDesc value = m_Components[j];
			gPUInstanceDataBuffer.descriptions[j] = value;
			int num5 = gPUInstanceDataBuffer.instancesNumPrefixSum[(int)value.instanceType];
			int num6 = num5 + gPUInstanceDataBuffer.instancesSpan[(int)value.instanceType];
			int num7 = ((!value.isPerInstance) ? 1 : (num6 - num5));
			data4[j] = new Vector2Int(num5, num5 + num7);
			int num8 = num4 - num5 * value.byteSize;
			gPUInstanceDataBuffer.gpuBufferComponentAddress[j] = num8;
			gPUInstanceDataBuffer.defaultMetadata[j] = CreateMetadataValue(value.propertyID, num8, value.isOverriden);
			data2[j] = num8;
			data3[j] = value.byteSize;
			int num9 = value.byteSize * num7;
			num4 += num9;
			gPUInstanceDataBuffer.nameToMetadataMap.TryAdd(value.propertyID, j);
			if (value.isPerInstance)
			{
				data[num] = j;
				num++;
			}
		}
		gPUInstanceDataBuffer.byteSize = num4;
		gPUInstanceDataBuffer.gpuBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, gPUInstanceDataBuffer.byteSize / 4, 4);
		gPUInstanceDataBuffer.gpuBuffer.SetData(new NativeArray<Vector4>(4, Allocator.Temp), 0, 0, 4);
		gPUInstanceDataBuffer.validComponentsIndicesGpuBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, num, 4);
		gPUInstanceDataBuffer.validComponentsIndicesGpuBuffer.SetData(data, 0, 0, num);
		gPUInstanceDataBuffer.componentAddressesGpuBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, m_Components.Length, 4);
		gPUInstanceDataBuffer.componentAddressesGpuBuffer.SetData(data2, 0, 0, m_Components.Length);
		gPUInstanceDataBuffer.componentInstanceIndexRangesGpuBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, m_Components.Length, 8);
		gPUInstanceDataBuffer.componentInstanceIndexRangesGpuBuffer.SetData(data4, 0, 0, m_Components.Length);
		gPUInstanceDataBuffer.componentByteCountsGpuBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, m_Components.Length, 4);
		gPUInstanceDataBuffer.componentByteCountsGpuBuffer.SetData(data3, 0, 0, m_Components.Length);
		gPUInstanceDataBuffer.perInstanceComponentCount = num;
		data.Dispose();
		data2.Dispose();
		data3.Dispose();
		return gPUInstanceDataBuffer;
	}

	public void Dispose()
	{
		if (m_Components.IsCreated)
		{
			m_Components.Dispose();
		}
	}
}
