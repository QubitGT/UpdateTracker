namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceQuaternionValueReader.html")]
[CreateAssetMenu(fileName = "XRInputDeviceQuaternionValueReader", menuName = "XR/Input Value Reader/Quaternion")]
public class XRInputDeviceQuaternionValueReader : XRInputDeviceValueReader<Quaternion>
{
	public override Quaternion ReadValue()
	{
		return ReadQuaternionValue();
	}

	public override bool TryReadValue(out Quaternion value)
	{
		return TryReadQuaternionValue(out value);
	}
}
