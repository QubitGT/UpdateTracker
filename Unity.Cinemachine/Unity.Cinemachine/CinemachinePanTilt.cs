using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Rotation Control/Cinemachine Pan Tilt")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Aim)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachinePanTilt.html")]
public class CinemachinePanTilt : CinemachineComponentBase, IInputAxisOwner, IInputAxisResetSource, CinemachineFreeLookModifier.IModifierValueSource
{
	public enum ReferenceFrames
	{
		ParentObject,
		World,
		TrackingTarget,
		LookAtTarget
	}

	public enum RecenterTargetModes
	{
		AxisCenter,
		TrackingTargetForward,
		LookAtTargetForward
	}

	public ReferenceFrames ReferenceFrame;

	public RecenterTargetModes RecenterTarget;

	[Tooltip("Axis representing the current horizontal rotation.  Value is in degrees and represents a rotation about the Y axis.")]
	public InputAxis PanAxis = DefaultPan;

	[Tooltip("Axis representing the current vertical rotation.  Value is in degrees and represents a rotation about the X axis.")]
	public InputAxis TiltAxis = DefaultTilt;

	private Quaternion m_PreviousCameraRotation;

	private Action m_ResetHandler;

	private static InputAxis DefaultPan
	{
		get
		{
			InputAxis result = default(InputAxis);
			result.Value = 0f;
			result.Range = new Vector2(-180f, 180f);
			result.Wrap = true;
			result.Center = 0f;
			result.Recentering = InputAxis.RecenteringSettings.Default;
			return result;
		}
	}

	private static InputAxis DefaultTilt
	{
		get
		{
			InputAxis result = default(InputAxis);
			result.Value = 0f;
			result.Range = new Vector2(-70f, 70f);
			result.Wrap = false;
			result.Center = 0f;
			result.Recentering = InputAxis.RecenteringSettings.Default;
			return result;
		}
	}

	float CinemachineFreeLookModifier.IModifierValueSource.NormalizedModifierValue
	{
		get
		{
			float num = TiltAxis.Range.y - TiltAxis.Range.x;
			return (TiltAxis.Value - TiltAxis.Range.x) / ((num > 0.001f) ? num : 1f) * 2f - 1f;
		}
	}

	bool IInputAxisResetSource.HasResetHandler => m_ResetHandler != null;

	public override bool IsValid => base.enabled;

	public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Aim;

	private void OnValidate()
	{
		PanAxis.Validate();
		TiltAxis.Range.x = Mathf.Clamp(TiltAxis.Range.x, -90f, 90f);
		TiltAxis.Range.y = Mathf.Clamp(TiltAxis.Range.y, -90f, 90f);
		TiltAxis.Validate();
	}

	private void Reset()
	{
		PanAxis = DefaultPan;
		TiltAxis = DefaultTilt;
		ReferenceFrame = ReferenceFrames.ParentObject;
		RecenterTarget = RecenterTargetModes.AxisCenter;
	}

	void IInputAxisOwner.GetInputAxes(List<IInputAxisOwner.AxisDescriptor> axes)
	{
		axes.Add(new IInputAxisOwner.AxisDescriptor
		{
			DrivenAxis = () => ref PanAxis,
			Name = "Look X (Pan)",
			Hint = IInputAxisOwner.AxisDescriptor.Hints.X
		});
		axes.Add(new IInputAxisOwner.AxisDescriptor
		{
			DrivenAxis = () => ref TiltAxis,
			Name = "Look Y (Tilt)",
			Hint = IInputAxisOwner.AxisDescriptor.Hints.Y
		});
	}

	void IInputAxisResetSource.RegisterResetHandler(Action handler)
	{
		m_ResetHandler = (Action)Delegate.Combine(m_ResetHandler, handler);
	}

	void IInputAxisResetSource.UnregisterResetHandler(Action handler)
	{
		m_ResetHandler = (Action)Delegate.Remove(m_ResetHandler, handler);
	}

	public override void PrePipelineMutateCameraState(ref CameraState state, float deltaTime)
	{
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		if (IsValid)
		{
			if (deltaTime < 0f || !base.VirtualCamera.PreviousStateIsValid || !CinemachineCore.IsLive(base.VirtualCamera))
			{
				m_ResetHandler?.Invoke();
			}
			Quaternion quaternion = (curState.RawOrientation = GetReferenceFrame(curState.ReferenceUp) * Quaternion.Euler(TiltAxis.Value, PanAxis.Value, 0f));
			if (base.VirtualCamera.PreviousStateIsValid)
			{
				curState.RotationDampingBypass *= UnityVectorExtensions.SafeFromToRotation(m_PreviousCameraRotation * Vector3.forward, quaternion * Vector3.forward, curState.ReferenceUp);
			}
			m_PreviousCameraRotation = quaternion;
			bool flag = PanAxis.TrackValueChange();
			bool flag2 = TiltAxis.TrackValueChange();
			if (PanAxis.Recentering.Time == TiltAxis.Recentering.Time)
			{
				flag = flag || flag2;
				flag2 = flag2 || flag;
			}
			Vector2 recenterTarget = GetRecenterTarget();
			PanAxis.UpdateRecentering(deltaTime, flag, recenterTarget.x);
			TiltAxis.UpdateRecentering(deltaTime, flag2, recenterTarget.y);
		}
	}

	public override void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
		SetAxesForRotation(rot);
	}

	public override bool OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		m_ResetHandler?.Invoke();
		if (fromCam != null && (base.VirtualCamera.State.BlendHint & CameraState.BlendHints.InheritPosition) != 0 && !CinemachineCore.IsLiveInBlend(base.VirtualCamera))
		{
			SetAxesForRotation(fromCam.State.RawOrientation);
			return true;
		}
		return false;
	}

	private void SetAxesForRotation(Quaternion targetRot)
	{
		m_ResetHandler?.Invoke();
		Vector3 referenceUp = base.VcamState.ReferenceUp;
		Vector3 vector = GetReferenceFrame(referenceUp) * Vector3.forward;
		PanAxis.Value = 0f;
		Vector3 vector2 = targetRot * Vector3.forward;
		Vector3 vector3 = vector.ProjectOntoPlane(referenceUp);
		Vector3 vector4 = vector2.ProjectOntoPlane(referenceUp);
		if (!vector3.AlmostZero() && !vector4.AlmostZero())
		{
			PanAxis.Value = Vector3.SignedAngle(vector3, vector4, referenceUp);
		}
		TiltAxis.Value = 0f;
		vector = Quaternion.AngleAxis(PanAxis.Value, referenceUp) * vector;
		Vector3 vector5 = Vector3.Cross(referenceUp, vector);
		if (!vector5.AlmostZero())
		{
			TiltAxis.Value = Vector3.SignedAngle(vector, vector2, vector5);
		}
	}

	private Quaternion GetReferenceFrame(Vector3 up)
	{
		Transform transform = null;
		switch (ReferenceFrame)
		{
		case ReferenceFrames.TrackingTarget:
			transform = base.FollowTarget;
			break;
		case ReferenceFrames.LookAtTarget:
			transform = base.LookAtTarget;
			break;
		case ReferenceFrames.ParentObject:
			transform = base.VirtualCamera.transform.parent;
			break;
		}
		if (!(transform != null))
		{
			return Quaternion.FromToRotation(Vector3.up, up);
		}
		return transform.rotation;
	}

	public Vector2 GetRecenterTarget()
	{
		Transform transform = null;
		switch (RecenterTarget)
		{
		case RecenterTargetModes.TrackingTargetForward:
			transform = base.VirtualCamera.Follow;
			break;
		case RecenterTargetModes.LookAtTargetForward:
			transform = base.VirtualCamera.LookAt;
			break;
		}
		if (transform != null)
		{
			Vector3 vector = transform.forward;
			Transform parent = base.VirtualCamera.transform.parent;
			if (parent != null)
			{
				vector = parent.rotation * vector;
			}
			Vector3 eulerAngles = Quaternion.FromToRotation(Vector3.forward, vector).eulerAngles;
			return new Vector2(NormalizeAngle(eulerAngles.y), NormalizeAngle(eulerAngles.x));
		}
		return new Vector2(PanAxis.Center, TiltAxis.Center);
		static float NormalizeAngle(float angle)
		{
			return (angle + 180f) % 360f - 180f;
		}
	}
}
