namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceBoolValueReader.html")]
[CreateAssetMenu(fileName = "XRInputDeviceBoolValueReader", menuName = "XR/Input Value Reader/bool")]
public class XRInputDeviceBoolValueReader : XRInputDeviceValueReader<bool>
{
	public override bool ReadValue()
	{
		return ReadBoolValue();
	}

	public override bool TryReadValue(out bool value)
	{
		return TryReadBoolValue(out value);
	}
}
