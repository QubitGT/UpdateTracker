namespace UnityEngine.Rendering;

internal struct OcclusionCullingDebugOutput
{
	public RTHandle occluderDepthPyramid;

	public GraphicsBuffer occlusionDebugOverlay;

	public OcclusionCullingDebugShaderVariables cb;
}
