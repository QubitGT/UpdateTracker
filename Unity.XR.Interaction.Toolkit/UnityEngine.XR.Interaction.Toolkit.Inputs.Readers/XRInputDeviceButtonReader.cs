using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

[AddComponentMenu("XR/Input/XR Input Device Button Reader", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Readers.XRInputDeviceButtonReader.html")]
[DefaultExecutionOrder(-31000)]
public sealed class XRInputDeviceButtonReader : MonoBehaviour, IXRInputButtonReader, IXRInputValueReader<float>, IXRInputValueReader
{
	[SerializeField]
	[Tooltip("The value that is read to determine whether the button is down.")]
	private XRInputDeviceBoolValueReader m_BoolValueReader;

	[SerializeField]
	[Tooltip("The value that is read to determine the scalar value that varies from 0 to 1.")]
	private XRInputDeviceFloatValueReader m_FloatValueReader;

	private bool m_IsPerformed;

	private bool m_WasPerformedThisFrame;

	private bool m_WasCompletedThisFrame;

	private readonly UnityObjectReferenceCache<XRInputDeviceBoolValueReader> m_BoolValueReaderCache = new UnityObjectReferenceCache<XRInputDeviceBoolValueReader>();

	private readonly UnityObjectReferenceCache<XRInputDeviceFloatValueReader> m_FloatValueReaderCache = new UnityObjectReferenceCache<XRInputDeviceFloatValueReader>();

	public XRInputDeviceBoolValueReader boolValueReader
	{
		get
		{
			return m_BoolValueReader;
		}
		set
		{
			m_BoolValueReader = value;
		}
	}

	public XRInputDeviceFloatValueReader floatValueReader
	{
		get
		{
			return m_FloatValueReader;
		}
		set
		{
			m_FloatValueReader = value;
		}
	}

	private void Awake()
	{
		if (m_BoolValueReader == null)
		{
			Debug.LogError("No bool value reader set for XRInputDeviceButtonReader.", this);
		}
		if (m_FloatValueReader == null)
		{
			Debug.LogError("No float value reader set for XRInputDeviceButtonReader.", this);
		}
	}

	private void Update()
	{
		bool isPerformed = m_IsPerformed;
		m_IsPerformed = TryGetBoolValueReader(out var reference) && reference.ReadValue();
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
		if (TryGetFloatValueReader(out var reference))
		{
			return reference.ReadValue();
		}
		return 0f;
	}

	public bool TryReadValue(out float value)
	{
		if (TryGetFloatValueReader(out var reference))
		{
			return reference.TryReadValue(out value);
		}
		value = 0f;
		return false;
	}

	private bool TryGetBoolValueReader(out XRInputDeviceBoolValueReader reference)
	{
		return m_BoolValueReaderCache.TryGet(m_BoolValueReader, out reference);
	}

	private bool TryGetFloatValueReader(out XRInputDeviceFloatValueReader reference)
	{
		return m_FloatValueReaderCache.TryGet(m_FloatValueReader, out reference);
	}
}
