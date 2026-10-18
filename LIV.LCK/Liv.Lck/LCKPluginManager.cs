using Liv.Lck.DependencyInjection;
using UnityEngine;

namespace Liv.Lck;

public class LCKPluginManager : MonoBehaviour
{
	[InjectLck]
	private ILckService _lckService;

	[Header("Plugin Management")]
	[SerializeField]
	private bool autoInitializePlugins = true;

	[SerializeField]
	private bool logPluginInfo = true;

	private void Start()
	{
		if (!autoInitializePlugins)
		{
			return;
		}
		if (_lckService != null)
		{
			LCKPluginIntegration.InitializePlugins((LckService)_lckService);
			if (logPluginInfo)
			{
				LCKPluginIntegration.LogPluginInfo();
			}
		}
		else
		{
			LckLog.LogError("LCK service has not been injected.", "Start", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginIntegration.cs", 110);
		}
	}

	private void OnDestroy()
	{
		LCKPluginIntegration.ShutdownPlugins();
	}

	[ContextMenu("Test Plugin Access")]
	public void TestPluginAccess()
	{
		if (LCKPluginIntegration.HasPlugin<ExamplePlugin>())
		{
			LCKPluginIntegration.GetPlugin<ExamplePlugin>().DoSomething();
		}
		else
		{
			LckLog.LogWarning("ExamplePlugin not found", "TestPluginAccess", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginIntegration.cs", 135);
		}
		if (LCKPluginIntegration.HasPlugin<AnotherExamplePlugin>())
		{
			LCKPluginIntegration.GetPlugin<AnotherExamplePlugin>().DoSomethingElse();
		}
		else
		{
			LckLog.LogWarning("AnotherExamplePlugin not found", "TestPluginAccess", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LCKPluginIntegration.cs", 146);
		}
	}
}
