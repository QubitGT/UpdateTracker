using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Composites;

[Preserve]
public class QuaternionFallbackComposite : FallbackComposite<Quaternion>
{
	[InputControl(layout = "Quaternion")]
	public int first;

	[InputControl(layout = "Quaternion")]
	public int second;

	[InputControl(layout = "Quaternion")]
	public int third;

	public override Quaternion ReadValue(ref InputBindingCompositeContext context)
	{
		Quaternion result = context.ReadValue<Quaternion, QuaternionCompositeComparer>(first, out var sourceControl);
		if (sourceControl != null)
		{
			return result;
		}
		result = context.ReadValue<Quaternion, QuaternionCompositeComparer>(second, out sourceControl);
		if (sourceControl != null)
		{
			return result;
		}
		return context.ReadValue<Quaternion, QuaternionCompositeComparer>(third);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	[Preserve]
	private static void Initialize()
	{
	}

	[Preserve]
	static QuaternionFallbackComposite()
	{
		UnityEngine.InputSystem.InputSystem.RegisterBindingComposite<QuaternionFallbackComposite>();
	}
}
