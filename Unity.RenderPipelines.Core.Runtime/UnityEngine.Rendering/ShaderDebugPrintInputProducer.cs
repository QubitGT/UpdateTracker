using UnityEngine.InputSystem;

namespace UnityEngine.Rendering;

public static class ShaderDebugPrintInputProducer
{
	public static ShaderDebugPrintInput Get()
	{
		ShaderDebugPrintInput result = default(ShaderDebugPrintInput);
		Mouse current = Mouse.current;
		result.pos = current.position.ReadValue();
		result.leftDown = current.leftButton.isPressed;
		result.rightDown = current.rightButton.isPressed;
		result.middleDown = current.middleButton.isPressed;
		return result;
	}
}
