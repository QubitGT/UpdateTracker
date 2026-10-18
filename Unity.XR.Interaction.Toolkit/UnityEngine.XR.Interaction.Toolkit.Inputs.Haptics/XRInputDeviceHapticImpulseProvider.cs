namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.XRInputDeviceHapticImpulseProvider.html")]
[CreateAssetMenu(fileName = "XRInputDeviceHapticImpulseProvider", menuName = "XR/Input Device Haptic Impulse Provider")]
public class XRInputDeviceHapticImpulseProvider : ScriptableObject, IXRHapticImpulseProvider
{
	[SerializeField]
	private InputDeviceCharacteristics m_Characteristics;

	private XRInputDeviceHapticImpulseChannelGroup m_ChannelGroup;

	private InputDevice m_InputDevice;

	public IXRHapticImpulseChannelGroup GetChannelGroup()
	{
		RefreshInputDeviceIfNeeded();
		if (m_ChannelGroup == null)
		{
			m_ChannelGroup = new XRInputDeviceHapticImpulseChannelGroup();
		}
		m_ChannelGroup.Initialize(m_InputDevice);
		return m_ChannelGroup;
	}

	private void RefreshInputDeviceIfNeeded()
	{
		if (!m_InputDevice.isValid)
		{
			XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(m_Characteristics, out m_InputDevice);
		}
	}
}
