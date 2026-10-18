using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("XR/Locomotion/Legacy/Continuous Move Provider (Device-based)", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.DeviceBasedContinuousMoveProvider.html")]
[Obsolete("DeviceBasedContinuousMoveProvider has been deprecated in version 3.0.0. Use ContinuousMoveProvider instead.")]
public class DeviceBasedContinuousMoveProvider : ContinuousMoveProviderBase
{
	public enum InputAxes
	{
		Primary2DAxis,
		Secondary2DAxis
	}

	[SerializeField]
	[Tooltip("The 2D Input Axis on the controller devices that will be used to trigger a move.")]
	private InputAxes m_InputBinding;

	[SerializeField]
	[Tooltip("A list of controllers that allow move.  If an XRController is not enabled, or does not have input actions enabled, move will not work.")]
	private List<XRBaseController> m_Controllers = new List<XRBaseController>();

	[SerializeField]
	[Tooltip("Value below which input values will be clamped. After clamping, values will be renormalized to [0, 1] between min and max.")]
	private float m_DeadzoneMin = 0.125f;

	[SerializeField]
	[Tooltip("Value above which input values will be clamped. After clamping, values will be renormalized to [0, 1] between min and max.")]
	private float m_DeadzoneMax = 0.925f;

	private static readonly InputFeatureUsage<Vector2>[] k_Vec2UsageList = new InputFeatureUsage<Vector2>[2]
	{
		CommonUsages.primary2DAxis,
		CommonUsages.secondary2DAxis
	};

	public InputAxes inputBinding
	{
		get
		{
			return m_InputBinding;
		}
		set
		{
			m_InputBinding = value;
		}
	}

	public List<XRBaseController> controllers
	{
		get
		{
			return m_Controllers;
		}
		set
		{
			m_Controllers = value;
		}
	}

	public float deadzoneMin
	{
		get
		{
			return m_DeadzoneMin;
		}
		set
		{
			m_DeadzoneMin = value;
		}
	}

	public float deadzoneMax
	{
		get
		{
			return m_DeadzoneMax;
		}
		set
		{
			m_DeadzoneMax = value;
		}
	}

	protected override Vector2 ReadInput()
	{
		if (m_Controllers.Count == 0)
		{
			return Vector2.zero;
		}
		Vector2 zero = Vector2.zero;
		InputFeatureUsage<Vector2> usage = k_Vec2UsageList[(int)m_InputBinding];
		for (int i = 0; i < m_Controllers.Count; i++)
		{
			XRController xRController = m_Controllers[i] as XRController;
			if (xRController != null && xRController.enableInputActions && xRController.inputDevice.TryGetFeatureValue(usage, out var value))
			{
				zero += GetDeadzoneAdjustedValue(value);
			}
		}
		return zero;
	}

	protected Vector2 GetDeadzoneAdjustedValue(Vector2 value)
	{
		float magnitude = value.magnitude;
		float deadzoneAdjustedValue = GetDeadzoneAdjustedValue(magnitude);
		if (Mathf.Approximately(deadzoneAdjustedValue, 0f))
		{
			value = Vector2.zero;
		}
		else
		{
			value *= deadzoneAdjustedValue / magnitude;
		}
		return value;
	}

	protected float GetDeadzoneAdjustedValue(float value)
	{
		float num = m_DeadzoneMin;
		float num2 = m_DeadzoneMax;
		float num3 = Mathf.Abs(value);
		if (num3 < num)
		{
			return 0f;
		}
		if (num3 > num2)
		{
			return Mathf.Sign(value);
		}
		return Mathf.Sign(value) * ((num3 - num) / (num2 - num));
	}
}
