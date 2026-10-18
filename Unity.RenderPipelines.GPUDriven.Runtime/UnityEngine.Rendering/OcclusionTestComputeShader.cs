namespace UnityEngine.Rendering;

internal struct OcclusionTestComputeShader
{
	public ComputeShader cs;

	public LocalKeyword occlusionDebugKeyword;

	public void Init(ComputeShader cs)
	{
		this.cs = cs;
		occlusionDebugKeyword = new LocalKeyword(cs, "OCCLUSION_DEBUG");
	}
}
