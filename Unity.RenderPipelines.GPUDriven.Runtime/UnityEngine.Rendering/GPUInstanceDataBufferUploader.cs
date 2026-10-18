using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

internal struct GPUInstanceDataBufferUploader : IDisposable
{
	private static class UploadKernelIDs
	{
		public static readonly int _InputValidComponentCounts = Shader.PropertyToID("_InputValidComponentCounts");

		public static readonly int _InputInstanceCounts = Shader.PropertyToID("_InputInstanceCounts");

		public static readonly int _InputInstanceByteSize = Shader.PropertyToID("_InputInstanceByteSize");

		public static readonly int _InputComponentOffsets = Shader.PropertyToID("_InputComponentOffsets");

		public static readonly int _InputInstanceData = Shader.PropertyToID("_InputInstanceData");

		public static readonly int _InputInstanceIndices = Shader.PropertyToID("_InputInstanceIndices");

		public static readonly int _InputValidComponentIndices = Shader.PropertyToID("_InputValidComponentIndices");

		public static readonly int _InputComponentAddresses = Shader.PropertyToID("_InputComponentAddresses");

		public static readonly int _InputComponentByteCounts = Shader.PropertyToID("_InputComponentByteCounts");

		public static readonly int _InputComponentInstanceIndexRanges = Shader.PropertyToID("_InputComponentInstanceIndexRanges");

		public static readonly int _OutputBuffer = Shader.PropertyToID("_OutputBuffer");
	}

	public struct GPUResources : IDisposable
	{
		public ComputeBuffer instanceData;

		public ComputeBuffer instanceIndices;

		public ComputeBuffer inputComponentOffsets;

		public ComputeBuffer validComponentIndices;

		public ComputeShader cs;

		public int kernelId;

		private int m_InstanceDataByteSize;

		private int m_InstanceCount;

		private int m_ComponentCounts;

		private int m_ValidComponentIndicesCount;

		public void LoadShaders(GPUResidentDrawerResources resources)
		{
			if (cs == null)
			{
				cs = resources.instanceDataBufferUploadKernels;
				kernelId = cs.FindKernel("MainUploadScatterInstances");
			}
		}

		public void CreateResources(int newInstanceCount, int sizePerInstance, int newComponentCounts, int validComponentIndicesCount)
		{
			int num = newInstanceCount * sizePerInstance;
			if (num > m_InstanceDataByteSize || instanceData == null)
			{
				if (instanceData != null)
				{
					instanceData.Release();
				}
				instanceData = new ComputeBuffer((num + 3) / 4, 4, ComputeBufferType.Raw);
				m_InstanceDataByteSize = num;
			}
			if (newInstanceCount > m_InstanceCount || instanceIndices == null)
			{
				if (instanceIndices != null)
				{
					instanceIndices.Release();
				}
				instanceIndices = new ComputeBuffer(newInstanceCount, 4, ComputeBufferType.Raw);
				m_InstanceCount = newInstanceCount;
			}
			if (newComponentCounts > m_ComponentCounts || inputComponentOffsets == null)
			{
				if (inputComponentOffsets != null)
				{
					inputComponentOffsets.Release();
				}
				inputComponentOffsets = new ComputeBuffer(newComponentCounts, 4, ComputeBufferType.Raw);
				m_ComponentCounts = newComponentCounts;
			}
			if (validComponentIndicesCount > m_ValidComponentIndicesCount || validComponentIndices == null)
			{
				if (validComponentIndices != null)
				{
					validComponentIndices.Release();
				}
				validComponentIndices = new ComputeBuffer(validComponentIndicesCount, 4, ComputeBufferType.Raw);
				m_ValidComponentIndicesCount = validComponentIndicesCount;
			}
		}

		public void Dispose()
		{
			cs = null;
			if (instanceData != null)
			{
				instanceData.Release();
			}
			if (instanceIndices != null)
			{
				instanceIndices.Release();
			}
			if (inputComponentOffsets != null)
			{
				inputComponentOffsets.Release();
			}
			if (validComponentIndices != null)
			{
				validComponentIndices.Release();
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal struct WriteInstanceDataParameterJob : IJobParallelFor
	{
		public const int k_BatchSize = 512;

		[ReadOnly]
		public bool gatherData;

		[ReadOnly]
		public int parameterIndex;

		[ReadOnly]
		public int uintPerParameter;

		[ReadOnly]
		public int uintPerInstance;

		[ReadOnly]
		public NativeArray<int> componentDataIndex;

		[ReadOnly]
		public NativeArray<int> gatherIndices;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[ReadOnly]
		public NativeArray<uint> instanceData;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		[WriteOnly]
		public NativeArray<uint> tmpDataBuffer;

		public unsafe void Execute(int index)
		{
			int num = (gatherData ? gatherIndices[index] : index) * uintPerParameter;
			int num2 = UnsafeUtility.SizeOf<uint>();
			uint* source = (uint*)((byte*)instanceData.GetUnsafePtr() + (nint)num * (nint)4);
			UnsafeUtility.MemCpy((byte*)tmpDataBuffer.GetUnsafePtr() + (nint)(index * uintPerInstance) * (nint)4 + (nint)componentDataIndex[parameterIndex] * (nint)4, source, uintPerParameter * num2);
		}
	}

	private int m_UintPerInstance;

	private int m_Capacity;

	private int m_InstanceCount;

	private NativeArray<bool> m_ComponentIsInstanced;

	private NativeArray<int> m_ComponentDataIndex;

	private NativeArray<int> m_DescriptionsUintSize;

	private NativeArray<uint> m_TmpDataBuffer;

	private NativeList<int> m_WritenComponentIndices;

	private NativeArray<int> m_DummyArray;

	public GPUInstanceDataBufferUploader(in NativeArray<GPUInstanceComponentDesc> descriptions, int capacity, InstanceType instanceType)
	{
		m_Capacity = capacity;
		m_InstanceCount = 0;
		m_UintPerInstance = 0;
		m_ComponentDataIndex = new NativeArray<int>(descriptions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		m_ComponentIsInstanced = new NativeArray<bool>(descriptions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		m_DescriptionsUintSize = new NativeArray<int>(descriptions.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
		m_WritenComponentIndices = new NativeList<int>(descriptions.Length, Allocator.TempJob);
		m_DummyArray = new NativeArray<int>(0, Allocator.Persistent);
		int num = UnsafeUtility.SizeOf<uint>();
		for (int i = 0; i < descriptions.Length; i++)
		{
			GPUInstanceComponentDesc gPUInstanceComponentDesc = descriptions[i];
			m_ComponentIsInstanced[i] = gPUInstanceComponentDesc.isPerInstance;
			if (gPUInstanceComponentDesc.instanceType == instanceType)
			{
				m_ComponentDataIndex[i] = m_UintPerInstance;
				m_DescriptionsUintSize[i] = descriptions[i].byteSize / num;
				m_UintPerInstance += (gPUInstanceComponentDesc.isPerInstance ? (gPUInstanceComponentDesc.byteSize / num) : 0);
			}
			else
			{
				m_ComponentDataIndex[i] = -1;
				m_DescriptionsUintSize[i] = 0;
			}
		}
		m_TmpDataBuffer = new NativeArray<uint>(m_Capacity * m_UintPerInstance, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
	}

	public unsafe IntPtr GetUploadBufferPtr()
	{
		return new IntPtr(m_TmpDataBuffer.GetUnsafePtr());
	}

	public int GetUIntPerInstance()
	{
		return m_UintPerInstance;
	}

	public int GetParamUIntOffset(int parameterIndex)
	{
		return m_ComponentDataIndex[parameterIndex];
	}

	public int PrepareParamWrite<T>(int parameterIndex) where T : unmanaged
	{
		_ = UnsafeUtility.SizeOf<T>() / UnsafeUtility.SizeOf<uint>();
		if (!m_WritenComponentIndices.Contains(parameterIndex))
		{
			m_WritenComponentIndices.Add(in parameterIndex);
		}
		return GetParamUIntOffset(parameterIndex);
	}

	public void AllocateUploadHandles(int handlesLength)
	{
		m_InstanceCount = handlesLength;
	}

	public JobHandle WriteInstanceDataJob<T>(int parameterIndex, NativeArray<T> instanceData) where T : unmanaged
	{
		return WriteInstanceDataJob(parameterIndex, instanceData, m_DummyArray);
	}

	public JobHandle WriteInstanceDataJob<T>(int parameterIndex, NativeArray<T> instanceData, NativeArray<int> gatherIndices) where T : unmanaged
	{
		if (m_InstanceCount == 0)
		{
			return default(JobHandle);
		}
		bool gatherData = gatherIndices.Length != 0;
		int uintPerParameter = UnsafeUtility.SizeOf<T>() / UnsafeUtility.SizeOf<uint>();
		if (!m_WritenComponentIndices.Contains(parameterIndex))
		{
			m_WritenComponentIndices.Add(in parameterIndex);
		}
		WriteInstanceDataParameterJob jobData = default(WriteInstanceDataParameterJob);
		jobData.gatherData = gatherData;
		jobData.gatherIndices = gatherIndices;
		jobData.parameterIndex = parameterIndex;
		jobData.uintPerParameter = uintPerParameter;
		jobData.uintPerInstance = m_UintPerInstance;
		jobData.componentDataIndex = m_ComponentDataIndex;
		jobData.instanceData = instanceData.Reinterpret<uint>(UnsafeUtility.SizeOf<T>());
		jobData.tmpDataBuffer = m_TmpDataBuffer;
		return IJobParallelForExtensions.Schedule(jobData, m_InstanceCount, 512);
	}

	public void SubmitToGpu(GPUInstanceDataBuffer instanceDataBuffer, NativeArray<GPUInstanceIndex> gpuInstanceIndices, ref GPUResources gpuResources, bool submitOnlyWrittenParams)
	{
		if (m_InstanceCount != 0)
		{
			instanceDataBuffer.version++;
			int num = UnsafeUtility.SizeOf<uint>();
			int num2 = m_UintPerInstance * num;
			gpuResources.CreateResources(m_InstanceCount, num2, m_ComponentDataIndex.Length, m_WritenComponentIndices.Length);
			gpuResources.instanceData.SetData(m_TmpDataBuffer, 0, 0, m_InstanceCount * m_UintPerInstance);
			gpuResources.instanceIndices.SetData(gpuInstanceIndices, 0, 0, m_InstanceCount);
			gpuResources.inputComponentOffsets.SetData(m_ComponentDataIndex, 0, 0, m_ComponentDataIndex.Length);
			gpuResources.cs.SetInt(UploadKernelIDs._InputInstanceCounts, m_InstanceCount);
			gpuResources.cs.SetInt(UploadKernelIDs._InputInstanceByteSize, num2);
			gpuResources.cs.SetBuffer(gpuResources.kernelId, UploadKernelIDs._InputInstanceData, gpuResources.instanceData);
			gpuResources.cs.SetBuffer(gpuResources.kernelId, UploadKernelIDs._InputInstanceIndices, gpuResources.instanceIndices);
			gpuResources.cs.SetBuffer(gpuResources.kernelId, UploadKernelIDs._InputComponentOffsets, gpuResources.inputComponentOffsets);
			if (submitOnlyWrittenParams)
			{
				gpuResources.validComponentIndices.SetData(m_WritenComponentIndices.AsArray(), 0, 0, m_WritenComponentIndices.Length);
				gpuResources.cs.SetInt(UploadKernelIDs._InputValidComponentCounts, m_WritenComponentIndices.Length);
				gpuResources.cs.SetBuffer(gpuResources.kernelId, UploadKernelIDs._InputValidComponentIndices, gpuResources.validComponentIndices);
			}
			else
			{
				gpuResources.cs.SetInt(UploadKernelIDs._InputValidComponentCounts, instanceDataBuffer.perInstanceComponentCount);
				gpuResources.cs.SetBuffer(gpuResources.kernelId, UploadKernelIDs._InputValidComponentIndices, instanceDataBuffer.validComponentsIndicesGpuBuffer);
			}
			gpuResources.cs.SetBuffer(gpuResources.kernelId, UploadKernelIDs._InputComponentAddresses, instanceDataBuffer.componentAddressesGpuBuffer);
			gpuResources.cs.SetBuffer(gpuResources.kernelId, UploadKernelIDs._InputComponentByteCounts, instanceDataBuffer.componentByteCountsGpuBuffer);
			gpuResources.cs.SetBuffer(gpuResources.kernelId, UploadKernelIDs._InputComponentInstanceIndexRanges, instanceDataBuffer.componentInstanceIndexRangesGpuBuffer);
			gpuResources.cs.SetBuffer(gpuResources.kernelId, UploadKernelIDs._OutputBuffer, instanceDataBuffer.gpuBuffer);
			gpuResources.cs.Dispatch(gpuResources.kernelId, (m_InstanceCount + 63) / 64, 1, 1);
			m_InstanceCount = 0;
			m_WritenComponentIndices.Clear();
		}
	}

	public void SubmitToGpu(GPUInstanceDataBuffer instanceDataBuffer, NativeArray<InstanceHandle> instances, ref GPUResources gpuResources, bool submitOnlyWrittenParams)
	{
		if (m_InstanceCount != 0)
		{
			NativeArray<GPUInstanceIndex> gpuInstanceIndices = new NativeArray<GPUInstanceIndex>(instances.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			instanceDataBuffer.CPUInstanceArrayToGPUInstanceArray(instances, gpuInstanceIndices);
			SubmitToGpu(instanceDataBuffer, gpuInstanceIndices, ref gpuResources, submitOnlyWrittenParams);
			gpuInstanceIndices.Dispose();
		}
	}

	public void Dispose()
	{
		if (m_ComponentDataIndex.IsCreated)
		{
			m_ComponentDataIndex.Dispose();
		}
		if (m_ComponentIsInstanced.IsCreated)
		{
			m_ComponentIsInstanced.Dispose();
		}
		if (m_DescriptionsUintSize.IsCreated)
		{
			m_DescriptionsUintSize.Dispose();
		}
		if (m_TmpDataBuffer.IsCreated)
		{
			m_TmpDataBuffer.Dispose();
		}
		if (m_WritenComponentIndices.IsCreated)
		{
			m_WritenComponentIndices.Dispose();
		}
		if (m_DummyArray.IsCreated)
		{
			m_DummyArray.Dispose();
		}
	}
}
