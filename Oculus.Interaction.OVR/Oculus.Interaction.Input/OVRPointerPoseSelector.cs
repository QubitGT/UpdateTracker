using UnityEngine;

namespace Oculus.Interaction.Input;

internal struct OVRPointerPoseSelector
{
	private static readonly Pose[] QUEST1_POINTERS = new Pose[2]
	{
		new Pose(new Vector3(-0.0078f, -0.0041f, 0.0375f), Quaternion.Euler(359.20953f, 6.4519606f, 6.955446f)),
		new Pose(new Vector3(0.0078f, -0.0041f, 0.0375f), Quaternion.Euler(359.20953f, 353.54803f, 353.04456f))
	};

	private static readonly Pose[] QUEST2_POINTERS = new Pose[2]
	{
		new Pose(new Vector3(0.009f, -0.0032102852f, 0.030869998f), Quaternion.Euler(359.20953f, 6.4519606f, 6.955446f)),
		new Pose(new Vector3(-0.009f, -0.0032102852f, 0.030869998f), Quaternion.Euler(359.20953f, 353.54803f, 353.04456f))
	};

	public Pose LocalPointerPose { get; private set; }

	public OVRPointerPoseSelector(Handedness handedness)
	{
		OVRPlugin.SystemHeadset systemHeadsetType = OVRPlugin.GetSystemHeadsetType();
		if (systemHeadsetType == OVRPlugin.SystemHeadset.Oculus_Quest_2 || systemHeadsetType == OVRPlugin.SystemHeadset.Oculus_Link_Quest_2)
		{
			LocalPointerPose = QUEST2_POINTERS[(int)handedness];
		}
		else
		{
			LocalPointerPose = QUEST1_POINTERS[(int)handedness];
		}
	}
}
