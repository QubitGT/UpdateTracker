using System;

namespace UnityEngine.Rendering.UnifiedRayTracing;

internal static class RayTracingHelper
{
	public const GraphicsBuffer.Target ScratchBufferTarget = GraphicsBuffer.Target.Structured;

	public static readonly uint k_DimensionByteOffset = 0u;

	public static readonly uint k_GroupSizeByteOffset = 12u;

	public static GraphicsBuffer CreateDispatchDimensionBuffer()
	{
		return new GraphicsBuffer(GraphicsBuffer.Target.CopySource | GraphicsBuffer.Target.Structured | GraphicsBuffer.Target.IndirectArguments, 6, 4);
	}

	public static GraphicsBuffer CreateScratchBufferForBuildAndDispatch(IRayTracingAccelStruct accelStruct, IRayTracingShader shader, uint dispatchWidth, uint dispatchHeight, uint dispatchDepth)
	{
		ulong num = Math.Max(accelStruct.GetBuildScratchBufferRequiredSizeInBytes(), shader.GetTraceScratchBufferRequiredSizeInBytes(dispatchWidth, dispatchHeight, dispatchDepth));
		if (num == 0L)
		{
			return null;
		}
		return new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)(num / 4), 4);
	}

	public static GraphicsBuffer CreateScratchBufferForBuild(IRayTracingAccelStruct accelStruct)
	{
		ulong buildScratchBufferRequiredSizeInBytes = accelStruct.GetBuildScratchBufferRequiredSizeInBytes();
		return new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)(buildScratchBufferRequiredSizeInBytes / 4), 4);
	}

	public static void ResizeScratchBufferForTrace(IRayTracingShader shader, uint dispatchWidth, uint dispatchHeight, uint dispatchDepth, ref GraphicsBuffer scratchBuffer)
	{
		ulong traceScratchBufferRequiredSizeInBytes = shader.GetTraceScratchBufferRequiredSizeInBytes(dispatchWidth, dispatchHeight, dispatchDepth);
		if (traceScratchBufferRequiredSizeInBytes != 0L && (scratchBuffer == null || (ulong)(scratchBuffer.count * scratchBuffer.stride) < traceScratchBufferRequiredSizeInBytes))
		{
			scratchBuffer?.Dispose();
			scratchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)(traceScratchBufferRequiredSizeInBytes / 4), 4);
		}
	}

	public static void ResizeScratchBufferForBuild(IRayTracingAccelStruct accelStruct, ref GraphicsBuffer scratchBuffer)
	{
		ulong buildScratchBufferRequiredSizeInBytes = accelStruct.GetBuildScratchBufferRequiredSizeInBytes();
		if (buildScratchBufferRequiredSizeInBytes != 0L && (scratchBuffer == null || (ulong)(scratchBuffer.count * scratchBuffer.stride) < buildScratchBufferRequiredSizeInBytes))
		{
			scratchBuffer?.Dispose();
			scratchBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)(buildScratchBufferRequiredSizeInBytes / 4), 4);
		}
	}
}
