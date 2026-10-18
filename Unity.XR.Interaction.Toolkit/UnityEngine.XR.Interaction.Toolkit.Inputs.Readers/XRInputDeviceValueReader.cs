namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

public abstract class XRInputDeviceValueReader : ScriptableObject
{
	[SerializeField]
	[Tooltip("Characteristics of the input device to read from. Controllers are either:\nHeld In Hand, Tracked Device, Controller, Left\nHeld In Hand, Tracked Device, Controller, Right")]
	private protected InputDeviceCharacteristics m_Characteristics;

	public InputDeviceCharacteristics characteristics
	{
		get
		{
			return m_Characteristics;
		}
		set
		{
			m_Characteristics = value;
		}
	}
}
public abstract class XRInputDeviceValueReader<TValue> : XRInputDeviceValueReader, IXRInputValueReader<TValue>, IXRInputValueReader where TValue : struct
{
	[SerializeField]
	[Tooltip("The name of the input feature to read.")]
	private InputFeatureUsageString<TValue> m_Usage;

	private InputDevice m_InputDevice;

	public InputFeatureUsageString<TValue> usage
	{
		get
		{
			return m_Usage;
		}
		set
		{
			m_Usage = value;
		}
	}

	public abstract TValue ReadValue();

	public abstract bool TryReadValue(out TValue value);

	protected bool ReadBoolValue()
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<bool>(m_Usage.name), out var value))
		{
			return value;
		}
		return false;
	}

	protected uint ReadUIntValue()
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<uint>(m_Usage.name), out var value))
		{
			return value;
		}
		return 0u;
	}

	protected float ReadFloatValue()
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<float>(m_Usage.name), out var value))
		{
			return value;
		}
		return 0f;
	}

	protected Vector2 ReadVector2Value()
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<Vector2>(m_Usage.name), out var value))
		{
			return value;
		}
		return default(Vector2);
	}

	protected Vector3 ReadVector3Value()
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<Vector3>(m_Usage.name), out var value))
		{
			return value;
		}
		return default(Vector3);
	}

	protected Quaternion ReadQuaternionValue()
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<Quaternion>(m_Usage.name), out var value))
		{
			return value;
		}
		return default(Quaternion);
	}

	protected InputTrackingState ReadInputTrackingStateValue()
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<InputTrackingState>(m_Usage.name), out var value))
		{
			return value;
		}
		return InputTrackingState.None;
	}

	protected bool TryReadBoolValue(out bool value)
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<bool>(m_Usage.name), out value))
		{
			return true;
		}
		value = false;
		return false;
	}

	protected bool TryReadUIntValue(out uint value)
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<uint>(m_Usage.name), out value))
		{
			return true;
		}
		value = 0u;
		return false;
	}

	protected bool TryReadFloatValue(out float value)
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<float>(m_Usage.name), out value))
		{
			return true;
		}
		value = 0f;
		return false;
	}

	protected bool TryReadVector2Value(out Vector2 value)
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<Vector2>(m_Usage.name), out value))
		{
			return true;
		}
		value = default(Vector2);
		return false;
	}

	protected bool TryReadVector3Value(out Vector3 value)
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<Vector3>(m_Usage.name), out value))
		{
			return true;
		}
		value = default(Vector3);
		return false;
	}

	protected bool TryReadQuaternionValue(out Quaternion value)
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<Quaternion>(m_Usage.name), out value))
		{
			return true;
		}
		value = default(Quaternion);
		return false;
	}

	protected bool TryReadInputTrackingStateValue(out InputTrackingState value)
	{
		if (RefreshInputDeviceIfNeeded() && m_InputDevice.TryGetFeatureValue(new InputFeatureUsage<InputTrackingState>(m_Usage.name), out value))
		{
			return true;
		}
		value = InputTrackingState.None;
		return false;
	}

	protected bool RefreshInputDeviceIfNeeded()
	{
		if (!m_InputDevice.isValid)
		{
			return XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(m_Characteristics, out m_InputDevice);
		}
		return true;
	}
}
