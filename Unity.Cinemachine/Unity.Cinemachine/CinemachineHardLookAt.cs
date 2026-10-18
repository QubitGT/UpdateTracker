using UnityEngine;

namespace Unity.Cinemachine;

[AddComponentMenu("Cinemachine/Procedural/Rotation Control/Cinemachine Hard Look At")]
[SaveDuringPlay]
[DisallowMultipleComponent]
[CameraPipeline(CinemachineCore.Stage.Aim)]
[RequiredTarget(RequiredTargetAttribute.RequiredTargets.LookAt)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineHardLookAt.html")]
public class CinemachineHardLookAt : CinemachineComponentBase
{
	[Tooltip("Offset from the LookAt target's origin, in target's local space.  The camera will look at this point.")]
	public Vector3 LookAtOffset = Vector3.zero;

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

	private void Reset()
	{
		LookAtOffset = Vector3.zero;
	}

	public override void MutateCameraState(ref CameraState curState, float deltaTime)
	{
		if (!IsValid || !curState.HasLookAt())
		{
			return;
		}
		Vector3 vector = base.LookAtTargetRotation * LookAtOffset;
		Vector3 vector2 = curState.ReferenceLookAt + vector - curState.GetCorrectedPosition();
		if (vector2.magnitude > 0.0001f)
		{
			if (Vector3.Cross(vector2.normalized, curState.ReferenceUp).magnitude < 0.0001f)
			{
				curState.RawOrientation = Quaternion.FromToRotation(Vector3.forward, vector2);
			}
			else
			{
				curState.RawOrientation = Quaternion.LookRotation(vector2, curState.ReferenceUp);
			}
		}
	}
}
