using System.Collections.Generic;
using Oculus.Interaction.Input;

namespace Oculus.Interaction.PoseDetection;

public class JointDeltaConfig
{
	public readonly int InstanceID;

	public readonly IEnumerable<HandJointId> JointIDs;

	public JointDeltaConfig(int instanceID, IEnumerable<HandJointId> jointIDs)
	{
		InstanceID = instanceID;
		JointIDs = jointIDs;
	}
}
