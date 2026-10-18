using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Rotation Control/Cinemachine Rotation Composer")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Aim)]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.LookAt)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineRotationComposer.html")]
public class CinemachineRotationComposer : CinemachineComponentBase, CinemachineFreeLookModifier.IModifiableComposition
{
	private struct FovCache
	{
		public Rect FovSoftGuideRect;

		public Rect FovHardGuideRect;

		public Vector2 Fov;

		private float m_OrthoSizeOverDistance;

		private float m_Aspect;

		private Rect m_DeadZoneRect;

		private Rect m_HardLimitRect;

		private Vector2 m_ScreenBounds;

		private Vector2 m_HalfFovRad;

		public void UpdateCache(ref LensSettings lens, Rect softGuide, Rect hardGuide, float targetDistance)
		{
			bool flag = m_Aspect != lens.Aspect || softGuide != m_DeadZoneRect || hardGuide != m_HardLimitRect || m_ScreenBounds == Vector2.zero;
			if (lens.Orthographic)
			{
				float num = Mathf.Abs(lens.OrthographicSize / targetDistance);
				if (m_OrthoSizeOverDistance == 0f || Mathf.Abs(num - m_OrthoSizeOverDistance) / m_OrthoSizeOverDistance > m_OrthoSizeOverDistance * 0.01f)
				{
					flag = true;
				}
				if (flag)
				{
					m_HalfFovRad = new Vector2(Mathf.Atan(lens.Aspect * num), Mathf.Atan(num));
					Fov = new Vector2(114.59156f * m_HalfFovRad.x, 114.59156f * m_HalfFovRad.y);
					m_OrthoSizeOverDistance = num;
				}
			}
			else
			{
				float fieldOfView = lens.FieldOfView;
				if (Fov.y != fieldOfView)
				{
					flag = true;
				}
				if (flag)
				{
					float num2 = fieldOfView * (MathF.PI / 180f) * 0.5f;
					m_HalfFovRad = new Vector2((float)Math.Atan(Math.Tan(num2) * (double)lens.Aspect), num2);
					Fov = new Vector2(57.29578f * m_HalfFovRad.x * 2f, fieldOfView);
					m_OrthoSizeOverDistance = 0f;
				}
			}
			if (flag)
			{
				m_Aspect = lens.Aspect;
				m_ScreenBounds = new Vector2(Mathf.Tan(m_HalfFovRad.x), Mathf.Tan(m_HalfFovRad.y));
				m_DeadZoneRect = softGuide;
				m_HardLimitRect = hardGuide;
				FovSoftGuideRect = new Rect
				{
					min = ScreenToAngle(softGuide.min),
					max = ScreenToAngle(softGuide.max)
				};
				FovHardGuideRect = new Rect
				{
					min = ScreenToAngle(hardGuide.min),
					max = ScreenToAngle(hardGuide.max)
				};
			}
		}

		private readonly Vector2 ScreenToAngle(Vector2 p)
		{
			return new Vector2((m_HalfFovRad.x + Mathf.Atan(2f * (p.x - 0.5f) * m_ScreenBounds.x)) / (2f * m_HalfFovRad.x), (m_HalfFovRad.y + Mathf.Atan(2f * (p.y - 0.5f) * m_ScreenBounds.y)) / (2f * m_HalfFovRad.y));
		}

		public readonly Vector3 DirectionFromScreen(Vector2 p)
		{
			return new Vector3(2f * (p.x - 0.5f) * m_ScreenBounds.x, -2f * (p.y - 0.5f) * m_ScreenBounds.y, 1f);
		}
	}

	[Header("Composition")]
	[HideFoldout]
	public ScreenComposerSettings Composition = ScreenComposerSettings.Default;

	[Tooltip("Force target to center of screen when this camera activates.  If false, will clamp target to the edges of the dead zone")]
	public bool CenterOnActivate = true;

	[Header("Target Tracking")]
	[Tooltip("Target offset from the target object's center in target-local space. Use this to fine-tune the tracking target position when the desired area is not the tracked object's center.")]
	[FormerlySerializedAs("TrackedObjectOffset")]
	public Vector3 TargetOffset;

	[Tooltip("How aggressively the camera tries to follow the target in the screen space. Small numbers are more responsive, rapidly orienting the camera to keep the target in the dead zone. Larger numbers give a more heavy slowly responding camera. Using different vertical and horizontal settings can yield a wide range of camera behaviors.")]
	public Vector2 Damping;

	[FoldoutWithEnabledButton("Enabled")]
	public LookaheadSettings Lookahead;

	private Vector3 m_CameraPosPrevFrame = Vector3.zero;

	private Vector3 m_LookAtPrevFrame = Vector3.zero;

	private Vector2 m_ScreenOffsetPrevFrame = Vector2.zero;

	private Quaternion m_CameraOrientationPrevFrame = Quaternion.identity;

	internal PositionPredictor m_Predictor;

	private ScreenComposerSettings m_CompositionLastFrame;

	private FovCache m_Cache;

	public override bool IsValid
	{
		get
		{
			if (base.enabled)
			{
				return base.LookAtTarget != null;
			}
			return false;
		}
	}

	public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Aim;

	internal override bool CameraLooksAtTarget => true;

	internal Vector3 TrackedPoint { get; private set; }

	internal ScreenComposerSettings GetEffectiveComposition => m_CompositionLastFrame;

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

	private void Reset()
	{
		TargetOffset = Vector3.zero;
		Lookahead = default(LookaheadSettings);
		Damping = new Vector2(0.5f, 0.5f);
		Composition = ScreenComposerSettings.Default;
		CenterOnActivate = true;
	}

	private void OnValidate()
	{
		Damping.x = Mathf.Max(0f, Damping.x);
		Damping.y = Mathf.Max(0f, Damping.y);
		Composition.Validate();
	}

	private Vector3 GetLookAtPointAndSetTrackedPoint(Vector3 lookAt, Vector3 up, float deltaTime)
	{
		Vector3 vector = lookAt;
		if (base.LookAtTarget != null)
		{
			vector += base.LookAtTargetRotation * TargetOffset;
		}
		if (!Lookahead.Enabled || Lookahead.Time < 0.0001f)
		{
			TrackedPoint = vector;
		}
		else
		{
			bool flag = base.VirtualCamera.LookAtTargetChanged || !base.VirtualCamera.PreviousStateIsValid;
			m_Predictor.Smoothing = Lookahead.Smoothing;
			m_Predictor.AddPosition(vector, flag ? (-1f) : deltaTime);
			Vector3 vector2 = m_Predictor.PredictPositionDelta(Lookahead.Time);
			if (Lookahead.IgnoreY)
			{
				vector2 = vector2.ProjectOntoPlane(up);
			}
			TrackedPoint = vector + vector2;
		}
		return TrackedPoint;
	}

	public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		base.OnTargetObjectWarped(target, positionDelta);
		if (target == base.LookAtTarget)
		{
			m_CameraPosPrevFrame += positionDelta;
			m_LookAtPrevFrame += positionDelta;
			m_Predictor.ApplyTransformDelta(positionDelta);
		}
	}

	public override void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
		base.ForceCameraPosition(pos, rot);
		m_Predictor.ApplyRotationDelta(rot * Quaternion.Inverse(m_CameraOrientationPrevFrame));
		m_CameraPosPrevFrame = pos;
		m_CameraOrientationPrevFrame = rot;
	}

	public override float GetMaxDampTime()
	{
		return Mathf.Max(Damping.x, Damping.y);
	}

	public override void PrePipelineMutateCameraState(ref CameraState curState, float deltaTime)
	{
		if (IsValid && curState.HasLookAt())
		{
			curState.ReferenceLookAt = GetLookAtPointAndSetTrackedPoint(curState.ReferenceLookAt, curState.ReferenceUp, deltaTime);
		}
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		if (!IsValid || !curState.HasLookAt())
		{
			return;
		}
		if (!(TrackedPoint - curState.ReferenceLookAt).AlmostZero())
		{
			Vector3 vector = Vector3.Lerp(curState.GetCorrectedPosition(), curState.ReferenceLookAt, 0.5f);
			Vector3 lhs = curState.ReferenceLookAt - vector;
			Vector3 rhs = TrackedPoint - vector;
			if (Vector3.Dot(lhs, rhs) < 0f)
			{
				float t = Vector3.Distance(curState.ReferenceLookAt, vector) / Vector3.Distance(curState.ReferenceLookAt, TrackedPoint);
				TrackedPoint = Vector3.Lerp(curState.ReferenceLookAt, TrackedPoint, t);
			}
		}
		float magnitude = (TrackedPoint - curState.GetCorrectedPosition()).magnitude;
		if (magnitude < 0.0001f)
		{
			if (deltaTime >= 0f && base.VirtualCamera.PreviousStateIsValid)
			{
				curState.RawOrientation = m_CameraOrientationPrevFrame;
			}
			return;
		}
		m_Cache.UpdateCache(ref curState.Lens, Composition.DeadZoneRect, Composition.HardLimitsRect, magnitude);
		Quaternion rawOrientation = curState.RawOrientation;
		if (deltaTime < 0f || !base.VirtualCamera.PreviousStateIsValid)
		{
			rawOrientation = Quaternion.LookRotation(rawOrientation * Vector3.forward, curState.ReferenceUp);
			Rect screenRect = m_Cache.FovSoftGuideRect;
			if (CenterOnActivate)
			{
				screenRect = new Rect(screenRect.center, Vector2.zero);
			}
			RotateToScreenBounds(ref curState, screenRect, curState.ReferenceLookAt, ref rawOrientation, m_Cache.Fov, -1f);
		}
		else
		{
			Vector3 vector2 = m_LookAtPrevFrame - m_CameraPosPrevFrame;
			if (vector2.AlmostZero())
			{
				rawOrientation = Quaternion.LookRotation(m_CameraOrientationPrevFrame * Vector3.forward, curState.ReferenceUp);
			}
			else
			{
				vector2 = curState.RotationDampingBypass * vector2;
				if (Composition.ScreenPosition != m_CompositionLastFrame.ScreenPosition)
				{
					Vector3 v = m_Cache.DirectionFromScreen(m_CompositionLastFrame.ScreenPosition);
					Vector3 v2 = m_Cache.DirectionFromScreen(Composition.ScreenPosition);
					Quaternion quaternion = Quaternion.identity.ApplyCameraRotation(m_ScreenOffsetPrevFrame, Vector3.up);
					quaternion *= UnityVectorExtensions.SafeFromToRotation(v, v2, Vector3.up);
					m_ScreenOffsetPrevFrame = Quaternion.identity.GetCameraRotationToTarget(quaternion * Vector3.forward, Vector3.up);
				}
				rawOrientation = Quaternion.LookRotation(vector2, curState.ReferenceUp);
				rawOrientation = rawOrientation.ApplyCameraRotation(-m_ScreenOffsetPrevFrame, curState.ReferenceUp);
			}
			RotateToScreenBounds(ref curState, m_Cache.FovSoftGuideRect, TrackedPoint, ref rawOrientation, m_Cache.Fov, deltaTime);
			if (Composition.HardLimits.Enabled && (deltaTime < 0f || base.VirtualCamera.LookAtTargetAttachment > 0.9999f))
			{
				RotateToScreenBounds(ref curState, m_Cache.FovHardGuideRect, curState.ReferenceLookAt, ref rawOrientation, m_Cache.Fov, -1f);
			}
		}
		m_CameraPosPrevFrame = curState.GetCorrectedPosition();
		m_LookAtPrevFrame = TrackedPoint;
		m_CameraOrientationPrevFrame = rawOrientation.normalized;
		m_ScreenOffsetPrevFrame = m_CameraOrientationPrevFrame.GetCameraRotationToTarget(m_LookAtPrevFrame - m_CameraPosPrevFrame, curState.ReferenceUp);
		m_CompositionLastFrame = Composition;
		curState.RawOrientation = m_CameraOrientationPrevFrame;
	}

	private void RotateToScreenBounds(ref CameraState state, Rect screenRect, Vector3 trackedPoint, ref Quaternion rigOrientation, Vector2 fov, float deltaTime)
	{
		Vector3 vector = trackedPoint - state.GetCorrectedPosition();
		Vector2 cameraRotationToTarget = rigOrientation.GetCameraRotationToTarget(vector, state.ReferenceUp);
		ClampVerticalBounds(ref screenRect, vector, state.ReferenceUp, fov.y);
		float num = (screenRect.yMin - 0.5f) * fov.y;
		float num2 = (screenRect.yMax - 0.5f) * fov.y;
		if (cameraRotationToTarget.x < num)
		{
			cameraRotationToTarget.x -= num;
		}
		else if (cameraRotationToTarget.x > num2)
		{
			cameraRotationToTarget.x -= num2;
		}
		else
		{
			cameraRotationToTarget.x = 0f;
		}
		num = (screenRect.xMin - 0.5f) * fov.x;
		num2 = (screenRect.xMax - 0.5f) * fov.x;
		if (cameraRotationToTarget.y < num)
		{
			cameraRotationToTarget.y -= num;
		}
		else if (cameraRotationToTarget.y > num2)
		{
			cameraRotationToTarget.y -= num2;
		}
		else
		{
			cameraRotationToTarget.y = 0f;
		}
		if (deltaTime >= 0f && base.VirtualCamera.PreviousStateIsValid)
		{
			cameraRotationToTarget.x = base.VirtualCamera.DetachedLookAtTargetDamp(cameraRotationToTarget.x, Damping.y, deltaTime);
			cameraRotationToTarget.y = base.VirtualCamera.DetachedLookAtTargetDamp(cameraRotationToTarget.y, Damping.x, deltaTime);
		}
		rigOrientation = rigOrientation.ApplyCameraRotation(cameraRotationToTarget, state.ReferenceUp);
	}

	private static bool ClampVerticalBounds(ref Rect r, Vector3 dir, Vector3 up, float fov)
	{
		float num = UnityVectorExtensions.Angle(dir, up);
		float num2 = fov / 2f + 1f;
		if (num < num2)
		{
			float num3 = 1f - (num2 - num) / fov;
			if (r.yMax > num3)
			{
				r.yMin = Mathf.Min(r.yMin, num3);
				r.yMax = Mathf.Min(r.yMax, num3);
				return true;
			}
		}
		if (num > 180f - num2)
		{
			float num4 = (num - (180f - num2)) / fov;
			if (num4 > r.yMin)
			{
				r.yMin = Mathf.Max(r.yMin, num4);
				r.yMax = Mathf.Max(r.yMax, num4);
				return true;
			}
		}
		return false;
	}
}
