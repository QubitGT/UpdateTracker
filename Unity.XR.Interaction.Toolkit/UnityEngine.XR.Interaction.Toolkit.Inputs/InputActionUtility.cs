using System;
using UnityEngine.InputSystem;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs;

internal static class InputActionUtility
{
	public static InputAction CreateValueAction(Type valueType, string name = null)
	{
		return new InputAction(name, InputActionType.Value, null, null, null, GetExpectedControlType(valueType));
	}

	public static InputAction CreateButtonAction(string name = null, bool wantsInitialStateCheck = false)
	{
		return new InputAction(name, InputActionType.Button)
		{
			wantsInitialStateCheck = wantsInitialStateCheck
		};
	}

	public static InputAction CreatePassThroughAction(Type valueType = null, string name = null, bool wantsInitialStateCheck = false)
	{
		return new InputAction(name, InputActionType.PassThrough, null, null, null, GetExpectedControlType(valueType))
		{
			wantsInitialStateCheck = wantsInitialStateCheck
		};
	}

	private static string GetExpectedControlType(Type valueType)
	{
		if ((object)valueType != null)
		{
			if (valueType == typeof(float))
			{
				return "Axis";
			}
			if (valueType == typeof(int) || valueType == typeof(InputTrackingState))
			{
				return "Integer";
			}
			if (valueType == typeof(Quaternion))
			{
				return "Quaternion";
			}
			if (valueType == typeof(Vector2))
			{
				return "Vector2";
			}
			if (valueType == typeof(Vector3))
			{
				return "Vector3";
			}
		}
		return null;
	}
}
