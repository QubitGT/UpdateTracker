using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger;

public class RuntimeSettings : OVRRuntimeAssetsBase, ISerializationCallbackReceiver
{
	public enum DistanceOption
	{
		Close,
		Default,
		Far
	}

	internal static string InstanceAssetName = "ImmersiveDebuggerSettings";

	private static RuntimeSettings _instance;

	[SerializeField]
	private List<DebugData> debugTypes;

	internal Dictionary<string, List<string>> debugTypesDict;

	[SerializeField]
	private bool immersiveDebuggerEnabled;

	[SerializeField]
	private bool immersiveDebuggerDisplayAtStartup;

	[SerializeField]
	private bool enableOnlyInDebugBuild;

	[SerializeField]
	private bool showInspectors;

	[SerializeField]
	private bool showConsole;

	[SerializeField]
	private bool followOverride = true;

	[SerializeField]
	private bool rotateOverride;

	[SerializeField]
	private bool showInfoLog;

	[SerializeField]
	private bool showWarningLog = true;

	[SerializeField]
	private bool showErrorLog = true;

	[SerializeField]
	private bool collapsedIdenticalLogEntries;

	[SerializeField]
	private int maximumNumberOfLogEntries = 1000;

	[SerializeField]
	private DistanceOption panelDistance = DistanceOption.Default;

	[SerializeField]
	private bool createEventSystem = true;

	[SerializeField]
	private bool automaticLayerCullingUpdate = true;

	[SerializeField]
	private int panelLayer = 20;

	[SerializeField]
	private int meshRendererLayer = 21;

	[SerializeField]
	private int overlayDepth = 10;

	[SerializeField]
	private bool useOverlay = true;

	[SerializeField]
	private List<bool> inspectedDataEnabled = new List<bool>();

	[SerializeField]
	private List<InspectedData> inspectedDataAssets = new List<InspectedData>();

	[SerializeField]
	private bool useCustomIntegrationConfig;

	[SerializeField]
	private string customIntegrationConfigClassName;

	[SerializeField]
	private bool hierarchyViewShowsPrivateMembers;

	[SerializeField]
	private OVRInput.Button clickButton = OVRInput.Button.One | OVRInput.Button.PrimaryIndexTrigger;

	[SerializeField]
	private OVRInput.Button toggleFollowTranslationButton;

	[SerializeField]
	private OVRInput.Button toggleFollowRotationButton;

	[SerializeField]
	private OVRInput.Button immersiveDebuggerToggleDisplayButton = OVRInput.Button.Two;

	internal static RuntimeSettings Instance
	{
		get
		{
			if (_instance == null)
			{
				OVRRuntimeAssetsBase.LoadAsset(out RuntimeSettings assetInstance, InstanceAssetName, (Action<RuntimeSettings>)null);
				_instance = assetInstance;
			}
			return _instance;
		}
	}

	internal bool ImmersiveDebuggerEnabled
	{
		get
		{
			return immersiveDebuggerEnabled;
		}
		set
		{
			if (immersiveDebuggerEnabled != value)
			{
				immersiveDebuggerEnabled = value;
				RuntimeSettings.OnImmersiveDebuggerEnabledChanged?.Invoke();
			}
		}
	}

	internal bool ImmersiveDebuggerDisplayAtStartup
	{
		get
		{
			return immersiveDebuggerDisplayAtStartup;
		}
		set
		{
			immersiveDebuggerDisplayAtStartup = value;
		}
	}

	internal bool EnableOnlyInDebugBuild
	{
		get
		{
			return enableOnlyInDebugBuild;
		}
		set
		{
			enableOnlyInDebugBuild = value;
		}
	}

	internal bool ShowInspectors
	{
		get
		{
			return showInspectors;
		}
		set
		{
			showInspectors = value;
		}
	}

	internal bool ShowConsole
	{
		get
		{
			return showConsole;
		}
		set
		{
			showConsole = value;
		}
	}

	internal bool FollowOverride
	{
		get
		{
			return followOverride;
		}
		set
		{
			followOverride = value;
		}
	}

	internal bool RotateOverride
	{
		get
		{
			return rotateOverride;
		}
		set
		{
			rotateOverride = value;
		}
	}

	internal bool ShowInfoLog
	{
		get
		{
			return showInfoLog;
		}
		set
		{
			showInfoLog = value;
		}
	}

	internal bool ShowWarningLog
	{
		get
		{
			return showWarningLog;
		}
		set
		{
			showWarningLog = value;
		}
	}

	internal bool ShowErrorLog
	{
		get
		{
			return showErrorLog;
		}
		set
		{
			showErrorLog = value;
		}
	}

	internal bool CollapsedIdenticalLogEntries
	{
		get
		{
			return collapsedIdenticalLogEntries;
		}
		set
		{
			collapsedIdenticalLogEntries = value;
		}
	}

	internal int MaximumNumberOfLogEntries
	{
		get
		{
			return maximumNumberOfLogEntries;
		}
		set
		{
			maximumNumberOfLogEntries = value;
		}
	}

	internal DistanceOption PanelDistance
	{
		get
		{
			return panelDistance;
		}
		set
		{
			panelDistance = value;
		}
	}

	internal bool CreateEventSystem
	{
		get
		{
			return createEventSystem;
		}
		set
		{
			createEventSystem = value;
		}
	}

	internal bool AutomaticLayerCullingUpdate
	{
		get
		{
			return automaticLayerCullingUpdate;
		}
		set
		{
			automaticLayerCullingUpdate = value;
		}
	}

	internal int PanelLayer
	{
		get
		{
			return panelLayer;
		}
		set
		{
			panelLayer = value;
		}
	}

	internal int MeshRendererLayer
	{
		get
		{
			return meshRendererLayer;
		}
		set
		{
			meshRendererLayer = value;
		}
	}

	internal int OverlayDepth
	{
		get
		{
			return overlayDepth;
		}
		set
		{
			overlayDepth = value;
		}
	}

	internal bool UseOverlay
	{
		get
		{
			return useOverlay;
		}
		set
		{
			useOverlay = value;
		}
	}

	internal bool ShouldUseOverlay => UseOverlay;

	internal List<bool> InspectedDataEnabled
	{
		get
		{
			return inspectedDataEnabled;
		}
		set
		{
			inspectedDataEnabled = value;
		}
	}

	internal List<InspectedData> InspectedDataAssets
	{
		get
		{
			return inspectedDataAssets;
		}
		set
		{
			inspectedDataAssets = value;
		}
	}

	internal bool UseCustomIntegrationConfig
	{
		get
		{
			return useCustomIntegrationConfig;
		}
		set
		{
			useCustomIntegrationConfig = value;
		}
	}

	internal string CustomIntegrationConfigClassName
	{
		get
		{
			return customIntegrationConfigClassName;
		}
		set
		{
			customIntegrationConfigClassName = value;
		}
	}

	internal bool HierarchyViewShowsPrivateMembers
	{
		get
		{
			return hierarchyViewShowsPrivateMembers;
		}
		set
		{
			hierarchyViewShowsPrivateMembers = value;
		}
	}

	internal OVRInput.Button ClickButton
	{
		get
		{
			return clickButton;
		}
		set
		{
			clickButton = value;
		}
	}

	internal OVRInput.Button ToggleFollowTranslationButton
	{
		get
		{
			return toggleFollowTranslationButton;
		}
		set
		{
			toggleFollowTranslationButton = value;
		}
	}

	internal OVRInput.Button ToggleFollowRotationButton
	{
		get
		{
			return toggleFollowRotationButton;
		}
		set
		{
			toggleFollowRotationButton = value;
		}
	}

	internal OVRInput.Button ImmersiveDebuggerToggleDisplayButton
	{
		get
		{
			return immersiveDebuggerToggleDisplayButton;
		}
		set
		{
			immersiveDebuggerToggleDisplayButton = value;
		}
	}

	internal static event Action OnImmersiveDebuggerEnabledChanged;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_instance = null;
	}

	internal RuntimeSettings()
	{
		debugTypes = new List<DebugData>();
		debugTypesDict = new Dictionary<string, List<string>>();
	}

	public void OnBeforeSerialize()
	{
		debugTypes.Clear();
		foreach (var (assemblyName, types) in debugTypesDict)
		{
			debugTypes.Add(new DebugData(assemblyName, types));
		}
	}

	public void OnAfterDeserialize()
	{
		foreach (DebugData debugType in debugTypes)
		{
			debugTypesDict[debugType.AssemblyName] = debugType.DebugTypes;
		}
	}
}
