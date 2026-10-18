using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-core-overview/#scriptss")]
public class OVRResetOrientation : MonoBehaviour
{
	public OVRInput.RawButton resetButton = OVRInput.RawButton.Y;

	private void Update()
	{
		if (OVRInput.GetDown(resetButton))
		{
			OVRManager.display.RecenterPose();
		}
	}
}
