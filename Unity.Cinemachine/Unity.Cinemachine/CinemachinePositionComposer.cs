using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Position Control/Cinemachine Position Composer")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Body)]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachinePositionComposer.html")]
public class CinemachinePositionComposer : CinemachineComponentBase, CinemachineFreeLookModifier.IModifiablePositionDamping, CinemachineFreeLookModifier.IModifiableDistance, CinemachineFreeLookModifier.IModifiableComposition
{
	[Header("Camera Position")]
	[Tooltip("The distance along the camera axis that will be maintained from the target")]
	public float CameraDistance = 10f;

	[Tooltip("The camera will not move along its z-axis if the target is within this distance of the specified camera distance")]
	public float DeadZoneDepth;

	[Header("Composition")]
	[HideFoldout]
	public ScreenComposerSettings Composition = ScreenComposerSettings.Default;

	[Tooltip("Force target to center of screen when this camera activates.  If false, will clamp target to the edges of the dead zone")]
	public bool CenterOnActivate = true;

	[Header("Target Tracking")]
	[Tooltip("Offset from the target object (in target-local co-ordinates).  The camera will attempt to frame the point which is the target's position plus this offset.  Use it to correct for cases when the target's origin is not the point of interest for the camera.")]
	[FormerlySerializedAs("TrackedObjectOffset")]
	public Vector3 TargetOffset;

	[Tooltip("How aggressively the camera tries to follow the target in the screen space. Small numbers are more responsive, rapidly orienting the camera to keep the target in the dead zone. Larger numbers give a more heavy slowly responding camera. Using different vertical and horizontal settings can yield a wide range of camera behaviors.")]
	public Vector3 Damping;

	[FoldoutWithEnabledButton("Enabled")]
	public LookaheadSettings Lookahead;

	private const float kMinimumCameraDistance = 0.01f;

	internal PositionPredictor m_Predictor;

	private Vector3 m_PreviousCameraPosition = Vector3.zero;

	private Quaternion m_PreviousRotation;

	private ScreenComposerSettings m_PreviousComposition;

	private float m_PreviousDesiredDistance;

	private bool m_InheritingPosition;

	internal ScreenComposerSettings GetEffectiveComposition => m_PreviousComposition;

	ScreenComposerSettings CinemachineFreeLookModifier.IModifiableComposition.Composition
	{
		get
		{
			return Composition;
		}
		set
		{
			Composition = value;
		}
	}

	Vector3 CinemachineFreeLookModifier.IModifiablePositionDamping.PositionDamping
	{
		get
		{
			return Damping;
		}
		set
		{
			Damping = value;
		}
	}

	float CinemachineFreeLookModifier.IModifiableDistance.Distance
	{
		get
		{
			return CameraDistance;
		}
		set
		{
			CameraDistance = value;
		}
	}

	public override bool IsValid
	{
		get
		{
			if (base.enabled)
			{
				return base.FollowTarget != null;
			}
			return false;
		}
	}

	public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Body;

	public override bool BodyAppliesAfterAim => true;

	internal Vector3 TrackedPoint { get; private set; }

	private void Reset()
	{
		TargetOffset = Vector3.zero;
		Lookahead = default(LookaheadSettings);
		Damping = Vector3.one;
		CameraDistance = 10f;
		Composition = ScreenComposerSettings.Default;
		DeadZoneDepth = 0f;
		CenterOnActivate = true;
	}

	private void OnValidate()
	{
		Damping.x = Mathf.Max(0f, Damping.x);
		Damping.y = Mathf.Max(0f, Damping.y);
		Damping.z = Mathf.Max(0f, Damping.z);
		CameraDistance = Mathf.Max(0.01f, CameraDistance);
		DeadZoneDepth = Mathf.Max(0f, DeadZoneDepth);
		Composition.Validate();
	}

	public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		base.OnTargetObjectWarped(target, positionDelta);
		if (target == base.FollowTarget)
		{
			m_PreviousCameraPosition += positionDelta;
			m_Predictor.ApplyTransformDelta(positionDelta);
		}
	}

	public override void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
		base.ForceCameraPosition(pos, rot);
		m_Predictor.ApplyRotationDelta(rot * Quaternion.Inverse(m_PreviousRotation));
		m_PreviousCameraPosition = pos;
		m_PreviousRotation = rot;
	}

	public override float GetMaxDampTime()
	{
		return Mathf.Max(Damping.x, Mathf.Max(Damping.y, Damping.z));
	}

	public override bool OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		if (fromCam != null && (base.VirtualCamera.State.BlendHint & CameraState.BlendHints.InheritPosition) != 0 && !CinemachineCore.IsLiveInBlend(base.VirtualCamera))
		{
			m_PreviousCameraPosition = fromCam.State.RawPosition;
			m_PreviousRotation = fromCam.State.RawOrientation;
			m_InheritingPosition = true;
			return true;
		}
		return false;
	}

	private Rect ScreenToOrtho(Rect rScreen, float orthoSize, float aspect)
	{
		Rect result = default(Rect);
		result.yMax = 2f * orthoSize * (1f - rScreen.yMin - 0.5f);
		result.yMin = 2f * orthoSize * (1f - rScreen.yMax - 0.5f);
		result.xMin = 2f * orthoSize * aspect * (rScreen.xMin - 0.5f);
		result.xMax = 2f * orthoSize * aspect * (rScreen.xMax - 0.5f);
		return result;
	}

	private Vector3 OrthoOffsetToScreenBounds(Vector3 targetPos2D, Rect screenRect)
	{
		Vector3 zero = Vector3.zero;
		if (targetPos2D.x < screenRect.xMin)
		{
			zero.x += targetPos2D.x - screenRect.xMin;
		}
		if (targetPos2D.x > screenRect.xMax)
		{
			zero.x += targetPos2D.x - screenRect.xMax;
		}
		if (targetPos2D.y < screenRect.yMin)
		{
			zero.y += targetPos2D.y - screenRect.yMin;
		}
		if (targetPos2D.y > screenRect.yMax)
		{
			zero.y += targetPos2D.y - screenRect.yMax;
		}
		return zero;
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		LensSettings lens = curState.Lens;
		Vector3 vector = base.FollowTargetPosition + base.FollowTargetRotation * TargetOffset;
		bool flag = deltaTime >= 0f && base.VirtualCamera.PreviousStateIsValid;
		if (!flag || base.VirtualCamera.FollowTargetChanged)
		{
			m_Predictor.Reset();
		}
		if (!flag)
		{
			m_PreviousCameraPosition = curState.RawPosition;
			m_PreviousRotation = curState.RawOrientation;
			m_PreviousDesiredDistance = CameraDistance;
			m_PreviousComposition = Composition;
			if (!m_InheritingPosition && CenterOnActivate)
			{
				m_PreviousCameraPosition = base.FollowTargetPosition + curState.RawOrientation * Vector3.back * CameraDistance;
			}
		}
		if (!IsValid)
		{
			m_InheritingPosition = false;
			return;
		}
		float fieldOfView = lens.FieldOfView;
		TrackedPoint = vector;
		if (Lookahead.Enabled && Lookahead.Time > 0.0001f)
		{
			m_Predictor.Smoothing = Lookahead.Smoothing;
			m_Predictor.AddPosition(vector, deltaTime);
			Vector3 vector2 = m_Predictor.PredictPositionDelta(Lookahead.Time);
			if (Lookahead.IgnoreY)
			{
				vector2 = vector2.ProjectOntoPlane(curState.ReferenceUp);
			}
			TrackedPoint = vector + vector2;
		}
		if (!curState.HasLookAt() || curState.ReferenceLookAt == base.FollowTargetPosition)
		{
			curState.ReferenceLookAt = vector;
		}
		Quaternion rawOrientation = curState.RawOrientation;
		if (flag)
		{
			Vector3 vector3 = rawOrientation * Quaternion.Inverse(m_PreviousRotation) * (m_PreviousCameraPosition - TrackedPoint);
			m_PreviousCameraPosition = TrackedPoint + vector3;
			float num = CameraDistance - m_PreviousDesiredDistance;
			if (Mathf.Abs(num) > 0.0001f)
			{
				m_PreviousCameraPosition += vector3.normalized * num;
			}
		}
		m_PreviousRotation = rawOrientation;
		Vector3 previousCameraPosition = m_PreviousCameraPosition;
		Quaternion quaternion = Quaternion.Inverse(rawOrientation);
		Vector3 vector4 = quaternion * previousCameraPosition;
		Vector3 vector5 = quaternion * TrackedPoint - vector4;
		Vector3 vector6 = vector5;
		Vector3 vector7 = Vector3.zero;
		float num2 = Mathf.Max(0.01f, CameraDistance - DeadZoneDepth / 2f);
		float num3 = Mathf.Max(num2, CameraDistance + DeadZoneDepth / 2f);
		float num4 = Mathf.Min(vector5.z, vector6.z);
		if (num4 < num2)
		{
			vector7.z = num4 - num2;
		}
		if (num4 > num3)
		{
			vector7.z = num4 - num3;
		}
		float num5 = (lens.Orthographic ? lens.OrthographicSize : (Mathf.Tan(0.5f * fieldOfView * (MathF.PI / 180f)) * (num4 - vector7.z)));
		Rect rect = ScreenToOrtho(Composition.DeadZoneRect, num5, lens.Aspect);
		if (!flag)
		{
			Rect screenRect = rect;
			if (CenterOnActivate && !m_InheritingPosition)
			{
				screenRect = new Rect(screenRect.center, Vector2.zero);
			}
			vector7 += OrthoOffsetToScreenBounds(vector5, screenRect);
		}
		else
		{
			if (Composition.ScreenPosition != m_PreviousComposition.ScreenPosition)
			{
				Vector2 vector8 = Composition.ScreenPosition - m_PreviousComposition.ScreenPosition;
				Vector3 vector9 = new Vector3((0f - vector8.x) * num5 * lens.Aspect * 2f, vector8.y * num5 * 2f, 0f);
				vector5 += vector9;
				previousCameraPosition += rawOrientation * vector9;
			}
			vector7 += OrthoOffsetToScreenBounds(vector5, rect);
			vector7 = base.VirtualCamera.DetachedFollowTargetDamp(vector7, Damping, deltaTime);
			if (Composition.HardLimits.Enabled && (deltaTime < 0f || base.VirtualCamera.FollowTargetAttachment > 0.9999f))
			{
				Rect screenRect2 = ScreenToOrtho(Composition.HardLimitsRect, num5, lens.Aspect);
				Vector3 vector10 = quaternion * vector - vector4;
				vector7 += OrthoOffsetToScreenBounds(vector10 - vector7, screenRect2);
			}
		}
		curState.RawPosition = previousCameraPosition + rawOrientation * vector7;
		m_PreviousCameraPosition = curState.RawPosition;
		m_PreviousComposition = Composition;
		m_PreviousDesiredDistance = CameraDistance;
		m_InheritingPosition = false;
	}
}
