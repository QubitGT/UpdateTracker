using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Rotation Control/Cinemachine Rotate With Follow Target")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Aim)]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineRotateWithFollowTarget.html")]
public class CinemachineRotateWithFollowTarget : CinemachineComponentBase
{
	[Tooltip("How much time it takes for the aim to catch up to the target's rotation")]
	public float Damping;

	private Quaternion m_PreviousReferenceOrientation = Quaternion.identity;

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

	public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Aim;

	public override float GetMaxDampTime()
	{
		return Damping;
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		if (IsValid)
		{
			Quaternion quaternion = base.FollowTargetRotation;
			if (deltaTime >= 0f)
			{
				float t = base.VirtualCamera.DetachedFollowTargetDamp(1f, Damping, deltaTime);
				quaternion = Quaternion.Slerp(m_PreviousReferenceOrientation, base.FollowTargetRotation, t);
			}
			m_PreviousReferenceOrientation = quaternion;
			curState.RawOrientation = quaternion;
			curState.ReferenceUp = quaternion * Vector3.up;
		}
	}
}
