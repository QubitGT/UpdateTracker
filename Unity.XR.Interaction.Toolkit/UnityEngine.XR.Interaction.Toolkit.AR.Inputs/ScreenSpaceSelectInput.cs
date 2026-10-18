using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

namespace UnityEngine.XR.Interaction.Toolkit.AR.Inputs;

[AddComponentMenu("XR/Input/Screen Space Select Input", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.Inputs.ScreenSpaceSelectInput.html")]
[DefaultExecutionOrder(-30050)]
public class ScreenSpaceSelectInput : MonoBehaviour, IXRInputButtonReader, IXRInputValueReader<float>, IXRInputValueReader
{
	[SerializeField]
	private XRInputValueReader<Vector2> m_TapStartPositionInput = new XRInputValueReader<Vector2>("Tap Start Position");

	[SerializeField]
	private XRInputValueReader<Vector2> m_DragCurrentPositionInput = new XRInputValueReader<Vector2>("Drag Current Position");

	[SerializeField]
	private XRInputValueReader<float> m_PinchGapDeltaInput = new XRInputValueReader<float>("Pinch Gap Delta");

	[SerializeField]
	private XRInputValueReader<float> m_TwistDeltaRotationInput = new XRInputValueReader<float>("Twist Delta Rotation");

	private bool m_IsPerformed;

	private bool m_WasPerformedThisFrame;

	private bool m_WasCompletedThisFrame;

	private Vector2 m_TapStartPosition;

	public XRInputValueReader<Vector2> tapStartPositionInput
	{
		get
		{
			return m_TapStartPositionInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_TapStartPositionInput, value, this);
		}
	}

	public XRInputValueReader<Vector2> dragCurrentPositionInput
	{
		get
		{
			return m_DragCurrentPositionInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_DragCurrentPositionInput, value, this);
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

	protected void Update()
	{
		bool isPerformed = m_IsPerformed;
		Vector2 tapStartPosition = m_TapStartPosition;
		bool flag = m_TapStartPositionInput.TryReadValue(out m_TapStartPosition) && tapStartPosition != m_TapStartPosition;
		m_IsPerformed = m_PinchGapDeltaInput.TryReadValue(out var value) || m_TwistDeltaRotationInput.TryReadValue(out value) || m_DragCurrentPositionInput.TryReadValue(out var _) || flag;
		m_WasPerformedThisFrame = !isPerformed && m_IsPerformed;
		m_WasCompletedThisFrame = isPerformed && !m_IsPerformed;
	}

	public bool ReadIsPerformed()
	{
		return m_IsPerformed;
	}

	public bool ReadWasPerformedThisFrame()
	{
		return m_WasPerformedThisFrame;
	}

	public bool ReadWasCompletedThisFrame()
	{
		return m_WasCompletedThisFrame;
	}

	public float ReadValue()
	{
		if (!m_IsPerformed)
		{
			return 0f;
		}
		return 1f;
	}

	public bool TryReadValue(out float value)
	{
		value = (m_IsPerformed ? 1f : 0f);
		return m_IsPerformed;
	}
}
