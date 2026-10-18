using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

[AddComponentMenu("XR/Haptics/Haptic Impulse Player", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticImpulsePlayer.html")]
public class HapticImpulsePlayer : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Specifies the output haptic control or controller that haptic impulses will be sent to.")]
	private XRInputHapticImpulseProvider m_HapticOutput = new XRInputHapticImpulseProvider("Haptic");

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Amplitude multiplier which can be used to dampen the haptic impulses sent by this component.")]
	private float m_AmplitudeMultiplier = 1f;

	public XRInputHapticImpulseProvider hapticOutput
	{
		get
		{
			return m_HapticOutput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_HapticOutput, value, this);
		}
	}

	public float amplitudeMultiplier
	{
		get
		{
			return m_AmplitudeMultiplier;
		}
		set
		{
			m_AmplitudeMultiplier = value;
		}
	}

	protected void Awake()
	{
		XRInputHapticImpulseProvider xRInputHapticImpulseProvider = m_HapticOutput;
		if (xRInputHapticImpulseProvider != null && xRInputHapticImpulseProvider.inputSourceMode == XRInputHapticImpulseProvider.InputSourceMode.InputActionReference && m_HapticOutput.inputActionReference == null)
		{
			IXRHapticImpulseProvider componentInParent = base.gameObject.GetComponentInParent<IXRHapticImpulseProvider>(includeInactive: true);
			if (componentInParent as Component != null)
			{
				m_HapticOutput.SetObjectReference(componentInParent);
				m_HapticOutput.inputSourceMode = XRInputHapticImpulseProvider.InputSourceMode.ObjectReference;
			}
		}
	}

	protected void OnEnable()
	{
		m_HapticOutput.EnableDirectActionIfModeUsed();
	}

	protected void OnDisable()
	{
		m_HapticOutput.DisableDirectActionIfModeUsed();
	}

	public bool SendHapticImpulse(float amplitude, float duration)
	{
		return SendHapticImpulse(amplitude, duration, 0f);
	}

	public bool SendHapticImpulse(float amplitude, float duration, float frequency)
	{
		if (!base.isActiveAndEnabled)
		{
			return false;
		}
		return (m_HapticOutput.GetChannelGroup()?.GetChannel()?.SendHapticImpulse(amplitude * m_AmplitudeMultiplier, duration, frequency)).GetValueOrDefault();
	}

	internal static HapticImpulsePlayer GetOrCreateInHierarchy(GameObject gameObject)
	{
		HapticImpulsePlayer hapticImpulsePlayer = gameObject.GetComponentInParent<HapticImpulsePlayer>(includeInactive: true);
		if (hapticImpulsePlayer == null)
		{
			Component component = gameObject.GetComponentInParent<IXRHapticImpulseProvider>(includeInactive: true) as Component;
			hapticImpulsePlayer = ((component != null) ? component.gameObject.AddComponent<HapticImpulsePlayer>() : gameObject.AddComponent<HapticImpulsePlayer>());
		}
		return hapticImpulsePlayer;
	}
}
