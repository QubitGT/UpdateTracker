using System;

namespace Oculus.Haptics;

internal static class Utils
{
	internal static Ffi.Controller ControllerToFfiController(Controller controller)
	{
		return controller switch
		{
			Controller.Left => Ffi.Controller.Left, 
			Controller.Right => Ffi.Controller.Right, 
			Controller.Both => Ffi.Controller.Both, 
			_ => throw new ArgumentException($"Invalid controller selected: {controller}."), 
		};
	}

	internal static float Map(int input, int inMin, int inMax, int outMin, int outMax)
	{
		float num = (input - inMin) * (outMax - outMin);
		float num2 = inMax - inMin;
		return num / num2 + (float)outMin;
	}
}
