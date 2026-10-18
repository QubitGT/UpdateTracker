namespace UnityEngine.Rendering;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.unity.render-pipelines.core@04755ad51d99\\Runtime\\Lighting\\ProbeVolume\\ShaderVariablesProbeVolumes.cs")]
internal class APVDefinitions
{
	public static int probeIndexChunkSize = 243;

	public const float probeValidityThreshold = 0.05f;

	public static int probeMaxRegionCount = 4;

	public static Color32[] layerMaskColors = new Color32[4]
	{
		new Color32(230, 159, 0, byte.MaxValue),
		new Color32(0, 158, 115, byte.MaxValue),
		new Color32(0, 114, 178, byte.MaxValue),
		new Color32(204, 121, 167, byte.MaxValue)
	};

	public static Color debugEmptyColor = new Color(0.388f, 0.812f, 0.804f, 1f);
}
