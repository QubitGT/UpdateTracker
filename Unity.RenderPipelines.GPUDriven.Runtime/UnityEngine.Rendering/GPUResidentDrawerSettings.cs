namespace UnityEngine.Rendering;

public struct GPUResidentDrawerSettings
{
	public GPUResidentDrawerMode mode;

	public bool supportDitheringCrossFade;

	public bool enableOcclusionCulling;

	public bool allowInEditMode;

	public float smallMeshScreenPercentage;

	public Shader errorShader;

	public Shader loadingShader;
}
