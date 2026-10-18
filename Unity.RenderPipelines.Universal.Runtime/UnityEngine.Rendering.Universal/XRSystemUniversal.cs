using UnityEngine.Experimental.Rendering;
using UnityEngine.XR;

namespace UnityEngine.Rendering.Universal;

internal static class XRSystemUniversal
{
	private static Matrix4x4[] s_projMatrix = new Matrix4x4[2];

	private static MaterialPropertyBlock s_XRSharedPropertyBlock = new MaterialPropertyBlock();

	internal static MaterialPropertyBlock GetMaterialPropertyBlock()
	{
		return s_XRSharedPropertyBlock;
	}

	internal static void BeginLateLatching(Camera camera, XRPassUniversal xrPass)
	{
		XRDisplaySubsystem activeDisplay = XRSystem.GetActiveDisplay();
		if (activeDisplay != null && xrPass.viewCount == 2)
		{
			activeDisplay.BeginRecordingIfLateLatched(camera);
			xrPass.isLateLatchEnabled = true;
		}
	}

	internal static void EndLateLatching(Camera camera, XRPassUniversal xrPass)
	{
		XRDisplaySubsystem activeDisplay = XRSystem.GetActiveDisplay();
		if (activeDisplay != null && xrPass.isLateLatchEnabled)
		{
			activeDisplay.EndRecordingIfLateLatched(camera);
			xrPass.isLateLatchEnabled = false;
		}
	}

	internal static void UnmarkShaderProperties(RasterCommandBuffer cmd, XRPassUniversal xrPass)
	{
		if (xrPass.isLateLatchEnabled && xrPass.hasMarkedLateLatch)
		{
			cmd.UnmarkLateLatchMatrix(CameraLateLatchMatrixType.View);
			cmd.UnmarkLateLatchMatrix(CameraLateLatchMatrixType.InverseView);
			cmd.UnmarkLateLatchMatrix(CameraLateLatchMatrixType.ViewProjection);
			cmd.UnmarkLateLatchMatrix(CameraLateLatchMatrixType.InverseViewProjection);
			xrPass.hasMarkedLateLatch = false;
		}
	}

	internal static void MarkShaderProperties(RasterCommandBuffer cmd, XRPassUniversal xrPass, bool renderIntoTexture)
	{
		if (xrPass.isLateLatchEnabled && xrPass.canMarkLateLatch)
		{
			cmd.MarkLateLatchMatrixShaderPropertyID(CameraLateLatchMatrixType.View, XRBuiltinShaderConstants.unity_StereoMatrixV);
			cmd.MarkLateLatchMatrixShaderPropertyID(CameraLateLatchMatrixType.InverseView, XRBuiltinShaderConstants.unity_StereoMatrixInvV);
			cmd.MarkLateLatchMatrixShaderPropertyID(CameraLateLatchMatrixType.ViewProjection, XRBuiltinShaderConstants.unity_StereoMatrixVP);
			cmd.MarkLateLatchMatrixShaderPropertyID(CameraLateLatchMatrixType.InverseViewProjection, XRBuiltinShaderConstants.unity_StereoMatrixInvVP);
			for (int i = 0; i < 2; i++)
			{
				s_projMatrix[i] = GL.GetGPUProjectionMatrix(xrPass.GetProjMatrix(i), renderIntoTexture);
			}
			cmd.SetLateLatchProjectionMatrices(s_projMatrix);
			xrPass.hasMarkedLateLatch = true;
		}
	}
}
