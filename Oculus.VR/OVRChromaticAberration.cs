using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-core-overview/#scripts")]
public class OVRChromaticAberration : MonoBehaviour
{
	public OVRInput.RawButton toggleButton = OVRInput.RawButton.X;

	private bool chromatic;

	private void Start()
	{
		OVRManager.instance.chromatic = chromatic;
	}

	private void Update()
	{
		if (OVRInput.GetDown(toggleButton))
		{
			chromatic = !chromatic;
			OVRManager.instance.chromatic = chromatic;
		}
	}
}
