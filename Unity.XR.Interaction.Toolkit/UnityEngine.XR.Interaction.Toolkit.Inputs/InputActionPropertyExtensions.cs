using UnityEngine.InputSystem;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs;

public static class InputActionPropertyExtensions
{
	public static void EnableDirectAction(this InputActionProperty property)
	{
		if (!(property.reference != null))
		{
			property.action?.Enable();
		}
	}

	public static void DisableDirectAction(this InputActionProperty property)
	{
		if (!(property.reference != null))
		{
			property.action?.Disable();
		}
	}
}
