using UnityEngine;

namespace Oculus.Interaction.Input;

public struct ControllerInput
{
	public ControllerButtonUsage ButtonUsageMask { get; set; }

	public bool PrimaryButton => (ButtonUsageMask & ControllerButtonUsage.PrimaryButton) != 0;

	public bool PrimaryTouch => (ButtonUsageMask & ControllerButtonUsage.PrimaryTouch) != 0;

	public bool SecondaryButton => (ButtonUsageMask & ControllerButtonUsage.SecondaryButton) != 0;

	public bool SecondaryTouch => (ButtonUsageMask & ControllerButtonUsage.SecondaryTouch) != 0;

	public bool GripButton => (ButtonUsageMask & ControllerButtonUsage.GripButton) != 0;

	public bool TriggerButton => (ButtonUsageMask & ControllerButtonUsage.TriggerButton) != 0;

	public bool MenuButton => (ButtonUsageMask & ControllerButtonUsage.MenuButton) != 0;

	public bool Primary2DAxisClick => (ButtonUsageMask & ControllerButtonUsage.Primary2DAxisClick) != 0;

	public bool Primary2DAxisTouch => (ButtonUsageMask & ControllerButtonUsage.Primary2DAxisTouch) != 0;

	public bool Thumbrest => (ButtonUsageMask & ControllerButtonUsage.Thumbrest) != 0;

	public float Trigger { get; private set; }

	public float Grip { get; private set; }

	public Vector2 Primary2DAxis { get; private set; }

	public Vector2 Secondary2DAxis { get; private set; }

	public void Clear()
	{
		ButtonUsageMask = ControllerButtonUsage.None;
		Trigger = 0f;
		Grip = 0f;
		Primary2DAxis = Vector2.zero;
		Secondary2DAxis = Vector2.zero;
	}

	public void SetButton(ControllerButtonUsage usage, bool value)
	{
		if (value)
		{
			ButtonUsageMask |= usage;
		}
		else
		{
			ButtonUsageMask &= ~usage;
		}
	}

	public void SetAxis1D(ControllerAxis1DUsage usage, float value)
	{
		switch (usage)
		{
		case ControllerAxis1DUsage.Trigger:
			Trigger = value;
			break;
		case ControllerAxis1DUsage.Grip:
			Grip = value;
			break;
		}
	}

	public void SetAxis2D(ControllerAxis2DUsage usage, Vector2 value)
	{
		switch (usage)
		{
		case ControllerAxis2DUsage.Primary2DAxis:
			Primary2DAxis = value;
			break;
		case ControllerAxis2DUsage.Secondary2DAxis:
			Secondary2DAxis = value;
			break;
		}
	}
}
