using System.Collections.Generic;

namespace UnityEngine.Rendering.RenderGraphModule;

internal class RenderGraphDebugParams : IDebugDisplaySettingsQuery
{
	private static class Strings
	{
		public static readonly DebugUI.Widget.NameAndTooltip ClearRenderTargetsAtCreation = new DebugUI.Widget.NameAndTooltip
		{
			name = "Clear Render Targets At Creation",
			tooltip = "Enable to clear all render textures before any rendergraph passes to check if some clears are missing."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ClearRenderTargetsAtFree = new DebugUI.Widget.NameAndTooltip
		{
			name = "Clear Render Targets When Freed",
			tooltip = "Enable to clear all render textures when textures are freed by the graph to detect use after free of textures."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DisablePassCulling = new DebugUI.Widget.NameAndTooltip
		{
			name = "Disable Pass Culling",
			tooltip = "Enable to temporarily disable culling to assess if a pass is culled."
		};

		public static readonly DebugUI.Widget.NameAndTooltip DisablePassMerging = new DebugUI.Widget.NameAndTooltip
		{
			name = "Disable Pass Merging",
			tooltip = "Enable to temporarily disable pass merging to diagnose issues or analyze performance."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ImmediateMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Immediate Mode",
			tooltip = "Enable to force render graph to execute all passes in the order you registered them."
		};

		public static readonly DebugUI.Widget.NameAndTooltip EnableLogging = new DebugUI.Widget.NameAndTooltip
		{
			name = "Enable Logging",
			tooltip = "Enable to allow HDRP to capture information in the log."
		};

		public static readonly DebugUI.Widget.NameAndTooltip LogFrameInformation = new DebugUI.Widget.NameAndTooltip
		{
			name = "Log Frame Information",
			tooltip = "Enable to log information output from each frame."
		};

		public static readonly DebugUI.Widget.NameAndTooltip LogResources = new DebugUI.Widget.NameAndTooltip
		{
			name = "Log Resources",
			tooltip = "Enable to log the current render graph's global resource usage."
		};
	}

	private DebugUI.Widget[] m_DebugItems;

	private DebugUI.Panel m_DebugPanel;

	public bool clearRenderTargetsAtCreation;

	public bool clearRenderTargetsAtRelease;

	public bool disablePassCulling;

	public bool disablePassMerging;

	public bool immediateMode;

	public bool logFrameInformation;

	public bool logResources;

	public bool enableLogging
	{
		get
		{
			if (!logFrameInformation)
			{
				return logResources;
			}
			return true;
		}
	}

	public bool AreAnySettingsActive
	{
		get
		{
			if (!clearRenderTargetsAtCreation && !clearRenderTargetsAtRelease && !disablePassCulling && !disablePassMerging && !immediateMode)
			{
				return enableLogging;
			}
			return true;
		}
	}

	public void ResetLogging()
	{
		logFrameInformation = false;
		logResources = false;
	}

	internal void Reset()
	{
		clearRenderTargetsAtCreation = false;
		clearRenderTargetsAtRelease = false;
		disablePassCulling = false;
		disablePassMerging = false;
		immediateMode = false;
		ResetLogging();
	}

	internal List<DebugUI.Widget> GetWidgetList(string name)
	{
		return new List<DebugUI.Widget>
		{
			new DebugUI.Container
			{
				displayName = name + " Render Graph",
				children = 
				{
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.ClearRenderTargetsAtCreation,
						getter = () => clearRenderTargetsAtCreation,
						setter = delegate(bool value)
						{
							clearRenderTargetsAtCreation = value;
						}
					},
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.ClearRenderTargetsAtFree,
						getter = () => clearRenderTargetsAtRelease,
						setter = delegate(bool value)
						{
							clearRenderTargetsAtRelease = value;
						}
					},
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.DisablePassCulling,
						getter = () => disablePassCulling,
						setter = delegate(bool value)
						{
							disablePassCulling = value;
						}
					},
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.DisablePassMerging,
						getter = () => disablePassMerging,
						setter = delegate(bool value)
						{
							disablePassMerging = value;
						},
						isHiddenCallback = () => !RenderGraph.hasAnyRenderGraphWithNativeRenderPassesEnabled
					},
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.ImmediateMode,
						getter = () => immediateMode,
						setter = delegate(bool value)
						{
							immediateMode = value;
						},
						isHiddenCallback = () => !IsImmediateModeSupported()
					},
					(DebugUI.Widget)new DebugUI.Button
					{
						nameAndTooltip = Strings.LogFrameInformation,
						action = delegate
						{
							logFrameInformation = true;
						}
					},
					(DebugUI.Widget)new DebugUI.Button
					{
						nameAndTooltip = Strings.LogResources,
						action = delegate
						{
							logResources = true;
						}
					}
				}
			}
		};
	}

	private bool IsImmediateModeSupported()
	{
		if (GraphicsSettings.currentRenderPipeline is IRenderGraphEnabledRenderPipeline renderGraphEnabledRenderPipeline)
		{
			return renderGraphEnabledRenderPipeline.isImmediateModeSupported;
		}
		return false;
	}

	public void RegisterDebug(string name, DebugUI.Panel debugPanel = null)
	{
		List<DebugUI.Widget> widgetList = GetWidgetList(name);
		m_DebugItems = widgetList.ToArray();
		m_DebugPanel = ((debugPanel != null) ? debugPanel : DebugManager.instance.GetPanel((name.Length == 0) ? "Rendering" : name, createIfNull: true));
		DebugUI.Foldout foldout = new DebugUI.Foldout
		{
			displayName = name
		};
		foldout.children.Add(m_DebugItems);
		m_DebugPanel.children.Add(foldout);
	}

	public void UnRegisterDebug(string name)
	{
		if (m_DebugPanel != null)
		{
			m_DebugPanel.children.Remove(m_DebugItems);
		}
		m_DebugPanel = null;
		m_DebugItems = null;
	}
}
