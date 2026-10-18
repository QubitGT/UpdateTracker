using System;
using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("XR/Locomotion/Legacy/Snap Turn Provider (Device-based)", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.DeviceBasedSnapTurnProvider.html")]
[Obsolete("DeviceBasedSnapTurnProvider has been deprecated in version 3.0.0. Use SnapTurnProvider instead.")]
public class DeviceBasedSnapTurnProvider : SnapTurnProviderBase
{
	public enum InputAxes
	{
		Primary2DAxis,
		Secondary2DAxis
	}

	[SerializeField]
	[Tooltip("The 2D Input Axis on the controller devices that will be used to trigger a snap turn.")]
	private InputAxes m_TurnUsage;

	[SerializeField]
	[Tooltip("A list of controllers that allow Snap Turn.  If an XRController is not enabled, or does not have input actions enabled, snap turn will not work.")]
	private List<XRBaseController> m_Controllers = new List<XRBaseController>();

	[SerializeField]
	[Tooltip("The deadzone that the controller movement will have to be above to trigger a snap turn.")]
	private float m_DeadZone = 0.75f;

	private static readonly InputFeatureUsage<Vector2>[] k_Vec2UsageList = new InputFeatureUsage<Vector2>[2]
	{
		CommonUsages.primary2DAxis,
		CommonUsages.secondary2DAxis
	};

	public InputAxes turnUsage
	{
		get
		{
			return m_TurnUsage;
		}
		set
		{
			m_TurnUsage = value;
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

	public float deadZone
	{
		get
		{
			return m_DeadZone;
		}
		set
		{
			m_DeadZone = value;
		}
	}

	protected override Vector2 ReadInput()
	{
		if (m_Controllers.Count == 0)
		{
			return Vector2.zero;
		}
		Vector2 zero = Vector2.zero;
		InputFeatureUsage<Vector2> usage = k_Vec2UsageList[(int)m_TurnUsage];
		float num = m_DeadZone * m_DeadZone;
		for (int i = 0; i < m_Controllers.Count; i++)
		{
			XRController xRController = m_Controllers[i] as XRController;
			if (xRController != null && xRController.enableInputActions && xRController.inputDevice.TryGetFeatureValue(usage, out var value) && value.sqrMagnitude > num)
			{
				zero += value;
			}
		}
		return zero;
	}
}
