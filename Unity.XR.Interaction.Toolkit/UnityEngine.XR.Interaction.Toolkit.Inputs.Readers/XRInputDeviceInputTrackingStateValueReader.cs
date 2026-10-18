namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceInputTrackingStateValueReader.html")]
[CreateAssetMenu(fileName = "XRInputDeviceInputTrackingStateValueReader", menuName = "XR/Input Value Reader/InputTrackingState")]
public class XRInputDeviceInputTrackingStateValueReader : XRInputDeviceValueReader<InputTrackingState>
{
	public override InputTrackingState ReadValue()
	{
		return ReadInputTrackingStateValue();
	}

	public override bool TryReadValue(out InputTrackingState value)
	{
		return TryReadInputTrackingStateValue(out value);
	}
}
