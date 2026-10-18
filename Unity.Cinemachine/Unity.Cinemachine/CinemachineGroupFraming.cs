using System;
using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Group Framing")]
[ExecuteAlways]
[SaveDuringPlay]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.GroupLookAt)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineGroupFraming.html")]
public class CinemachineGroupFraming : CinemachineExtension
{
	public enum FramingModes
	{
		Horizontal,
		Vertical,
		HorizontalAndVertical
	}

	public enum SizeAdjustmentModes
	{
		ZoomOnly,
		DollyOnly,
		DollyThenZoom
	}

	public enum LateralAdjustmentModes
	{
		ChangePosition,
		ChangeRotation
	}

	private class VcamExtraState : VcamExtraStateBase
	{
		public Vector3 PosAdjustment;

		public Vector2 RotAdjustment;

		public float FovAdjustment;

		public CinemachineCore.Stage Stage = CinemachineCore.Stage.Finalize;

		public CinemachineConfiner2D Confiner;

		public float PreviousOrthoSize;

		public void Reset()
		{
			PosAdjustment = Vector3.zero;
			RotAdjustment = Vector2.zero;
			FovAdjustment = 0f;
			Stage = CinemachineCore.Stage.Finalize;
		}
	}

	[Tooltip("What screen dimensions to consider when framing.  Can be Horizontal, Vertical, or both")]
	public FramingModes FramingMode = FramingModes.HorizontalAndVertical;

	[Tooltip("The bounding box of the targets should occupy this amount of the screen space.  1 means fill the whole screen.  0.5 means fill half the screen, etc.")]
	[Range(0f, 2f)]
	public float FramingSize = 0.8f;

	[Tooltip("A nonzero value will offset the group in the camera frame.")]
	public Vector2 CenterOffset = Vector2.zero;

	[Range(0f, 20f)]
	[Tooltip("How aggressively the camera tries to frame the group. Small numbers are more responsive, rapidly adjusting the camera to keep the group in the frame.  Larger numbers give a heavier more slowly responding camera.")]
	public float Damping = 2f;

	[Tooltip("How to adjust the camera to get the desired framing size.  You can zoom, dolly in/out, or do both.")]
	public SizeAdjustmentModes SizeAdjustment = SizeAdjustmentModes.DollyThenZoom;

	[Tooltip("How to adjust the camera to get the desired horizontal and vertical framing.")]
	public LateralAdjustmentModes LateralAdjustment;

	[Tooltip("Allowable FOV range, if adjusting FOV.")]
	[MinMaxRangeSlider(1f, 179f)]
	public Vector2 FovRange = new Vector2(1f, 100f);

	[Tooltip("Allowable range for the camera to move.  0 is the undollied position.  Negative values move the camera closer to the target.")]
	[Vector2AsRange]
	public Vector2 DollyRange = new Vector2(-100f, 100f);

	[Tooltip("Allowable orthographic size range, if adjusting orthographic size.")]
	[Vector2AsRange]
	public Vector2 OrthoSizeRange = new Vector2(1f, 1000f);

	private const float k_MinimumGroupSize = 0.01f;

	internal Bounds GroupBounds;

	internal Matrix4x4 GroupBoundsMatrix;

	private void OnValidate()
	{
		FramingSize = Mathf.Max(0.01f, FramingSize);
		Damping = Mathf.Max(0f, Damping);
		DollyRange.y = Mathf.Max(DollyRange.x, DollyRange.y);
		FovRange.y = Mathf.Clamp(FovRange.y, 1f, 179f);
		FovRange.x = Mathf.Clamp(FovRange.x, 1f, FovRange.y);
		OrthoSizeRange.x = Mathf.Max(0.01f, OrthoSizeRange.x);
		OrthoSizeRange.y = Mathf.Max(OrthoSizeRange.x, OrthoSizeRange.y);
	}

	private void Reset()
	{
		FramingMode = FramingModes.HorizontalAndVertical;
		SizeAdjustment = SizeAdjustmentModes.DollyThenZoom;
		LateralAdjustment = LateralAdjustmentModes.ChangePosition;
		FramingSize = 0.8f;
		CenterOffset = Vector2.zero;
		Damping = 2f;
		DollyRange = new Vector2(-100f, 100f);
		FovRange = new Vector2(1f, 100f);
		OrthoSizeRange = new Vector2(1f, 1000f);
	}

	public override float GetMaxDampTime()
	{
		return Damping;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		VcamExtraState extraState = GetExtraState<VcamExtraState>(vcam);
		if (!vcam.PreviousStateIsValid)
		{
			extraState.Reset();
		}
		if (extraState.Stage == CinemachineCore.Stage.Finalize || !Application.isPlaying)
		{
			if (vcam.TryGetComponent<CinemachineConfiner2D>(out extraState.Confiner))
			{
				extraState.Stage = CinemachineCore.Stage.Body;
			}
			else
			{
				extraState.Stage = CinemachineCore.Stage.Aim;
				if (vcam is CinemachineCamera cinemachineCamera)
				{
					CinemachineComponentBase cinemachineComponent = cinemachineCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
					if (cinemachineComponent != null && cinemachineComponent.BodyAppliesAfterAim)
					{
						extraState.Stage = CinemachineCore.Stage.Body;
					}
				}
			}
		}
		if (stage != extraState.Stage)
		{
			return;
		}
		ICinemachineTargetGroup cinemachineTargetGroup = vcam.LookAtTargetAsGroup;
		if (cinemachineTargetGroup == null)
		{
			cinemachineTargetGroup = vcam.FollowTargetAsGroup;
		}
		if (cinemachineTargetGroup != null && cinemachineTargetGroup.IsValid)
		{
			if (state.Lens.Orthographic)
			{
				OrthoFraming(vcam, cinemachineTargetGroup, extraState, ref state, deltaTime);
			}
			else
			{
				PerspectiveFraming(vcam, cinemachineTargetGroup, extraState, ref state, deltaTime);
			}
			if (extraState.Confiner != null && Mathf.Abs(extraState.PreviousOrthoSize - state.Lens.OrthographicSize) > 0.0001f)
			{
				extraState.Confiner.InvalidateLensCache();
				extraState.PreviousOrthoSize = state.Lens.OrthographicSize;
			}
		}
	}

	private void OrthoFraming(CinemachineVirtualCameraBase vcam, ICinemachineTargetGroup group, VcamExtraState extra, ref CameraState state, float deltaTime)
	{
		float dampTime = ((vcam.PreviousStateIsValid && deltaTime >= 0f) ? Damping : 0f);
		Vector3 correctedPosition = state.GetCorrectedPosition();
		Quaternion correctedOrientation = state.GetCorrectedOrientation();
		GroupBoundsMatrix = Matrix4x4.TRS(correctedPosition, correctedOrientation, Vector3.one);
		GroupBounds = group.GetViewSpaceBoundingBox(GroupBoundsMatrix, includeBehind: true);
		Vector3 center = GroupBounds.center;
		center.z = Mathf.Min(0f, center.z - GroupBounds.extents.z);
		LensSettings lens = state.Lens;
		float num = Mathf.Clamp(GetFrameHeight(GroupBounds.size / FramingSize, lens.Aspect) * 0.5f, OrthoSizeRange.x, OrthoSizeRange.y) - lens.OrthographicSize;
		extra.FovAdjustment += vcam.DetachedFollowTargetDamp(num - extra.FovAdjustment, dampTime, deltaTime);
		lens.OrthographicSize += extra.FovAdjustment;
		center.x -= CenterOffset.x * lens.OrthographicSize / lens.Aspect;
		center.y -= CenterOffset.y * lens.OrthographicSize;
		extra.PosAdjustment += vcam.DetachedFollowTargetDamp(center - extra.PosAdjustment, dampTime, deltaTime);
		state.PositionCorrection += correctedOrientation * extra.PosAdjustment;
		state.Lens = lens;
	}

	private void PerspectiveFraming(CinemachineVirtualCameraBase vcam, ICinemachineTargetGroup group, VcamExtraState extra, ref CameraState state, float deltaTime)
	{
		float dampTime = ((vcam.PreviousStateIsValid && deltaTime >= 0f) ? Damping : 0f);
		Vector3 correctedPosition = state.GetCorrectedPosition();
		Quaternion correctedOrientation = state.GetCorrectedOrientation();
		Vector3 camPos = correctedPosition;
		Quaternion camRot = correctedOrientation;
		Vector3 vector = camRot * Vector3.up;
		float fov = state.Lens.FieldOfView;
		bool flag = SizeAdjustment != SizeAdjustmentModes.ZoomOnly;
		Vector2 vector2 = (flag ? DollyRange : Vector2.zero);
		Matrix4x4 observer = Matrix4x4.TRS(camPos, camRot, Vector3.one);
		Bounds viewSpaceBoundingBox = group.GetViewSpaceBoundingBox(observer, flag);
		bool flag2 = LateralAdjustment == LateralAdjustmentModes.ChangePosition;
		if (!flag2)
		{
			Vector3 vector3 = observer.MultiplyPoint3x4(viewSpaceBoundingBox.center) - camPos;
			if (!Vector3.Cross(vector3, vector).AlmostZero())
			{
				camRot = Quaternion.LookRotation(vector3, vector);
			}
		}
		float dollyAmount = Mathf.Clamp(Mathf.Min(0f, viewSpaceBoundingBox.center.z) - viewSpaceBoundingBox.extents.z - 5f, vector2.x, vector2.y);
		camPos += camRot * new Vector3(0f, 0f, dollyAmount);
		ComputeCameraViewGroupBounds(group, ref camPos, ref camRot, flag2);
		AdjustSize(group, state.Lens.Aspect, ref camPos, ref camRot, ref fov, ref dollyAmount);
		LensSettings lens = state.Lens;
		float num = fov - lens.FieldOfView;
		extra.FovAdjustment += vcam.DetachedFollowTargetDamp(num - extra.FovAdjustment, dampTime, deltaTime);
		lens.FieldOfView += extra.FovAdjustment;
		state.Lens = lens;
		Vector2 cameraRotationToTarget = correctedOrientation.GetCameraRotationToTarget(camRot * Vector3.forward, vector);
		extra.RotAdjustment.x += vcam.DetachedFollowTargetDamp(cameraRotationToTarget.x - extra.RotAdjustment.x, dampTime, deltaTime);
		extra.RotAdjustment.y += vcam.DetachedFollowTargetDamp(cameraRotationToTarget.y - extra.RotAdjustment.y, dampTime, deltaTime);
		state.OrientationCorrection *= Quaternion.identity.ApplyCameraRotation(extra.RotAdjustment, vector);
		correctedOrientation = state.GetCorrectedOrientation();
		Vector3 vector4 = Quaternion.Inverse(correctedOrientation) * (camPos - correctedPosition);
		extra.PosAdjustment += vcam.DetachedFollowTargetDamp(vector4 - extra.PosAdjustment, dampTime, deltaTime);
		state.PositionCorrection += correctedOrientation * extra.PosAdjustment;
		if (Mathf.Abs(CenterOffset.x) > 0.01f || Mathf.Abs(CenterOffset.y) > 0.01f)
		{
			float num2 = 0.5f * state.Lens.FieldOfView;
			if (flag2)
			{
				float num3 = GroupBounds.center.z - GroupBounds.extents.z;
				state.PositionCorrection -= correctedOrientation * new Vector3(CenterOffset.x * Mathf.Tan(num2 * (MathF.PI / 180f) * state.Lens.Aspect) * num3, CenterOffset.y * Mathf.Tan(num2 * (MathF.PI / 180f)) * num3, 0f);
			}
			else
			{
				Vector2 rot = new Vector2(CenterOffset.y * num2, CenterOffset.x * num2 / state.Lens.Aspect);
				state.OrientationCorrection *= Quaternion.identity.ApplyCameraRotation(rot, state.ReferenceUp);
			}
		}
	}

	private void AdjustSize(ICinemachineTargetGroup group, float aspect, ref Vector3 camPos, ref Quaternion camRot, ref float fov, ref float dollyAmount)
	{
		if (SizeAdjustment != 0)
		{
			float frameHeight = GetFrameHeight(GroupBounds.size / FramingSize, aspect);
			float num = GroupBounds.center.z - GroupBounds.extents.z;
			float num2 = frameHeight / (2f * Mathf.Tan(fov * (MathF.PI / 180f) / 2f));
			float num3 = num - num2;
			num3 = Mathf.Clamp(num3 + dollyAmount, DollyRange.x, DollyRange.y) - dollyAmount;
			dollyAmount += num3;
			camPos += camRot * new Vector3(0f, 0f, num3);
			ComputeCameraViewGroupBounds(group, ref camPos, ref camRot, moveCamera: true);
		}
		if (SizeAdjustment != SizeAdjustmentModes.DollyOnly)
		{
			float frameHeight2 = GetFrameHeight(GroupBounds.size / FramingSize, aspect);
			float num4 = GroupBounds.center.z - GroupBounds.extents.z;
			if (num4 > 0.0001f)
			{
				fov = 2f * Mathf.Atan(frameHeight2 / (2f * num4)) * 57.29578f;
			}
			fov = Mathf.Clamp(fov, FovRange.x, FovRange.y);
		}
	}

	private void ComputeCameraViewGroupBounds(ICinemachineTargetGroup group, ref Vector3 camPos, ref Quaternion camRot, bool moveCamera)
	{
		GroupBoundsMatrix = Matrix4x4.TRS(camPos, camRot, Vector3.one);
		if (moveCamera)
		{
			GroupBounds = group.GetViewSpaceBoundingBox(GroupBoundsMatrix, includeBehind: false);
			Vector3 center = GroupBounds.center;
			center.z = 0f;
			camPos = GroupBoundsMatrix.MultiplyPoint3x4(center);
			GroupBoundsMatrix = Matrix4x4.TRS(camPos, camRot, Vector3.one);
		}
		group.GetViewSpaceAngularBounds(GroupBoundsMatrix, out var minAngles, out var maxAngles, out var zRange);
		Vector2 vector = (minAngles + maxAngles) / 2f;
		Quaternion quaternion = Quaternion.identity.ApplyCameraRotation(vector, Vector3.up);
		if (moveCamera)
		{
			Vector3 vector2 = quaternion * Vector3.forward;
			new Plane(Vector3.forward, new Vector3(0f, 0f, zRange.x)).Raycast(new Ray(Vector3.zero, vector2), out var enter);
			camPos = vector2 * enter;
			camPos.z = 0f;
			camPos = GroupBoundsMatrix.MultiplyPoint3x4(camPos);
			GroupBoundsMatrix.SetColumn(3, camPos);
			group.GetViewSpaceAngularBounds(GroupBoundsMatrix, out minAngles, out maxAngles, out zRange);
		}
		else
		{
			camRot *= quaternion;
			GroupBoundsMatrix = Matrix4x4.TRS(camPos, camRot, Vector3.one);
			minAngles -= vector;
			maxAngles -= vector;
		}
		Vector2 vector3 = new Vector2(89.5f, 89.5f);
		if (zRange.x > 0f)
		{
			vector3 = Vector2.Max(maxAngles, minAngles.Abs());
			vector3 = Vector2.Min(vector3, new Vector2(89.5f, 89.5f));
		}
		float num = zRange.x * 2f;
		vector3 *= MathF.PI / 180f;
		GroupBounds = new Bounds(new Vector3(0f, 0f, (zRange.x + zRange.y) * 0.5f), new Vector3(Mathf.Tan(vector3.y) * num, Mathf.Tan(vector3.x) * num, zRange.y - zRange.x));
	}

	private float GetFrameHeight(Vector2 boundsSize, float aspect)
	{
		return Mathf.Max(FramingMode switch
		{
			FramingModes.Horizontal => Mathf.Max(0.0001f, boundsSize.x) / aspect, 
			FramingModes.Vertical => Mathf.Max(0.0001f, boundsSize.y), 
			_ => Mathf.Max(Mathf.Max(0.0001f, boundsSize.x) / aspect, Mathf.Max(0.0001f, boundsSize.y)), 
		}, 0.01f);
	}
}
