using System.Diagnostics;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.AR.Inputs;

[AddComponentMenu("XR/Input/Screen Space Rotate Input", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AR.Inputs.ScreenSpaceRotateInput.html")]
public class ScreenSpaceRotateInput : MonoBehaviour, IXRInputValueReader<Vector2>, IXRInputValueReader
{
	[SerializeField]
	[Tooltip("The ray interactor to get the attach transform from.")]
	private XRRayInteractor m_RayInteractor;

	[SerializeField]
	[Tooltip("The input used to read the twist delta rotation value.")]
	private XRInputValueReader<float> m_TwistDeltaRotationInput = new XRInputValueReader<float>("Twist Delta Rotation");

	[SerializeField]
	[Tooltip("The input used to read the drag delta value.")]
	private XRInputValueReader<Vector2> m_DragDeltaInput = new XRInputValueReader<Vector2>("Drag Delta");

	[SerializeField]
	[Tooltip("The input used to read the screen touch count value.")]
	private XRInputValueReader<int> m_ScreenTouchCountInput = new XRInputValueReader<int>("Screen Touch Count");

	public XRRayInteractor rayInteractor
	{
		get
		{
			return m_RayInteractor;
		}
		set
		{
			m_RayInteractor = value;
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

	public XRInputValueReader<Vector2> dragDeltaInput
	{
		get
		{
			return m_DragDeltaInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_DragDeltaInput, value, this);
		}
	}

	public XRInputValueReader<int> screenTouchCountInput
	{
		get
		{
			return m_ScreenTouchCountInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ScreenTouchCountInput, value, this);
		}
	}

	[Conditional("UNITY_EDITOR")]
	protected void Reset()
	{
	}

	protected void Awake()
	{
		if (m_RayInteractor == null)
		{
			m_RayInteractor = GetComponentInParent<XRRayInteractor>(includeInactive: true);
		}
	}

	protected void OnEnable()
	{
		m_TwistDeltaRotationInput.EnableDirectActionIfModeUsed();
		m_DragDeltaInput.EnableDirectActionIfModeUsed();
		m_ScreenTouchCountInput.EnableDirectActionIfModeUsed();
	}

	protected void OnDisable()
	{
		m_TwistDeltaRotationInput.DisableDirectActionIfModeUsed();
		m_DragDeltaInput.DisableDirectActionIfModeUsed();
		m_ScreenTouchCountInput.DisableDirectActionIfModeUsed();
	}

	public Vector2 ReadValue()
	{
		TryReadValue(out var value);
		return value;
	}

	public bool TryReadValue(out Vector2 value)
	{
		if (m_TwistDeltaRotationInput.TryReadValue(out var value2))
		{
			value = new Vector2(0f - value2, 0f);
			return true;
		}
		if (m_ScreenTouchCountInput.ReadValue() > 1 && m_DragDeltaInput.TryReadValue(out var value3))
		{
			Transform attachTransform = m_RayInteractor.attachTransform;
			value = new Vector2((Quaternion.Inverse(Quaternion.LookRotation(attachTransform.forward, Vector3.up)) * attachTransform.rotation * value3).x * DisplayUtility.screenDpiRatio * -50f, 0f);
			return true;
		}
		value = Vector2.zero;
		return false;
	}
}
