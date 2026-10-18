using UnityEngine;
using UnityEngine.InputSystem;

internal class DeltaTimeScaleProcessor : InputProcessor<Vector2>
{
	public override Vector2 Process(Vector2 value, InputControl control)
	{
		return value / Time.unscaledDeltaTime;
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize()
	{
		InputSystem.RegisterProcessor<DeltaTimeScaleProcessor>();
	}
}
