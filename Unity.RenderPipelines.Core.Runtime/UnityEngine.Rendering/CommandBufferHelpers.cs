using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.VFX;

namespace UnityEngine.Rendering;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CommandBufferHelpers
{
	internal static RasterCommandBuffer rasterCmd = new RasterCommandBuffer(null, null, isAsync: false);

	internal static ComputeCommandBuffer computeCmd = new ComputeCommandBuffer(null, null, isAsync: false);

	internal static UnsafeCommandBuffer unsafeCmd = new UnsafeCommandBuffer(null, null, isAsync: false);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static RasterCommandBuffer GetRasterCommandBuffer(CommandBuffer baseBuffer)
	{
		rasterCmd.m_WrappedCommandBuffer = baseBuffer;
		return rasterCmd;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ComputeCommandBuffer GetComputeCommandBuffer(CommandBuffer baseBuffer)
	{
		computeCmd.m_WrappedCommandBuffer = baseBuffer;
		return computeCmd;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static UnsafeCommandBuffer GetUnsafeCommandBuffer(CommandBuffer baseBuffer)
	{
		unsafeCmd.m_WrappedCommandBuffer = baseBuffer;
		return unsafeCmd;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static CommandBuffer GetNativeCommandBuffer(UnsafeCommandBuffer baseBuffer)
	{
		return baseBuffer.m_WrappedCommandBuffer;
	}

	public static void VFXManager_ProcessCameraCommand(Camera cam, UnsafeCommandBuffer cmd, VFXCameraXRSettings camXRSettings, CullingResults results)
	{
		VFXManager.ProcessCameraCommand(cam, cmd.m_WrappedCommandBuffer, camXRSettings, results);
	}
}
