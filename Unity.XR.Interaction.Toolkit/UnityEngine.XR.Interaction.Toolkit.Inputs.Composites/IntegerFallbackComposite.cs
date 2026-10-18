using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Composites;

[Preserve]
public class IntegerFallbackComposite : FallbackComposite<int>
{
	[InputControl(layout = "Integer")]
	public int first;

	[InputControl(layout = "Integer")]
	public int second;

	[InputControl(layout = "Integer")]
	public int third;

	public override int ReadValue(ref InputBindingCompositeContext context)
	{
		int result = context.ReadValue<int>(first, out var sourceControl);
		if (sourceControl != null)
		{
			return result;
		}
		result = context.ReadValue<int>(second, out sourceControl);
		if (sourceControl != null)
		{
			return result;
		}
		return context.ReadValue<int>(third);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	[Preserve]
	private static void Initialize()
	{
	}

	[Preserve]
	static IntegerFallbackComposite()
	{
		UnityEngine.InputSystem.InputSystem.RegisterBindingComposite<IntegerFallbackComposite>();
	}
}
