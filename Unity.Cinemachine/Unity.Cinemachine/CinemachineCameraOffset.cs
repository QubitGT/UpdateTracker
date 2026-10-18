using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Camera Offset")]
[ExecuteAlways]
[SaveDuringPlay]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineCameraOffset.html")]
public class CinemachineCameraOffset : CinemachineExtension
{
	[Tooltip("Offset the camera's position by this much (camera space)")]
	[FormerlySerializedAs("m_Offset")]
	public Vector3 Offset = Vector3.zero;

	[Tooltip("When to apply the offset")]
	[FormerlySerializedAs("m_ApplyAfter")]
	public CinemachineCore.Stage ApplyAfter = CinemachineCore.Stage.Aim;

	[Tooltip("If applying offset after aim, re-adjust the aim to preserve the screen position of the LookAt target as much as possible")]
	[FormerlySerializedAs("m_PreserveComposition")]
	public bool PreserveComposition;

	private void Reset()
	{
		Offset = Vector3.zero;
		ApplyAfter = CinemachineCore.Stage.Aim;
		PreserveComposition = false;
	}

	protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
	{
		if (stage == ApplyAfter)
		{
			bool num = PreserveComposition && state.HasLookAt() && stage > CinemachineCore.Stage.Body;
			Vector3 vector = Vector2.zero;
			if (num)
			{
				vector = state.RawOrientation.GetCameraRotationToTarget(state.ReferenceLookAt - state.GetCorrectedPosition(), state.ReferenceUp);
			}
			Vector3 vector2 = state.RawOrientation * Offset;
			state.PositionCorrection += vector2;
			if (!num)
			{
				state.ReferenceLookAt += vector2;
				return;
			}
			Quaternion orient = Quaternion.LookRotation(state.ReferenceLookAt - state.GetCorrectedPosition(), state.ReferenceUp);
			orient = orient.ApplyCameraRotation(-vector, state.ReferenceUp);
			state.RawOrientation = orient;
		}
	}
}
