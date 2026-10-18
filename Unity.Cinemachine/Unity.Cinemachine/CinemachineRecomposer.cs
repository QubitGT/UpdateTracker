using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Recomposer")]
[ExecuteAlways]
[SaveDuringPlay]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineRecomposer.html")]
public class CinemachineRecomposer : CinemachineExtension
{
	[Tooltip("When to apply the adjustment")]
	[FormerlySerializedAs("m_ApplyAfter")]
	public CinemachineCore.Stage ApplyAfter;

	[Tooltip("Tilt the camera by this much")]
	[FormerlySerializedAs("m_Tilt")]
	public float Tilt;

	[Tooltip("Pan the camera by this much")]
	[FormerlySerializedAs("m_Pan")]
	public float Pan;

	[Tooltip("Roll the camera by this much")]
	[FormerlySerializedAs("m_Dutch")]
	public float Dutch;

	[Tooltip("Scale the zoom by this amount (normal = 1)")]
	[FormerlySerializedAs("m_ZoomScale")]
	public float ZoomScale;

	[Range(0f, 1f)]
	[Tooltip("Lowering this value relaxes the camera's attention to the Follow target (normal = 1)")]
	[FormerlySerializedAs("m_FollowAttachment")]
	public float FollowAttachment;

	[Range(0f, 1f)]
	[Tooltip("Lowering this value relaxes the camera's attention to the LookAt target (normal = 1)")]
	[FormerlySerializedAs("m_LookAtAttachment")]
	public float LookAtAttachment;

	private void Reset()
	{
		ApplyAfter = CinemachineCore.Stage.Finalize;
		Tilt = 0f;
		Pan = 0f;
		Dutch = 0f;
		ZoomScale = 1f;
		FollowAttachment = 1f;
		LookAtAttachment = 1f;
	}

	private void OnValidate()
	{
		ZoomScale = Mathf.Max(0.01f, ZoomScale);
		FollowAttachment = Mathf.Clamp01(FollowAttachment);
		LookAtAttachment = Mathf.Clamp01(LookAtAttachment);
	}

	public override void PrePipelineMutateCameraStateCallback(CinemachineVirtualCameraBase vcam, ref CameraState curState, float deltaTime)
	{
		vcam.FollowTargetAttachment = FollowAttachment;
		vcam.LookAtTargetAttachment = LookAtAttachment;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage == ApplyAfter)
		{
			LensSettings lens = state.Lens;
			Quaternion quaternion = state.RawOrientation * Quaternion.AngleAxis(Tilt, Vector3.right);
			Quaternion quaternion2 = Quaternion.AngleAxis(Pan, state.ReferenceUp) * quaternion;
			state.OrientationCorrection = Quaternion.Inverse(state.GetCorrectedOrientation()) * quaternion2;
			lens.Dutch += Dutch;
			if (ZoomScale != 1f)
			{
				lens.OrthographicSize *= ZoomScale;
				lens.FieldOfView *= ZoomScale;
			}
			state.Lens = lens;
		}
	}
}
