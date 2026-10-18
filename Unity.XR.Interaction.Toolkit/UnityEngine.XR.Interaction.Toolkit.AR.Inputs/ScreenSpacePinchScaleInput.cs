using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.AR.Inputs;

[AddComponentMenu("XR/Input/Screen Space Pinch Scale Input", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.Inputs.ScreenSpacePinchScaleInput.html")]
public class ScreenSpacePinchScaleInput : MonoBehaviour, IXRInputValueReader<float>, IXRInputValueReader
{
	[SerializeField]
	[Tooltip("Enables a rotation threshold that blocks pinch scale gestures when surpassed.")]
	private bool m_UseRotationThreshold = true;

	[SerializeField]
	[Tooltip("The threshold at which a gestures will be interpreted only as rotation and not a pinch scale gesture.")]
	private float m_RotationThreshold = 0.02f;

	[SerializeField]
	[Tooltip("The input used to read the pinch gap delta value.")]
	private XRInputValueReader<float> m_PinchGapDeltaInput = new XRInputValueReader<float>("Pinch Gap Delta");

	[SerializeField]
	[Tooltip("The input used to read the twist delta rotation value.")]
	private XRInputValueReader<float> m_TwistDeltaRotationInput = new XRInputValueReader<float>("Twist Delta Rotation");

	public bool useRotationThreshold
	{
		get
		{
			return m_UseRotationThreshold;
		}
		set
		{
			m_UseRotationThreshold = value;
		}
	}

	public float rotationThreshold
	{
		get
		{
			return m_RotationThreshold;
		}
		set
		{
			m_RotationThreshold = value;
		}
	}

	public XRInputValueReader<float> pinchGapDeltaInput
	{
		get
		{
			return m_PinchGapDeltaInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_PinchGapDeltaInput, value, this);
		}
	}

	public XRInputValueReader<float> twistDeltaRotationInput
	{
		get
		{
			return m_TwistDeltaRotationInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_TwistDeltaRotationInput, value, this);
		}
	}

	protected void OnEnable()
	{
		m_PinchGapDeltaInput.EnableDirectActionIfModeUsed();
		m_TwistDeltaRotationInput.EnableDirectActionIfModeUsed();
	}

	protected void OnDisable()
	{
		m_PinchGapDeltaInput.DisableDirectActionIfModeUsed();
		m_TwistDeltaRotationInput.DisableDirectActionIfModeUsed();
	}

	public float ReadValue()
	{
		TryReadValue(out var value);
		return value;
	}

	public bool TryReadValue(out float value)
	{
		if (m_UseRotationThreshold && m_TwistDeltaRotationInput.TryReadValue(out var value2) && Mathf.Abs(value2) >= m_RotationThreshold)
		{
			value = 0f;
			return true;
		}
		if (m_PinchGapDeltaInput.TryReadValue(out var value3))
		{
			value = value3 * DisplayUtility.screenDpiRatio;
			return true;
		}
		value = 0f;
		return false;
	}
}
