using UnityEngine;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-core-overview/#scripts")]
public class OVRModeParms : MonoBehaviour
{
	public OVRInput.RawButton resetButton = OVRInput.RawButton.X;

	private void Start()
	{
		if (!OVRManager.isHmdPresent)
		{
			base.enabled = false;
		}
		else
		{
			InvokeRepeating("TestPowerStateMode", 10f, 10f);
		}
	}

	private void Update()
	{
		if (OVRInput.GetDown(resetButton))
		{
			OVRPlugin.suggestedCpuPerfLevel = OVRPlugin.ProcessorPerformanceLevel.PowerSavings;
			OVRPlugin.suggestedGpuPerfLevel = OVRPlugin.ProcessorPerformanceLevel.SustainedLow;
		}
	}

	private void TestPowerStateMode()
	{
		if (OVRPlugin.powerSaving)
		{
			Debug.Log("POWER SAVE MODE ACTIVATED");
		}
	}
}
