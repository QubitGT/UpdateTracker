using System;

namespace UnityEngine.Rendering;

internal struct GPUInstanceDataBufferGrower : IDisposable
{
	private static class CopyInstancesKernelIDs
	{
		public static readonly int _InputValidComponentCounts = Shader.PropertyToID("_InputValidComponentCounts");

		public static readonly int _InstanceCounts = Shader.PropertyToID("_InstanceCounts");

		public static readonly int _InstanceOffset = Shader.PropertyToID("_InstanceOffset");

		public static readonly int _OutputInstanceOffset = Shader.PropertyToID("_OutputInstanceOffset");

		public static readonly int _ValidComponentIndices = Shader.PropertyToID("_ValidComponentIndices");

		public static readonly int _ComponentByteCounts = Shader.PropertyToID("_ComponentByteCounts");

		public static readonly int _InputComponentAddresses = Shader.PropertyToID("_InputComponentAddresses");

		public static readonly int _OutputComponentAddresses = Shader.PropertyToID("_OutputComponentAddresses");

		public static readonly int _InputComponentInstanceIndexRanges = Shader.PropertyToID("_InputComponentInstanceIndexRanges");

		public static readonly int _InputBuffer = Shader.PropertyToID("_InputBuffer");

		public static readonly int _OutputBuffer = Shader.PropertyToID("_OutputBuffer");
	}

	public struct GPUResources : IDisposable
	{
		public ComputeShader cs;

		public int kernelId;

		public void LoadShaders(GPUResidentDrawerResources resources)
		{
			if (cs == null)
			{
				cs = resources.instanceDataBufferCopyKernels;
				kernelId = cs.FindKernel("MainCopyInstances");
			}
		}

		public void CreateResources()
		{
		}

		public void Dispose()
		{
			cs = null;
		}
	}

	private GPUInstanceDataBuffer m_SrcBuffer;

	private GPUInstanceDataBuffer m_DstBuffer;

	public unsafe GPUInstanceDataBufferGrower(GPUInstanceDataBuffer sourceBuffer, in InstanceNumInfo instanceNumInfo)
	{
		m_SrcBuffer = sourceBuffer;
		m_DstBuffer = null;
		bool flag = false;
		for (int i = 0; i < 2; i++)
		{
			if (instanceNumInfo.InstanceNums[i] > sourceBuffer.instanceNumInfo.InstanceNums[i])
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		GPUInstanceDataBufferBuilder gPUInstanceDataBufferBuilder = default(GPUInstanceDataBufferBuilder);
		foreach (GPUInstanceComponentDesc description in sourceBuffer.descriptions)
		{
			gPUInstanceDataBufferBuilder.AddComponent(description.propertyID, description.isOverriden, description.byteSize, description.isPerInstance, description.instanceType, description.componentGroup);
		}
		m_DstBuffer = gPUInstanceDataBufferBuilder.Build(in instanceNumInfo);
		gPUInstanceDataBufferBuilder.Dispose();
	}

	public GPUInstanceDataBuffer SubmitToGpu(ref GPUResources gpuResources)
	{
		if (m_DstBuffer == null)
		{
			return m_SrcBuffer;
		}
		if (m_SrcBuffer.instanceNumInfo.GetTotalInstanceNum() == 0)
		{
			return m_DstBuffer;
		}
		gpuResources.CreateResources();
		gpuResources.cs.SetInt(CopyInstancesKernelIDs._InputValidComponentCounts, m_SrcBuffer.perInstanceComponentCount);
		gpuResources.cs.SetBuffer(gpuResources.kernelId, CopyInstancesKernelIDs._ValidComponentIndices, m_SrcBuffer.validComponentsIndicesGpuBuffer);
		gpuResources.cs.SetBuffer(gpuResources.kernelId, CopyInstancesKernelIDs._ComponentByteCounts, m_SrcBuffer.componentByteCountsGpuBuffer);
		gpuResources.cs.SetBuffer(gpuResources.kernelId, CopyInstancesKernelIDs._InputComponentAddresses, m_SrcBuffer.componentAddressesGpuBuffer);
		gpuResources.cs.SetBuffer(gpuResources.kernelId, CopyInstancesKernelIDs._InputComponentInstanceIndexRanges, m_SrcBuffer.componentInstanceIndexRangesGpuBuffer);
		gpuResources.cs.SetBuffer(gpuResources.kernelId, CopyInstancesKernelIDs._OutputComponentAddresses, m_DstBuffer.componentAddressesGpuBuffer);
		gpuResources.cs.SetBuffer(gpuResources.kernelId, CopyInstancesKernelIDs._InputBuffer, m_SrcBuffer.gpuBuffer);
		gpuResources.cs.SetBuffer(gpuResources.kernelId, CopyInstancesKernelIDs._OutputBuffer, m_DstBuffer.gpuBuffer);
		for (int i = 0; i < 2; i++)
		{
			int instanceNum = m_SrcBuffer.instanceNumInfo.GetInstanceNum((InstanceType)i);
			if (instanceNum > 0)
			{
				int val = m_SrcBuffer.instancesNumPrefixSum[i];
				int val2 = m_DstBuffer.instancesNumPrefixSum[i];
				gpuResources.cs.SetInt(CopyInstancesKernelIDs._InstanceCounts, instanceNum);
				gpuResources.cs.SetInt(CopyInstancesKernelIDs._InstanceOffset, val);
				gpuResources.cs.SetInt(CopyInstancesKernelIDs._OutputInstanceOffset, val2);
				gpuResources.cs.Dispatch(gpuResources.kernelId, (instanceNum + 63) / 64, 1, 1);
			}
		}
		return m_DstBuffer;
	}

	public void Dispose()
	{
	}
}
