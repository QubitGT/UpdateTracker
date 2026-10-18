namespace UnityEngine.Rendering.Universal;

internal enum DecalSurfaceData
{
	[Tooltip("Decals will affect only base color and emission.")]
	Albedo,
	[Tooltip("Decals will affect only base color, normal and emission.")]
	AlbedoNormal,
	[Tooltip("Decals will affect base color, normal, metallic, ambient occlusion, smoothness and emission.")]
	AlbedoNormalMAOS
}
