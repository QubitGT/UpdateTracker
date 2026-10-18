using Unity.XR.CoreUtils;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[Preserve]
public static class XRInteractionSimulatorLoader
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	[Preserve]
	public static void Initialize()
	{
	}

	[Preserve]
	static XRInteractionSimulatorLoader()
	{
		if (!ScriptableSettings<XRDeviceSimulatorSettings>.Instance.automaticallyInstantiateSimulatorPrefab || (ScriptableSettings<XRDeviceSimulatorSettings>.Instance.automaticallyInstantiateInEditorOnly && !Application.isEditor))
		{
			return;
		}
		if (XRInteractionSimulator.instance != null)
		{
			Object.DontDestroyOnLoad(XRInteractionSimulator.instance);
			return;
		}
		if (XRDeviceSimulator.instance != null)
		{
			Object.DontDestroyOnLoad(XRDeviceSimulator.instance);
			return;
		}
		GameObject simulatorPrefab = ScriptableSettings<XRDeviceSimulatorSettings>.Instance.simulatorPrefab;
		if (simulatorPrefab == null)
		{
			Debug.LogWarning("XR Interaction Simulator prefab was missing, cannot automatically instantiate. Open Window > Package Manager, select XR Interaction Toolkit, and Reimport the XR Device Simulator sample, and then toggle the setting in Edit > Project Settings > XR Plug-in Management > XR Interaction Toolkit to try to resolve this issue.");
			return;
		}
		GameObject gameObject = Object.Instantiate(simulatorPrefab);
		gameObject.name = simulatorPrefab.name;
		Object.DontDestroyOnLoad(gameObject);
	}
}
