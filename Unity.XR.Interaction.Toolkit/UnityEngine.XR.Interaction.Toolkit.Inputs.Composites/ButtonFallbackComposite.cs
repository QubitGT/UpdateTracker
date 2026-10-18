using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Composites;

[Preserve]
public class ButtonFallbackComposite : FallbackComposite<float>
{
	[InputControl(layout = "Button")]
	public int first;

	[InputControl(layout = "Button")]
	public int second;

	[InputControl(layout = "Button")]
	public int third;

	public override float ReadValue(ref InputBindingCompositeContext context)
	{
		float result = context.ReadValue<float>(first, out var sourceControl);
		if (sourceControl != null)
		{
			return result;
		}
		result = context.ReadValue<float>(second, out sourceControl);
		if (sourceControl != null)
		{
			return result;
		}
		return context.ReadValue<float>(third);
	}

	public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
	{
		return ReadValue(ref context);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	[Preserve]
	private static void Initialize()
	{
	}

	[Preserve]
	static ButtonFallbackComposite()
	{
		UnityEngine.InputSystem.InputSystem.RegisterBindingComposite<ButtonFallbackComposite>();
	}
}
