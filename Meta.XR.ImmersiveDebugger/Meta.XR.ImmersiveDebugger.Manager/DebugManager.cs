using System;
using System.Collections.Generic;
using Meta.XR.ImmersiveDebugger.Hierarchy;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class DebugManager : MonoBehaviour
{
	internal delegate bool ShouldRetrieveInstanceDelegate();

	protected readonly InstanceCache InstanceCache = new InstanceCache();

	protected readonly List<IDebugManager> SubDebugManagers = new List<IDebugManager>();

	internal bool ShouldRetrieveInstances;

	private const float RetrievalIntervalInSec = 1f;

	private float _lastRetrievedTime;

	private readonly OVRSampledEventSender _frameUpdateRecorder = new OVRSampledEventSender(163056655, 0.1f, (OVRTelemetryMarker marker) => marker.AddPlayModeOrigin());

	public static DebugManager Instance { get; private set; }

	public IDebugUIPanel UiPanel { get; private set; }

	public static event Action<DebugManager> OnReady;

	public event Action OnFocusLostAction;

	public event Action OnDisableAction;

	public event Action OnUpdateAction;

	internal event ShouldRetrieveInstanceDelegate CustomShouldRetrieveInstanceCondition;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		Instance = null;
	}

	private void Awake()
	{
		Instance = this;
		InstanceCache.OnCacheChangedForTypeEvent += ProcessLoadedTypeBySubManagers;
		InstanceCache.OnInstanceRemoved += UnregisterInspector;
	}

	private void Start()
	{
		Telemetry.TelemetryTracker telemetryTracker = Telemetry.TelemetryTracker.Init(Telemetry.Method.Attributes, SubDebugManagers, InstanceCache, this);
		UiPanel = GetComponentInChildren<IDebugUIPanel>(includeInactive: true);
		InitSubManagers();
		AssemblyParser.RegisterAssemblyTypes(InstanceCache.RegisterClassTypes);
		RegisterTypesFromInspectedData();
		ShouldRetrieveInstances = true;
		RetrieveInstancesIfNeeded();
		DebugManager.OnReady?.Invoke(this);
		telemetryTracker.OnStart();
		_ = DebugManagerAddon<Meta.XR.ImmersiveDebugger.Hierarchy.Manager>.Instance;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (!hasFocus)
		{
			this.OnFocusLostAction?.Invoke();
		}
	}

	private void OnDisable()
	{
		this.OnDisableAction?.Invoke();
	}

	private void OnDestroy()
	{
		AssemblyParser.Unregister(InstanceCache.RegisterClassTypes);
	}

	private void Update()
	{
		RetrieveInstancesIfNeeded();
		this.OnUpdateAction?.Invoke();
		_frameUpdateRecorder.Send();
	}

	private void RetrieveInstancesIfNeeded()
	{
		if (Time.time - _lastRetrievedTime > 1f)
		{
			ShouldRetrieveInstances = true;
		}
		if (ShouldRetrieveInstances && this.CustomShouldRetrieveInstanceCondition != null)
		{
			ShouldRetrieveInstances = this.CustomShouldRetrieveInstanceCondition();
		}
		if (ShouldRetrieveInstances)
		{
			_frameUpdateRecorder.Start();
			InstanceCache.RetrieveInstances();
			_lastRetrievedTime = Time.time;
			ShouldRetrieveInstances = false;
		}
	}

	protected virtual void InitSubManagers()
	{
		RegisterManager<GizmoManager>();
		RegisterManager<WatchManager>();
		RegisterManager<ActionManager>();
		RegisterManager<TweakManager>();
	}

	private void RegisterManager<TManagerType>() where TManagerType : IDebugManager, new()
	{
		TManagerType val = new TManagerType();
		val.Setup(UiPanel, InstanceCache);
		SubDebugManagers.Add(val);
	}

	private void ProcessLoadedTypeBySubManagers(Type type)
	{
		foreach (IDebugManager subDebugManager in SubDebugManagers)
		{
			subDebugManager.ProcessType(type);
		}
	}

	private void UnregisterInspector(InstanceHandle handle)
	{
		UiPanel.UnregisterInspector(handle, Category.Default, allCategories: true);
	}

	private void RegisterTypesFromInspectedData()
	{
		InspectedDataRegistry.Reset();
		List<InspectedData> inspectedDataAssets = RuntimeSettings.Instance.InspectedDataAssets;
		List<bool> inspectedDataEnabled = RuntimeSettings.Instance.InspectedDataEnabled;
		for (int i = 0; i < inspectedDataAssets.Count; i++)
		{
			if (inspectedDataEnabled[i])
			{
				InstanceCache.RegisterClassTypes(inspectedDataAssets[i].ExtractTypesFromInspectedMembers());
			}
		}
	}
}
