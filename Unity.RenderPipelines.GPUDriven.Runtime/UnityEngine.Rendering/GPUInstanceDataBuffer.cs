using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.Rendering;

internal class GPUInstanceDataBuffer : IDisposable
{
	internal readonly struct ReadOnly
	{
		private readonly NativeArray<int> instancesNumPrefixSum;

		public ReadOnly(GPUInstanceDataBuffer buffer)
		{
			instancesNumPrefixSum = buffer.instancesNumPrefixSum;
		}

		public GPUInstanceIndex CPUInstanceToGPUInstance(InstanceHandle instance)
		{
			return GPUInstanceDataBuffer.CPUInstanceToGPUInstance(in instancesNumPrefixSum, instance);
		}

		public void CPUInstanceArrayToGPUInstanceArray(NativeArray<InstanceHandle> instances, NativeArray<GPUInstanceIndex> gpuInstanceIndices)
		{
			ConvertCPUInstancesToGPUInstancesJob jobData = default(ConvertCPUInstancesToGPUInstancesJob);
			jobData.instancesNumPrefixSum = instancesNumPrefixSum;
			jobData.instances = instances;
			jobData.gpuInstanceIndices = gpuInstanceIndices;
			IJobParallelForExtensions.Schedule(jobData, instances.Length, 512).Complete();
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	private struct ConvertCPUInstancesToGPUInstancesJob : IJobParallelFor
	{
		public const int k_BatchSize = 512;

		[ReadOnly]
		public NativeArray<int> instancesNumPrefixSum;

		[ReadOnly]
		public NativeArray<InstanceHandle> instances;

		[WriteOnly]
		public NativeArray<GPUInstanceIndex> gpuInstanceIndices;

		public void Execute(int index)
		{
			gpuInstanceIndices[index] = CPUInstanceToGPUInstance(in instancesNumPrefixSum, instances[index]);
		}
	}

	private static int s_NextLayoutVersion;

	public InstanceNumInfo instanceNumInfo;

	public NativeArray<int> instancesNumPrefixSum;

	public NativeArray<int> instancesSpan;

	public int byteSize;

	public int perInstanceComponentCount;

	public int version;

	public int layoutVersion;

	public GraphicsBuffer gpuBuffer;

	public GraphicsBuffer validComponentsIndicesGpuBuffer;

	public GraphicsBuffer componentAddressesGpuBuffer;

	public GraphicsBuffer componentInstanceIndexRangesGpuBuffer;

	public GraphicsBuffer componentByteCountsGpuBuffer;

	public NativeArray<GPUInstanceComponentDesc> descriptions;

	public NativeArray<MetadataValue> defaultMetadata;

	public NativeArray<int> gpuBufferComponentAddress;

	public NativeParallelHashMap<int, int> nameToMetadataMap;

	public bool valid => instancesSpan.IsCreated;

	public static int NextVersion()
	{
		return ++s_NextLayoutVersion;
	}

	private static GPUInstanceIndex CPUInstanceToGPUInstance(in NativeArray<int> instancesNumPrefixSum, InstanceHandle instance)
	{
		if (!instance.valid || instance.type >= InstanceType.Count)
		{
			return GPUInstanceIndex.Invalid;
		}
		int type = (int)instance.type;
		int instanceIndex = instance.instanceIndex;
		int index = instancesNumPrefixSum[type] + instanceIndex;
		GPUInstanceIndex result = default(GPUInstanceIndex);
		result.index = index;
		return result;
	}

	public int GetPropertyIndex(int propertyID, bool assertOnFail = true)
	{
		if (nameToMetadataMap.TryGetValue(propertyID, out var item))
		{
			return item;
		}
		return -1;
	}

	public int GetGpuAddress(string strName, bool assertOnFail = true)
	{
		int propertyIndex = GetPropertyIndex(Shader.PropertyToID(strName), assertOnFail: false);
		if (assertOnFail)
		{
			_ = -1;
		}
		if (propertyIndex == -1)
		{
			return -1;
		}
		return gpuBufferComponentAddress[propertyIndex];
	}

	public int GetGpuAddress(int propertyID, bool assertOnFail = true)
	{
		int propertyIndex = GetPropertyIndex(propertyID, assertOnFail);
		if (propertyIndex == -1)
		{
			return -1;
		}
		return gpuBufferComponentAddress[propertyIndex];
	}

	public GPUInstanceIndex CPUInstanceToGPUInstance(InstanceHandle instance)
	{
		return CPUInstanceToGPUInstance(in instancesNumPrefixSum, instance);
	}

	public InstanceHandle GPUInstanceToCPUInstance(GPUInstanceIndex gpuInstanceIndex)
	{
		int num = gpuInstanceIndex.index;
		InstanceType instanceType = InstanceType.Count;
		for (int i = 0; i < 2; i++)
		{
			int instanceNum = instanceNumInfo.GetInstanceNum((InstanceType)i);
			if (num < instanceNum)
			{
				instanceType = (InstanceType)i;
				break;
			}
			num -= instanceNum;
		}
		if (instanceType == InstanceType.Count)
		{
			return InstanceHandle.Invalid;
		}
		return InstanceHandle.Create(num, instanceType);
	}

	public void CPUInstanceArrayToGPUInstanceArray(NativeArray<InstanceHandle> instances, NativeArray<GPUInstanceIndex> gpuInstanceIndices)
	{
		ConvertCPUInstancesToGPUInstancesJob jobData = default(ConvertCPUInstancesToGPUInstancesJob);
		jobData.instancesNumPrefixSum = instancesNumPrefixSum;
		jobData.instances = instances;
		jobData.gpuInstanceIndices = gpuInstanceIndices;
		IJobParallelForExtensions.Schedule(jobData, instances.Length, 512).Complete();
	}

	public void Dispose()
	{
		if (instancesSpan.IsCreated)
		{
			instancesSpan.Dispose();
		}
		if (instancesNumPrefixSum.IsCreated)
		{
			instancesNumPrefixSum.Dispose();
		}
		if (descriptions.IsCreated)
		{
			descriptions.Dispose();
		}
		if (defaultMetadata.IsCreated)
		{
			defaultMetadata.Dispose();
		}
		if (gpuBufferComponentAddress.IsCreated)
		{
			gpuBufferComponentAddress.Dispose();
		}
		if (nameToMetadataMap.IsCreated)
		{
			nameToMetadataMap.Dispose();
		}
		if (gpuBuffer != null)
		{
			gpuBuffer.Release();
		}
		if (validComponentsIndicesGpuBuffer != null)
		{
			validComponentsIndicesGpuBuffer.Release();
		}
		if (componentAddressesGpuBuffer != null)
		{
			componentAddressesGpuBuffer.Release();
		}
		if (componentInstanceIndexRangesGpuBuffer != null)
		{
			componentInstanceIndexRangesGpuBuffer.Release();
		}
		if (componentByteCountsGpuBuffer != null)
		{
			componentByteCountsGpuBuffer.Release();
		}
	}

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(this);
	}
}
