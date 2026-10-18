using System.Collections.Generic;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal abstract class DebugManagerAddon<Type> where Type : DebugManagerAddon<Type>, new()
{
	private static Type _instance;

	protected static IDebugUIPanel _uiPanel;

	protected readonly InstanceCache _instanceCache = new InstanceCache();

	protected readonly List<IDebugManager> _subDebugManagers = new List<IDebugManager>();

	public static Type Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new Type();
				_instance.Setup();
			}
			return _instance;
		}
	}

	protected abstract Telemetry.Method Method { get; }

	private static List<IDebugManager> _subManagersToInitialize => new List<IDebugManager>
	{
		new GizmoManagerForAddon(),
		new WatchManagerForAddon(),
		new ActionManagerForAddon(),
		new TweakManagerForAddon()
	};

	static DebugManagerAddon()
	{
		_instance = null;
		_uiPanel = null;
	}

	private void Setup()
	{
		if (DebugManager.Instance == null)
		{
			DebugManager.OnReady -= OnReady;
			DebugManager.OnReady += OnReady;
		}
		else
		{
			OnReady(DebugManager.Instance);
		}
	}

	internal static void Destroy()
	{
		if (_instance != null)
		{
			DebugManager.OnReady -= _instance.OnReady;
		}
	}

	private void InitSubManagers()
	{
		foreach (IDebugManager item in _subManagersToInitialize)
		{
			item.Setup(_uiPanel, _instanceCache);
			_subDebugManagers.Add(item);
		}
	}

	private void OnReady(DebugManager debugManager)
	{
		Telemetry.TelemetryTracker telemetryTracker = Telemetry.TelemetryTracker.Init(Method, _subDebugManagers, _instanceCache, debugManager);
		_uiPanel = debugManager.UiPanel;
		InitSubManagers();
		OnReadyInternal();
		telemetryTracker.OnStart();
	}

	protected virtual void OnReadyInternal()
	{
	}
}
