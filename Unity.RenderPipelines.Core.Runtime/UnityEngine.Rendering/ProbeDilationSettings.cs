using System;

namespace UnityEngine.Rendering;

[Serializable]
internal struct ProbeDilationSettings
{
	public bool enableDilation;

	public float dilationDistance;

	public float dilationValidityThreshold;

	public int dilationIterations;

	public bool squaredDistWeighting;

	internal void SetDefaults()
	{
		enableDilation = false;
		dilationDistance = 1f;
		dilationValidityThreshold = 0.25f;
		dilationIterations = 1;
		squaredDistWeighting = true;
	}

	internal void UpgradeFromTo(ProbeVolumeBakingProcessSettings.SettingsVersion from, ProbeVolumeBakingProcessSettings.SettingsVersion to)
	{
	}
}
