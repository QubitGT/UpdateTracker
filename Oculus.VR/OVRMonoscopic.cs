using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-core-overview/#scriptss")]
public class OVRMonoscopic : MonoBehaviour
{
	public OVRInput.RawButton toggleButton = OVRInput.RawButton.B;

	private bool monoscopic;

	private void Update()
	{
		if (OVRInput.GetDown(toggleButton))
		{
			monoscopic = !monoscopic;
			OVRManager.instance.monoscopic = monoscopic;
		}
	}
}
