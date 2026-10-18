using Unity.Mathematics;
using UnityEngine.Rendering.RadeonRays;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal class ComputeRayTracingShader : IRayTracingShader
{
	private readonly ComputeShader m_Shader;

	private readonly int m_KernelIndex;

	private readonly int m_ComputeIndirectDispatchDimsKernelIndex;

	private uint3 m_ThreadGroupSizes;

	private readonly GraphicsBuffer m_DispatchBuffer;

	internal ComputeRayTracingShader(ComputeShader shader, string dispatchFuncName, GraphicsBuffer dispatchBuffer)
	{
		m_Shader = shader;
		m_KernelIndex = m_Shader.FindKernel(dispatchFuncName);
		m_ComputeIndirectDispatchDimsKernelIndex = m_Shader.FindKernel("ComputeIndirectDispatchDims");
		m_Shader.GetKernelThreadGroupSizes(m_KernelIndex, out m_ThreadGroupSizes.x, out m_ThreadGroupSizes.y, out m_ThreadGroupSizes.z);
		m_DispatchBuffer = dispatchBuffer;
	}

	public uint3 GetThreadGroupSizes()
	{
		return m_ThreadGroupSizes;
	}

	public void SetAccelerationStructure(CommandBuffer cmd, string name, IRayTracingAccelStruct accelStruct)
	{
		(accelStruct as ComputeRayTracingAccelStruct).Bind(cmd, name, this);
	}

	public void SetIntParam(CommandBuffer cmd, int nameID, int val)
	{
		cmd.SetComputeIntParam(m_Shader, nameID, val);
	}

	public void SetFloatParam(CommandBuffer cmd, int nameID, float val)
	{
		cmd.SetComputeFloatParam(m_Shader, nameID, val);
	}

	public void SetVectorParam(CommandBuffer cmd, int nameID, Vector4 val)
	{
		cmd.SetComputeVectorParam(m_Shader, nameID, val);
	}

	public void SetMatrixParam(CommandBuffer cmd, int nameID, Matrix4x4 val)
	{
		cmd.SetComputeMatrixParam(m_Shader, nameID, val);
	}

	public void SetTextureParam(CommandBuffer cmd, int nameID, RenderTargetIdentifier rt)
	{
		cmd.SetComputeTextureParam(m_Shader, m_KernelIndex, nameID, rt);
	}

	public void SetBufferParam(CommandBuffer cmd, int nameID, GraphicsBuffer buffer)
	{
		cmd.SetComputeBufferParam(m_Shader, m_KernelIndex, nameID, buffer);
	}

	public void SetBufferParam(CommandBuffer cmd, int nameID, ComputeBuffer buffer)
	{
		cmd.SetComputeBufferParam(m_Shader, m_KernelIndex, nameID, buffer);
	}

	public void Dispatch(CommandBuffer cmd, GraphicsBuffer scratchBuffer, uint width, uint height, uint depth)
	{
		GetTraceScratchBufferRequiredSizeInBytes(width, height, depth);
		_ = 0;
		cmd.SetComputeBufferParam(m_Shader, m_KernelIndex, SID.g_stack, scratchBuffer);
		cmd.SetBufferData(m_DispatchBuffer, new uint[3] { width, height, depth });
		SetBufferParam(cmd, SID.g_dispatch_dimensions, m_DispatchBuffer);
		uint threadGroupsX = (uint)GraphicsHelpers.DivUp((int)width, m_ThreadGroupSizes.x);
		uint threadGroupsY = (uint)GraphicsHelpers.DivUp((int)height, m_ThreadGroupSizes.y);
		uint threadGroupsZ = (uint)GraphicsHelpers.DivUp((int)depth, m_ThreadGroupSizes.z);
		cmd.DispatchCompute(m_Shader, m_KernelIndex, (int)threadGroupsX, (int)threadGroupsY, (int)threadGroupsZ);
	}

	public void Dispatch(CommandBuffer cmd, GraphicsBuffer scratchBuffer, GraphicsBuffer argsBuffer)
	{
		SetIndirectDispatchDimensions(cmd, argsBuffer);
		DispatchIndirect(cmd, scratchBuffer, argsBuffer);
	}

	internal void SetIndirectDispatchDimensions(CommandBuffer cmd, GraphicsBuffer argsBuffer)
	{
		cmd.SetComputeBufferParam(m_Shader, m_ComputeIndirectDispatchDimsKernelIndex, SID.g_dispatch_dimensions, argsBuffer);
		cmd.SetComputeBufferParam(m_Shader, m_ComputeIndirectDispatchDimsKernelIndex, SID.g_dispatch_dims_in_workgroups, m_DispatchBuffer);
		cmd.DispatchCompute(m_Shader, m_ComputeIndirectDispatchDimsKernelIndex, 1, 1, 1);
	}

	internal void DispatchIndirect(CommandBuffer cmd, GraphicsBuffer scratchBuffer, GraphicsBuffer argsBuffer)
	{
		cmd.SetComputeBufferParam(m_Shader, m_KernelIndex, SID.g_stack, scratchBuffer);
		cmd.SetComputeBufferParam(m_Shader, m_KernelIndex, SID.g_dispatch_dimensions, argsBuffer);
		cmd.DispatchCompute(m_Shader, m_KernelIndex, m_DispatchBuffer, 0u);
	}

	public ulong GetTraceScratchBufferRequiredSizeInBytes(uint width, uint height, uint depth)
	{
		return RadeonRaysAPI.GetTraceMemoryRequirements(width * height * depth) * 4;
	}
}
