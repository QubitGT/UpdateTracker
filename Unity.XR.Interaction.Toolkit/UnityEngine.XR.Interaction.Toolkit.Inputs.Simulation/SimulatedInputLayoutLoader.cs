using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[Preserve]
public static class SimulatedInputLayoutLoader
{
	[Preserve]
	static SimulatedInputLayoutLoader()
	{
		RegisterInputLayouts();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	[Preserve]
	public static void Initialize()
	{
	}

	private static void RegisterInputLayouts()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout<XRSimulatedHMD>(null, default(InputDeviceMatcher).WithProduct("XRSimulatedHMD"));
		UnityEngine.InputSystem.InputSystem.RegisterLayout<XRSimulatedController>(null, default(InputDeviceMatcher).WithProduct("XRSimulatedController"));
	}
}
