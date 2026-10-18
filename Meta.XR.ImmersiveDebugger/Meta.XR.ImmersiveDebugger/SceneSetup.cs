using System;
using Meta.XR.ImmersiveDebugger.Gizmo;
using Meta.XR.ImmersiveDebugger.Manager;
using Meta.XR.ImmersiveDebugger.UserInterface;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger;

internal static class SceneSetup
{
	[RuntimeInitializeOnLoadMethod]
	private static void OnLoad()
	{
		if (RuntimeSettings.Instance.ImmersiveDebuggerEnabled && !RuntimeSettings.Instance.EnableOnlyInDebugBuild)
		{
			SetupImmersiveDebugger();
		}
	}

	internal static void SetupImmersiveDebugger()
	{
		GizmoTypesRegistry.InitGizmos();
		GameObject gameObject = new GameObject("ImmersiveDebuggerManager");
		gameObject.AddComponent<DebugManager>();
		GameObject gameObject2 = new GameObject("ImmersiveDebuggerInterface");
		gameObject2.transform.SetParent(gameObject.transform);
		gameObject2.AddComponent<DebugInterface>();
		Type type = Type.GetType(RuntimeSettings.Instance.CustomIntegrationConfigClassName);
		if (RuntimeSettings.Instance.UseCustomIntegrationConfig && type != null)
		{
			if (typeof(MonoBehaviour).IsAssignableFrom(type) && type.IsSubclassOf(typeof(CustomIntegrationConfigBase)))
			{
				gameObject.AddComponent(type);
			}
			else
			{
				Debug.LogWarning("CustomIntegrationConfig file is not an valid type");
			}
		}
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
	}
}
