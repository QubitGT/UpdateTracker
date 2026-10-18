using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Position Control/Cinemachine Hard Lock to Target")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Body)]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.Tracking)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineHardLockToTarget.html")]
public class CinemachineHardLockToTarget : CinemachineComponentBase
{
	[Tooltip("How much time it takes for the position to catch up to the target's position")]
	[FormerlySerializedAs("m_Damping")]
	public float Damping;

	private Vector3 m_PreviousTargetPosition;

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

	public override float GetMaxDampTime()
	{
		return Damping;
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		if (IsValid)
		{
			Vector3 vector = base.FollowTargetPosition;
			if (base.VirtualCamera.PreviousStateIsValid && deltaTime >= 0f)
			{
				vector = m_PreviousTargetPosition + base.VirtualCamera.DetachedFollowTargetDamp(vector - m_PreviousTargetPosition, Damping, deltaTime);
			}
			m_PreviousTargetPosition = vector;
			curState.RawPosition = vector;
		}
	}
}
