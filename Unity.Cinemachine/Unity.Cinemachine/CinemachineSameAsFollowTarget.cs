using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[Obsolete("CinemachineSameAsFollowTarget has been deprecated. Use CinemachineRotateWithFollowTarget instead")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Aim)]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineSameAsFollowTarget.html")]
public class CinemachineSameAsFollowTarget : CinemachineComponentBase
{
	[Tooltip("How much time it takes for the aim to catch up to the target's rotation")]
	[FormerlySerializedAs("m_AngularDamping")]
	[FormerlySerializedAs("m_Damping")]
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

	internal void UpgradeToCm3(CinemachineRotateWithFollowTarget c)
	{
		c.Damping = Damping;
	}
}
