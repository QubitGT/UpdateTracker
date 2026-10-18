using System;
using UnityEngine;

namespace Unity.Cinemachine;

[Serializable]
[Obsolete("LegacyLensSettings is deprecated. Use LensSettings instead.")]
public struct LegacyLensSettings
{
	public float FieldOfView;

	public float OrthographicSize;

	public float NearClipPlane;

	public float FarClipPlane;

	public float Dutch;

	public LensSettings.OverrideModes ModeOverride;

	public Camera.GateFitMode GateFit;

	[HideInInspector]
	public Vector2 m_SensorSize;

	public Vector2 LensShift;

	public float FocusDistance;

	public int Iso;

	public float ShutterSpeed;

	public float Aperture;

	public int BladeCount;

	public Vector2 Curvature;

	public float BarrelClipping;

	public float Anamorphism;

	public static LegacyLensSettings Default
	{
		get
		{
			LegacyLensSettings result = default(LegacyLensSettings);
			result.FieldOfView = 40f;
			result.OrthographicSize = 10f;
			result.NearClipPlane = 0.1f;
			result.FarClipPlane = 5000f;
			result.Dutch = 0f;
			result.ModeOverride = LensSettings.OverrideModes.None;
			result.m_SensorSize = new Vector2(21.946f, 16.002f);
			result.GateFit = Camera.GateFitMode.Horizontal;
			result.FocusDistance = 10f;
			result.LensShift = Vector2.zero;
			result.Iso = 200;
			result.ShutterSpeed = 0.005f;
			result.Aperture = 16f;
			result.BladeCount = 5;
			result.Curvature = new Vector2(2f, 11f);
			result.BarrelClipping = 0.25f;
			result.Anamorphism = 0f;
			return result;
		}
	}

	public LensSettings ToLensSettings()
	{
		LensSettings lensSettings = default(LensSettings);
		lensSettings.FieldOfView = FieldOfView;
		lensSettings.OrthographicSize = OrthographicSize;
		lensSettings.NearClipPlane = NearClipPlane;
		lensSettings.FarClipPlane = FarClipPlane;
		lensSettings.Dutch = Dutch;
		lensSettings.ModeOverride = ModeOverride;
		lensSettings.PhysicalProperties = LensSettings.Default.PhysicalProperties;
		LensSettings result = lensSettings;
		result.PhysicalProperties.GateFit = GateFit;
		result.PhysicalProperties.SensorSize = m_SensorSize;
		result.PhysicalProperties.LensShift = LensShift;
		result.PhysicalProperties.FocusDistance = FocusDistance;
		result.PhysicalProperties.Iso = Iso;
		result.PhysicalProperties.ShutterSpeed = ShutterSpeed;
		result.PhysicalProperties.Aperture = Aperture;
		result.PhysicalProperties.BladeCount = BladeCount;
		result.PhysicalProperties.Curvature = Curvature;
		result.PhysicalProperties.BarrelClipping = BarrelClipping;
		result.PhysicalProperties.Anamorphism = Anamorphism;
		return result;
	}

	public void SetFromLensSettings(LensSettings src)
	{
		FieldOfView = src.FieldOfView;
		OrthographicSize = src.OrthographicSize;
		NearClipPlane = src.NearClipPlane;
		FarClipPlane = src.FarClipPlane;
		Dutch = src.Dutch;
		ModeOverride = src.ModeOverride;
		GateFit = src.PhysicalProperties.GateFit;
		m_SensorSize = src.PhysicalProperties.SensorSize;
		LensShift = src.PhysicalProperties.LensShift;
		FocusDistance = src.PhysicalProperties.FocusDistance;
		Iso = src.PhysicalProperties.Iso;
		ShutterSpeed = src.PhysicalProperties.ShutterSpeed;
		Aperture = src.PhysicalProperties.Aperture;
		BladeCount = src.PhysicalProperties.BladeCount;
		Curvature = src.PhysicalProperties.Curvature;
		BarrelClipping = src.PhysicalProperties.BarrelClipping;
		Anamorphism = src.PhysicalProperties.Anamorphism;
	}

	public void Validate()
	{
		FarClipPlane = Mathf.Max(FarClipPlane, NearClipPlane + 0.001f);
		FieldOfView = Mathf.Clamp(FieldOfView, 0.01f, 179f);
		FocusDistance = Mathf.Max(FocusDistance, 0.01f);
		ShutterSpeed = Mathf.Max(0f, ShutterSpeed);
		Aperture = Mathf.Clamp(Aperture, 0.7f, 32f);
		BladeCount = Mathf.Clamp(BladeCount, 3, 11);
		BarrelClipping = Mathf.Clamp01(BarrelClipping);
		Curvature.x = Mathf.Clamp(Curvature.x, 0.7f, 32f);
		Curvature.y = Mathf.Clamp(Curvature.y, Curvature.x, 32f);
		Anamorphism = Mathf.Clamp(Anamorphism, -1f, 1f);
	}
}
