namespace UnityEngine.Rendering.Universal;

internal struct LightStats
{
	public int totalLights;

	public int totalShadowLights;

	public int totalShadows;

	public int totalNormalMapUsage;

	public int totalVolumetricUsage;

	public int totalVolumetricShadowUsage;

	public uint blendStylesUsed;

	public uint blendStylesWithLights;

	public bool useLights => totalLights > 0;

	public bool useShadows => totalShadows > 0;

	public bool useVolumetricLights => totalVolumetricUsage > 0;

	public bool useVolumetricShadowLights => totalVolumetricShadowUsage > 0;

	public bool useNormalMap => totalNormalMapUsage > 0;
}
