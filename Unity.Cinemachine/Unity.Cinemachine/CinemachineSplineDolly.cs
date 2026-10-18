using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Position Control/Cinemachine Spline Dolly")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Body)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineSplineDolly.html")]
public class CinemachineSplineDolly : CinemachineComponentBase, ISplineReferencer
{
	public enum RotationMode
	{
		Default,
		Spline,
		SplineNoRoll,
		FollowTarget,
		FollowTargetNoRoll
	}

	[Serializable]
	public struct DampingSettings
	{
		[Tooltip("Enables damping, which causes the camera to move gradually towards the desired spline position")]
		public bool Enabled;

		[Tooltip("How aggressively the camera tries to maintain the offset along the x, y, or z directions in spline local space. \n- x represents the axis that is perpendicular to the spline. Use this to smooth out imperfections in the path. This may move the camera off the spline.\n- y represents the axis that is defined by the spline-local up direction. Use this to smooth out imperfections in the path. This may move the camera off the spline.\n- z represents the axis that is parallel to the spline. This won't move the camera off the spline.\n\nSmaller numbers are more responsive, larger numbers give a heavier more slowly responding camera. Using different settings per axis can yield a wide range of camera behaviors.")]
		public Vector3 Position;

		[Range(0f, 20f)]
		[Tooltip("How aggressively the camera tries to maintain the desired rotation.  This is only used if Camera Rotation is not Default.")]
		public float Angular;
	}

	[SerializeField]
	[FormerlySerializedAs("SplineSettings")]
	private SplineSettings m_SplineSettings = new SplineSettings
	{
		Units = PathIndexUnit.Normalized
	};

	[Tooltip("Where to put the camera relative to the spline position.  X is perpendicular to the spline, Y is up, and Z is parallel to the spline.")]
	public Vector3 SplineOffset = Vector3.zero;

	[Tooltip("How to set the camera's rotation and Up.  This will affect the screen composition, because the camera Aim behaviours will always try to respect the Up direction.")]
	[FormerlySerializedAs("CameraUp")]
	public RotationMode CameraRotation;

	[FoldoutWithEnabledButton("Enabled")]
	[Tooltip("Settings for controlling damping, which causes the camera to move gradually towards the desired spline position")]
	public DampingSettings Damping;

	[NoSaveDuringPlay]
	[FoldoutWithEnabledButton("Enabled")]
	[Tooltip("Controls how automatic dolly occurs.  A tracking target may be necessary to use this feature.")]
	public SplineAutoDolly AutomaticDolly;

	private float m_PreviousSplinePosition;

	private Quaternion m_PreviousRotation;

	private Vector3 m_PreviousPosition;

	private CinemachineSplineRoll.RollCache m_RollCache;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("CameraPosition")]
	private float m_LegacyPosition = -1f;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("PositionUnits")]
	private PathIndexUnit m_LegacyUnits;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("Spline")]
	private SplineContainer m_LegacySpline;

	public ref SplineSettings SplineSettings => ref m_SplineSettings;

	public SplineContainer Spline
	{
		get
		{
			return m_SplineSettings.Spline;
		}
		set
		{
			m_SplineSettings.Spline = value;
		}
	}

	public float CameraPosition
	{
		get
		{
			return m_SplineSettings.Position;
		}
		set
		{
			m_SplineSettings.Position = value;
		}
	}

	public PathIndexUnit PositionUnits
	{
		get
		{
			return m_SplineSettings.Units;
		}
		set
		{
			m_SplineSettings.ChangeUnitPreservePosition(value);
		}
	}

	public override bool IsValid
	{
		get
		{
			if (base.enabled)
			{
				return Spline != null;
			}
			return false;
		}
	}

	public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Body;

	private void PerformLegacyUpgrade()
	{
		if (m_LegacyPosition != -1f)
		{
			m_SplineSettings.Position = m_LegacyPosition;
			m_SplineSettings.Units = m_LegacyUnits;
			m_LegacyPosition = -1f;
			m_LegacyUnits = PathIndexUnit.Distance;
		}
		if (m_LegacySpline != null)
		{
			m_SplineSettings.Spline = m_LegacySpline;
			m_LegacySpline = null;
		}
	}

	private void OnValidate()
	{
		PerformLegacyUpgrade();
		Damping.Position.x = Mathf.Clamp(Damping.Position.x, 0f, 20f);
		Damping.Position.y = Mathf.Clamp(Damping.Position.y, 0f, 20f);
		Damping.Position.z = Mathf.Clamp(Damping.Position.z, 0f, 20f);
		Damping.Angular = Mathf.Clamp(Damping.Angular, 0f, 20f);
		AutomaticDolly.Method?.Validate();
	}

	private void Reset()
	{
		m_SplineSettings = new SplineSettings
		{
			Units = PathIndexUnit.Normalized
		};
		SplineOffset = Vector3.zero;
		CameraRotation = RotationMode.Default;
		Damping = default(DampingSettings);
		AutomaticDolly.Method = null;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_RollCache.Refresh(this);
		AutomaticDolly.Method?.Reset();
	}

	protected override void OnDisable()
	{
		m_SplineSettings.InvalidateCache();
		base.OnDisable();
	}

	public override float GetMaxDampTime()
	{
		if (Damping.Enabled)
		{
			return Mathf.Max(Mathf.Max(Damping.Position.x, Mathf.Max(Damping.Position.y, Damping.Position.z)), Damping.Angular);
		}
		return 0f;
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		if (!IsValid)
		{
			return;
		}
		CachedScaledSpline cachedSpline = m_SplineSettings.GetCachedSpline();
		if (cachedSpline == null)
		{
			return;
		}
		float maxPos;
		float num = cachedSpline.StandardizePosition(CameraPosition, PositionUnits, out maxPos);
		if (deltaTime < 0f || !base.VirtualCamera.PreviousStateIsValid)
		{
			m_PreviousSplinePosition = num;
			m_PreviousPosition = curState.RawPosition;
			m_PreviousRotation = curState.RawOrientation;
			m_RollCache.Refresh(this);
		}
		if (AutomaticDolly.Enabled && AutomaticDolly.Method != null)
		{
			num = AutomaticDolly.Method.GetSplinePosition(this, base.FollowTarget, Spline, num, PositionUnits, deltaTime);
		}
		if (Damping.Enabled && deltaTime >= 0f && base.VirtualCamera.PreviousStateIsValid)
		{
			float num2 = m_PreviousSplinePosition;
			if (cachedSpline.Closed && Mathf.Abs(num - num2) > maxPos * 0.5f)
			{
				num2 += ((num > num2) ? maxPos : (0f - maxPos));
			}
			num = num2 + Damper.Damp(num - num2, Damping.Position.z, deltaTime);
		}
		float previousSplinePosition = (CameraPosition = num);
		m_PreviousSplinePosition = previousSplinePosition;
		cachedSpline.EvaluateSplineWithRoll(Spline.transform, cachedSpline.ConvertIndexUnit(num, PositionUnits, PathIndexUnit.Normalized), m_RollCache.GetSplineRoll(this), out var position, out var rotation);
		Vector3 vector = rotation * Vector3.right;
		Vector3 vector2 = rotation * Vector3.up;
		Vector3 vector3 = rotation * Vector3.forward;
		position += SplineOffset.x * vector;
		position += SplineOffset.y * vector2;
		position += SplineOffset.z * vector3;
		if (Damping.Enabled && deltaTime >= 0f && base.VirtualCamera.PreviousStateIsValid)
		{
			Vector3 previousPosition = m_PreviousPosition;
			Vector3 vector4 = previousPosition - position;
			Vector3 vector5 = Vector3.Dot(vector4, vector2) * vector2;
			Vector3 initial = vector4 - vector5;
			initial = Damper.Damp(initial, Damping.Position.x, deltaTime);
			vector5 = Damper.Damp(vector5, Damping.Position.y, deltaTime);
			position = previousPosition - (initial + vector5);
		}
		curState.RawPosition = (m_PreviousPosition = position);
		bool isDefault;
		Quaternion quaternion = GetCameraRotationAtSplinePoint(rotation, curState.ReferenceUp, out isDefault);
		if (Damping.Enabled && deltaTime >= 0f && base.VirtualCamera.PreviousStateIsValid)
		{
			float t = base.VirtualCamera.DetachedFollowTargetDamp(1f, Damping.Angular, deltaTime);
			quaternion = Quaternion.Slerp(m_PreviousRotation, quaternion, t);
		}
		m_PreviousRotation = quaternion;
		curState.RawOrientation = quaternion;
		if (!isDefault)
		{
			curState.ReferenceUp = curState.RawOrientation * Vector3.up;
		}
	}

	private Quaternion GetCameraRotationAtSplinePoint(Quaternion splineOrientation, Vector3 up, out bool isDefault)
	{
		isDefault = false;
		switch (CameraRotation)
		{
		case RotationMode.Spline:
			return splineOrientation;
		case RotationMode.SplineNoRoll:
			return Quaternion.LookRotation(splineOrientation * Vector3.forward, up);
		case RotationMode.FollowTarget:
			if (base.FollowTarget != null)
			{
				return base.FollowTargetRotation;
			}
			break;
		case RotationMode.FollowTargetNoRoll:
			if (base.FollowTarget != null)
			{
				return Quaternion.LookRotation(base.FollowTargetRotation * Vector3.forward, up);
			}
			break;
		}
		isDefault = true;
		return Quaternion.LookRotation(base.VirtualCamera.transform.rotation * Vector3.forward, up);
	}
}
