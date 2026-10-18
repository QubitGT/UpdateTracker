using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public interface IJointDeltaProvider
{
	bool GetPositionDelta(HandJointId joint, out Vector3 delta);

	bool GetRotationDelta(HandJointId joint, out Quaternion delta);

	void RegisterConfig(JointDeltaConfig config);

	void UnRegisterConfig(JointDeltaConfig config);
}
