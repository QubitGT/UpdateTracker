using System;

namespace UnityEngine.Rendering.Universal;

[Serializable]
internal class ScreenSpaceAmbientOcclusionSettings
{
	internal enum DepthSource
	{
		Depth,
		DepthNormals
	}

	internal enum NormalQuality
	{
		Low,
		Medium,
		High
	}

	internal enum AOSampleOption
	{
		High,
		Medium,
		Low
	}

	internal enum AOMethodOptions
	{
		BlueNoise,
		InterleavedGradient
	}

	internal enum BlurQualityOptions
	{
		High,
		Medium,
		Low
	}

	[SerializeField]
	internal AOMethodOptions AOMethod;

	[SerializeField]
	internal bool Downsample;

	[SerializeField]
	internal bool AfterOpaque;

	[SerializeField]
	internal DepthSource Source = DepthSource.DepthNormals;

	[SerializeField]
	internal NormalQuality NormalSamples = NormalQuality.Medium;

	[SerializeField]
	internal float Intensity = 3f;

	[SerializeField]
	internal float DirectLightingStrength = 0.25f;

	[SerializeField]
	internal float Radius = 0.035f;

	[SerializeField]
	internal AOSampleOption Samples = AOSampleOption.Medium;

	[SerializeField]
	internal BlurQualityOptions BlurQuality;

	[SerializeField]
	internal float Falloff = 100f;

	[SerializeField]
	internal int SampleCount = -1;
}
