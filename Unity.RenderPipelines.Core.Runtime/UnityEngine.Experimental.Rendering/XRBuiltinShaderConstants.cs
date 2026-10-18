using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering;

public static class XRBuiltinShaderConstants
{
	public static readonly int unity_StereoCameraProjection = Shader.PropertyToID("unity_StereoCameraProjection");

	public static readonly int unity_StereoCameraInvProjection = Shader.PropertyToID("unity_StereoCameraInvProjection");

	public static readonly int unity_StereoMatrixV = Shader.PropertyToID("unity_StereoMatrixV");

	public static readonly int unity_StereoMatrixInvV = Shader.PropertyToID("unity_StereoMatrixInvV");

	public static readonly int unity_StereoMatrixP = Shader.PropertyToID("unity_StereoMatrixP");

	public static readonly int unity_StereoMatrixInvP = Shader.PropertyToID("unity_StereoMatrixInvP");

	public static readonly int unity_StereoMatrixVP = Shader.PropertyToID("unity_StereoMatrixVP");

	public static readonly int unity_StereoMatrixInvVP = Shader.PropertyToID("unity_StereoMatrixInvVP");

	public static readonly int unity_StereoWorldSpaceCameraPos = Shader.PropertyToID("unity_StereoWorldSpaceCameraPos");

	private static Matrix4x4[] s_cameraProjMatrix = new Matrix4x4[2];

	private static Matrix4x4[] s_invCameraProjMatrix = new Matrix4x4[2];

	private static Matrix4x4[] s_viewMatrix = new Matrix4x4[2];

	private static Matrix4x4[] s_invViewMatrix = new Matrix4x4[2];

	private static Matrix4x4[] s_projMatrix = new Matrix4x4[2];

	private static Matrix4x4[] s_invProjMatrix = new Matrix4x4[2];

	private static Matrix4x4[] s_viewProjMatrix = new Matrix4x4[2];

	private static Matrix4x4[] s_invViewProjMatrix = new Matrix4x4[2];

	private static Vector4[] s_worldSpaceCameraPos = new Vector4[2];

	public static void UpdateBuiltinShaderConstants(Matrix4x4 viewMatrix, Matrix4x4 projMatrix, bool renderIntoTexture, int viewIndex)
	{
		Matrix4x4 gPUProjectionMatrix = GL.GetGPUProjectionMatrix(projMatrix, renderIntoTexture);
		Matrix4x4 matrix4x = gPUProjectionMatrix * viewMatrix;
		s_cameraProjMatrix[viewIndex] = projMatrix;
		s_projMatrix[viewIndex] = gPUProjectionMatrix;
		s_viewMatrix[viewIndex] = viewMatrix;
		Matrix4x4.Inverse3DAffine(viewMatrix, ref s_invViewMatrix[viewIndex]);
		s_viewProjMatrix[viewIndex] = matrix4x;
		s_invCameraProjMatrix[viewIndex] = Matrix4x4.Inverse(projMatrix);
		s_invProjMatrix[viewIndex] = Matrix4x4.Inverse(gPUProjectionMatrix);
		s_invViewProjMatrix[viewIndex] = Matrix4x4.Inverse(matrix4x);
		s_worldSpaceCameraPos[viewIndex] = s_invViewMatrix[viewIndex].GetColumn(3);
	}

	public static void SetBuiltinShaderConstants(CommandBuffer cmd)
	{
		cmd.SetGlobalMatrixArray(unity_StereoCameraProjection, s_cameraProjMatrix);
		cmd.SetGlobalMatrixArray(unity_StereoCameraInvProjection, s_invCameraProjMatrix);
		cmd.SetGlobalMatrixArray(unity_StereoMatrixV, s_viewMatrix);
		cmd.SetGlobalMatrixArray(unity_StereoMatrixInvV, s_invViewMatrix);
		cmd.SetGlobalMatrixArray(unity_StereoMatrixP, s_projMatrix);
		cmd.SetGlobalMatrixArray(unity_StereoMatrixInvP, s_invProjMatrix);
		cmd.SetGlobalMatrixArray(unity_StereoMatrixVP, s_viewProjMatrix);
		cmd.SetGlobalMatrixArray(unity_StereoMatrixInvVP, s_invViewProjMatrix);
		cmd.SetGlobalVectorArray(unity_StereoWorldSpaceCameraPos, s_worldSpaceCameraPos);
	}

	public static void SetBuiltinShaderConstants(RasterCommandBuffer cmd)
	{
		SetBuiltinShaderConstants(cmd.m_WrappedCommandBuffer);
	}

	public static void Update(XRPass xrPass, CommandBuffer cmd, bool renderIntoTexture)
	{
		if (!xrPass.enabled)
		{
			return;
		}
		cmd.SetViewProjectionMatrices(xrPass.GetViewMatrix(), xrPass.GetProjMatrix());
		if (xrPass.singlePassEnabled)
		{
			for (int i = 0; i < 2; i++)
			{
				s_cameraProjMatrix[i] = xrPass.GetProjMatrix(i);
				s_viewMatrix[i] = xrPass.GetViewMatrix(i);
				s_projMatrix[i] = GL.GetGPUProjectionMatrix(s_cameraProjMatrix[i], renderIntoTexture);
				s_viewProjMatrix[i] = s_projMatrix[i] * s_viewMatrix[i];
				s_invCameraProjMatrix[i] = Matrix4x4.Inverse(s_cameraProjMatrix[i]);
				Matrix4x4.Inverse3DAffine(s_viewMatrix[i], ref s_invViewMatrix[i]);
				s_invProjMatrix[i] = Matrix4x4.Inverse(s_projMatrix[i]);
				s_invViewProjMatrix[i] = Matrix4x4.Inverse(s_viewProjMatrix[i]);
				s_worldSpaceCameraPos[i] = s_invViewMatrix[i].GetColumn(3);
			}
			cmd.SetGlobalMatrixArray(unity_StereoCameraProjection, s_cameraProjMatrix);
			cmd.SetGlobalMatrixArray(unity_StereoCameraInvProjection, s_invCameraProjMatrix);
			cmd.SetGlobalMatrixArray(unity_StereoMatrixV, s_viewMatrix);
			cmd.SetGlobalMatrixArray(unity_StereoMatrixInvV, s_invViewMatrix);
			cmd.SetGlobalMatrixArray(unity_StereoMatrixP, s_projMatrix);
			cmd.SetGlobalMatrixArray(unity_StereoMatrixInvP, s_invProjMatrix);
			cmd.SetGlobalMatrixArray(unity_StereoMatrixVP, s_viewProjMatrix);
			cmd.SetGlobalMatrixArray(unity_StereoMatrixInvVP, s_invViewProjMatrix);
			cmd.SetGlobalVectorArray(unity_StereoWorldSpaceCameraPos, s_worldSpaceCameraPos);
		}
	}
}
