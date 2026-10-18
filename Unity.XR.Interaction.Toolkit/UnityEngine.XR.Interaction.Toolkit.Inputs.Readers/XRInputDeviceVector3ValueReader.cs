namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceVector3ValueReader.html")]
[CreateAssetMenu(fileName = "XRInputDeviceVector3ValueReader", menuName = "XR/Input Value Reader/Vector3")]
public class XRInputDeviceVector3ValueReader : XRInputDeviceValueReader<Vector3>
{
	public override Vector3 ReadValue()
	{
		return ReadVector3Value();
	}

	public override bool TryReadValue(out Vector3 value)
	{
		return TryReadVector3Value(out value);
	}
}
