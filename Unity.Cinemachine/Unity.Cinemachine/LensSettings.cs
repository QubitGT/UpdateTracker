using System;
using UnityEngine;

namespace Unity.Cinemachine;

[Serializable]
public struct LensSettings
{
	public enum OverrideModes
	{
		None,
		Orthographic,
		Perspective,
		Physical
	}

	[Serializable]
	[Tooltip("These are settings that are used only if IsPhysicalCamera is true")]
	public struct PhysicalSettings
	{
		[Tooltip("How the image is fitted to the sensor if the aspect ratios differ")]
		public Camera.GateFitMode GateFit;

		[SensorSizeProperty]
		[Tooltip("This is the actual size of the image sensor (in mm)")]
		public Vector2 SensorSize;

		[Tooltip("Position of the gate relative to the film back")]
		public Vector2 LensShift;

		[Tooltip("Distance from the camera lens at which focus is sharpest.  The Depth of Field Volume override uses this value if you set FocusDistanceMode to Camera")]
		public float FocusDistance;

		[Tooltip("The sensor sensitivity (ISO)")]
		public int Iso;

		[Tooltip("The exposure time, in seconds")]
		public float ShutterSpeed;

		[Tooltip("The aperture number, in f-stop")]
		[Range(0.7f, 32f)]
		public float Aperture;

		[Tooltip("The number of diaphragm blades")]
		[Range(3f, 11f)]
		public int BladeCount;

		[Tooltip("Maps an aperture range to blade curvature")]
		[MinMaxRangeSlider(0.7f, 32f)]
		public Vector2 Curvature;

		[Tooltip("The strength of the \"cat-eye\" effect on bokeh (optical vignetting)")]
		[Range(0f, 1f)]
		public float BarrelClipping;

		[Tooltip("Stretches the sensor to simulate an anamorphic look.  Positive values distort the camera vertically, negative values distort the camera horizontally")]
		[Range(-1f, 1f)]
		public float Anamorphism;
	}

	[Tooltip("This setting controls the Field of View or Local Length of the lens, depending on whether the camera mode is physical or nonphysical.  Field of View can be either horizontal or vertical, depending on the setting in the Camera component.")]
	public float FieldOfView;

	[Tooltip("When using an orthographic camera, this defines the half-height, in world coordinates, of the camera view.")]
	public float OrthographicSize;

	[Tooltip("This defines the near region in the renderable range of the camera frustum. Raising this value will stop the game from drawing things near the camera, which can sometimes come in handy.  Larger values will also increase your shadow resolution.")]
	public float NearClipPlane;

	[Tooltip("This defines the far region of the renderable range of the camera frustum. Typically you want to set this value as low as possible without cutting off desired distant objects")]
	public float FarClipPlane;

	[Tooltip("Camera Z roll, or tilt, in degrees.")]
	public float Dutch;

	[Tooltip("Allows you to select a different camera mode to apply to the Camera component when Cinemachine activates this Virtual Camera.")]
	public OverrideModes ModeOverride;

	public PhysicalSettings PhysicalProperties;

	private bool m_OrthoFromCamera;

	private bool m_PhysicalFromCamera;

	private float m_AspectFromCamera;

	public bool Orthographic
	{
		get
		{
			if (ModeOverride != OverrideModes.Orthographic)
			{
				if (ModeOverride == OverrideModes.None)
				{
					return m_OrthoFromCamera;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsPhysicalCamera
	{
		get
		{
			if (ModeOverride != OverrideModes.Physical)
			{
				if (ModeOverride == OverrideModes.None)
				{
					return m_PhysicalFromCamera;
				}
				return false;
			}
			return true;
		}
	}

	public float Aspect
	{
		get
		{
			if (!IsPhysicalCamera)
			{
				return m_AspectFromCamera;
			}
			return PhysicalProperties.SensorSize.x / PhysicalProperties.SensorSize.y;
		}
	}

	public static LensSettings Default
	{
		get
		{
			LensSettings result = default(LensSettings);
			result.FieldOfView = 40f;
			result.OrthographicSize = 10f;
			result.NearClipPlane = 0.1f;
			result.FarClipPlane = 5000f;
			result.Dutch = 0f;
			result.ModeOverride = OverrideModes.None;
			result.PhysicalProperties = new PhysicalSettings
			{
				SensorSize = new Vector2(21.946f, 16.002f),
				GateFit = Camera.GateFitMode.Horizontal,
				FocusDistance = 10f,
				LensShift = Vector2.zero,
				Iso = 200,
				ShutterSpeed = 0.005f,
				Aperture = 16f,
				BladeCount = 5,
				Curvature = new Vector2(2f, 11f),
				BarrelClipping = 0.25f,
				Anamorphism = 0f
			};
			result.m_AspectFromCamera = 1f;
			return result;
		}
	}

	public static LensSettings FromCamera(Camera fromCamera)
	{
		LensSettings @default = Default;
		if (fromCamera != null)
		{
			@default.PullInheritedPropertiesFromCamera(fromCamera);
			@default.FieldOfView = fromCamera.fieldOfView;
			@default.OrthographicSize = fromCamera.orthographicSize;
			@default.NearClipPlane = fromCamera.nearClipPlane;
			@default.FarClipPlane = fromCamera.farClipPlane;
			if (@default.IsPhysicalCamera)
			{
				@default.FieldOfView = Camera.FocalLengthToFieldOfView(Mathf.Max(0.01f, fromCamera.focalLength), fromCamera.sensorSize.y);
				@default.PhysicalProperties.SensorSize = fromCamera.sensorSize;
				@default.PhysicalProperties.LensShift = fromCamera.lensShift;
				@default.PhysicalProperties.GateFit = fromCamera.gateFit;
				@default.PhysicalProperties.FocusDistance = fromCamera.focusDistance;
				@default.PhysicalProperties.Iso = fromCamera.iso;
				@default.PhysicalProperties.ShutterSpeed = fromCamera.shutterSpeed;
				@default.PhysicalProperties.Aperture = fromCamera.aperture;
				@default.PhysicalProperties.BladeCount = fromCamera.bladeCount;
				@default.PhysicalProperties.Curvature = fromCamera.curvature;
				@default.PhysicalProperties.BarrelClipping = fromCamera.barrelClipping;
				@default.PhysicalProperties.Anamorphism = fromCamera.anamorphism;
			}
		}
		return @default;
	}

	public void PullInheritedPropertiesFromCamera(Camera camera)
	{
		if (ModeOverride == OverrideModes.None)
		{
			m_OrthoFromCamera = camera.orthographic;
			m_PhysicalFromCamera = camera.usePhysicalProperties;
		}
		m_AspectFromCamera = camera.aspect;
	}

	public void CopyCameraMode(ref LensSettings fromLens)
	{
		ModeOverride = fromLens.ModeOverride;
		if (ModeOverride == OverrideModes.None)
		{
			m_OrthoFromCamera = fromLens.Orthographic;
			m_PhysicalFromCamera = fromLens.IsPhysicalCamera;
		}
		m_AspectFromCamera = fromLens.m_AspectFromCamera;
	}

	public static LensSettings Lerp(LensSettings lensA, LensSettings lensB, float t)
	{
		t = Mathf.Clamp01(t);
		if (t < 0.5f)
		{
			LensSettings result = lensA;
			result.Lerp(in lensB, t);
			return result;
		}
		LensSettings result2 = lensB;
		result2.Lerp(in lensA, 1f - t);
		return result2;
	}

	public void Lerp(in LensSettings lensB, float t)
	{
		FarClipPlane = Mathf.Lerp(FarClipPlane, lensB.FarClipPlane, t);
		NearClipPlane = Mathf.Lerp(NearClipPlane, lensB.NearClipPlane, t);
		FieldOfView = Mathf.Lerp(FieldOfView, lensB.FieldOfView, t);
		OrthographicSize = Mathf.Lerp(OrthographicSize, lensB.OrthographicSize, t);
		Dutch = Mathf.Lerp(Dutch, lensB.Dutch, t);
		PhysicalProperties.SensorSize = Vector2.Lerp(PhysicalProperties.SensorSize, lensB.PhysicalProperties.SensorSize, t);
		PhysicalProperties.LensShift = Vector2.Lerp(PhysicalProperties.LensShift, lensB.PhysicalProperties.LensShift, t);
		PhysicalProperties.FocusDistance = Mathf.Lerp(PhysicalProperties.FocusDistance, lensB.PhysicalProperties.FocusDistance, t);
		PhysicalProperties.Iso = Mathf.RoundToInt(Mathf.Lerp(PhysicalProperties.Iso, lensB.PhysicalProperties.Iso, t));
		PhysicalProperties.ShutterSpeed = Mathf.Lerp(PhysicalProperties.ShutterSpeed, lensB.PhysicalProperties.ShutterSpeed, t);
		PhysicalProperties.Aperture = Mathf.Lerp(PhysicalProperties.Aperture, lensB.PhysicalProperties.Aperture, t);
		PhysicalProperties.BladeCount = Mathf.RoundToInt(Mathf.Lerp(PhysicalProperties.BladeCount, lensB.PhysicalProperties.BladeCount, t));
		PhysicalProperties.Curvature = Vector2.Lerp(PhysicalProperties.Curvature, lensB.PhysicalProperties.Curvature, t);
		PhysicalProperties.BarrelClipping = Mathf.Lerp(PhysicalProperties.BarrelClipping, lensB.PhysicalProperties.BarrelClipping, t);
		PhysicalProperties.Anamorphism = Mathf.Lerp(PhysicalProperties.Anamorphism, lensB.PhysicalProperties.Anamorphism, t);
	}

	public void Validate()
	{
		FarClipPlane = Mathf.Max(FarClipPlane, NearClipPlane + 0.001f);
		FieldOfView = Mathf.Clamp(FieldOfView, 0.01f, 179f);
		PhysicalProperties.SensorSize.x = Mathf.Max(PhysicalProperties.SensorSize.x, 0.1f);
		PhysicalProperties.SensorSize.y = Mathf.Max(PhysicalProperties.SensorSize.y, 0.1f);
		PhysicalProperties.FocusDistance = Mathf.Max(PhysicalProperties.FocusDistance, 0.01f);
		if (m_AspectFromCamera == 0f)
		{
			m_AspectFromCamera = 1f;
		}
		PhysicalProperties.ShutterSpeed = Mathf.Max(0f, PhysicalProperties.ShutterSpeed);
		PhysicalProperties.Aperture = Mathf.Clamp(PhysicalProperties.Aperture, 0.7f, 32f);
		PhysicalProperties.BladeCount = Mathf.Clamp(PhysicalProperties.BladeCount, 3, 11);
		PhysicalProperties.BarrelClipping = Mathf.Clamp01(PhysicalProperties.BarrelClipping);
		PhysicalProperties.Curvature.x = Mathf.Clamp(PhysicalProperties.Curvature.x, 0.7f, 32f);
		PhysicalProperties.Curvature.y = Mathf.Clamp(PhysicalProperties.Curvature.y, PhysicalProperties.Curvature.x, 32f);
		PhysicalProperties.Anamorphism = Mathf.Clamp(PhysicalProperties.Anamorphism, -1f, 1f);
	}

	public static bool AreEqual(ref LensSettings a, ref LensSettings b)
	{
		if (Mathf.Approximately(a.NearClipPlane, b.NearClipPlane) && Mathf.Approximately(a.FarClipPlane, b.FarClipPlane) && Mathf.Approximately(a.OrthographicSize, b.OrthographicSize) && Mathf.Approximately(a.FieldOfView, b.FieldOfView) && Mathf.Approximately(a.Dutch, b.Dutch) && Mathf.Approximately(a.PhysicalProperties.LensShift.x, b.PhysicalProperties.LensShift.x) && Mathf.Approximately(a.PhysicalProperties.LensShift.y, b.PhysicalProperties.LensShift.y) && Mathf.Approximately(a.PhysicalProperties.SensorSize.x, b.PhysicalProperties.SensorSize.x) && Mathf.Approximately(a.PhysicalProperties.SensorSize.y, b.PhysicalProperties.SensorSize.y) && a.PhysicalProperties.GateFit == b.PhysicalProperties.GateFit && Mathf.Approximately(a.PhysicalProperties.FocusDistance, b.PhysicalProperties.FocusDistance) && Mathf.Approximately(a.PhysicalProperties.Iso, b.PhysicalProperties.Iso) && Mathf.Approximately(a.PhysicalProperties.ShutterSpeed, b.PhysicalProperties.ShutterSpeed) && Mathf.Approximately(a.PhysicalProperties.Aperture, b.PhysicalProperties.Aperture) && a.PhysicalProperties.BladeCount == b.PhysicalProperties.BladeCount && Mathf.Approximately(a.PhysicalProperties.Curvature.x, b.PhysicalProperties.Curvature.x) && Mathf.Approximately(a.PhysicalProperties.Curvature.y, b.PhysicalProperties.Curvature.y) && Mathf.Approximately(a.PhysicalProperties.BarrelClipping, b.PhysicalProperties.BarrelClipping))
		{
			return Mathf.Approximately(a.PhysicalProperties.Anamorphism, b.PhysicalProperties.Anamorphism);
		}
		return false;
	}
}
