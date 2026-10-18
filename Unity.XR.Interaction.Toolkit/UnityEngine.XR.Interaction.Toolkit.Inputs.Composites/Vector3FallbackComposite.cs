using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Composites;

[Preserve]
public class Vector3FallbackComposite : FallbackComposite<Vector3>
{
	[InputControl(layout = "Vector3")]
	public int first;

	[InputControl(layout = "Vector3")]
	public int second;

	[InputControl(layout = "Vector3")]
	public int third;

	public override Vector3 ReadValue(ref InputBindingCompositeContext context)
	{
		Vector3 result = context.ReadValue<Vector3, Vector3MagnitudeComparer>(first, out var sourceControl);
		if (sourceControl != null)
		{
			return result;
		}
		result = context.ReadValue<Vector3, Vector3MagnitudeComparer>(second, out sourceControl);
		if (sourceControl != null)
		{
			return result;
		}
		return context.ReadValue<Vector3, Vector3MagnitudeComparer>(third);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	[Preserve]
	private static void Initialize()
	{
	}

	[Preserve]
	static Vector3FallbackComposite()
	{
		UnityEngine.InputSystem.InputSystem.RegisterBindingComposite<Vector3FallbackComposite>();
	}
}
