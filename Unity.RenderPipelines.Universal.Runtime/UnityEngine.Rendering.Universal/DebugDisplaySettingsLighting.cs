using System;
using System.Reflection;

namespace UnityEngine.Rendering.Universal;

public class DebugDisplaySettingsLighting : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	internal static class Strings
	{
		public static readonly DebugUI.Widget.NameAndTooltip LightingDebugMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Lighting Debug Mode",
			tooltip = "Use the drop-down to select which lighting and shadow debug information to overlay on the screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip LightingFeatures = new DebugUI.Widget.NameAndTooltip
		{
			name = "Lighting Features",
			tooltip = "Filter and debug selected lighting features in the system."
		};

		public static readonly DebugUI.Widget.NameAndTooltip HDRDebugMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "HDR Debug Mode",
			tooltip = "Select which HDR brightness debug information to overlay on the screen."
		};
	}

	internal static class WidgetFactory
	{
		internal static DebugUI.Widget CreateLightingDebugMode(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.LightingDebugMode,
				autoEnum = typeof(DebugLightingMode),
				getter = () => (int)panel.data.lightingDebugMode,
				setter = delegate(int value)
				{
					panel.data.lightingDebugMode = (DebugLightingMode)value;
				},
				getIndex = () => (int)panel.data.lightingDebugMode,
				setIndex = delegate(int value)
				{
					panel.data.lightingDebugMode = (DebugLightingMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateLightingFeatures(SettingsPanel panel)
		{
			return new DebugUI.BitField
			{
				nameAndTooltip = Strings.LightingFeatures,
				getter = () => panel.data.lightingFeatureFlags,
				setter = delegate(Enum value)
				{
					panel.data.lightingFeatureFlags = (DebugLightingFeatureFlags)(object)value;
				},
				enumType = typeof(DebugLightingFeatureFlags)
			};
		}

		internal static DebugUI.Widget CreateHDRDebugMode(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.HDRDebugMode,
				autoEnum = typeof(HDRDebugMode),
				getter = () => (int)panel.data.hdrDebugMode,
				setter = delegate(int value)
				{
					panel.data.hdrDebugMode = (HDRDebugMode)value;
				},
				getIndex = () => (int)panel.data.hdrDebugMode,
				setIndex = delegate(int value)
				{
					panel.data.hdrDebugMode = (HDRDebugMode)value;
				}
			};
		}
	}

	[DisplayInfo(name = "Lighting", order = 3)]
	internal class SettingsPanel : DebugDisplaySettingsPanel<DebugDisplaySettingsLighting>
	{
		public SettingsPanel(DebugDisplaySettingsLighting data)
			: base(data)
		{
			AddWidget(new DebugUI.RuntimeDebugShadersMessageBox());
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Lighting Debug Modes",
				flags = DebugUI.Flags.FrequentlyUsed,
				opened = true,
				children = 
				{
					WidgetFactory.CreateLightingDebugMode(this),
					WidgetFactory.CreateHDRDebugMode(this),
					WidgetFactory.CreateLightingFeatures(this)
				},
				documentationUrl = typeof(DebugDisplaySettingsLighting).GetCustomAttribute<HelpURLAttribute>()?.URL
			});
		}
	}

	public DebugLightingMode lightingDebugMode { get; set; }

	public DebugLightingFeatureFlags lightingFeatureFlags { get; set; }

	public HDRDebugMode hdrDebugMode { get; set; }

	public bool AreAnySettingsActive
	{
		get
		{
			if (lightingDebugMode == DebugLightingMode.None && lightingFeatureFlags == DebugLightingFeatureFlags.None)
			{
				return hdrDebugMode != HDRDebugMode.None;
			}
			return true;
		}
	}

	public bool IsPostProcessingAllowed
	{
		get
		{
			if (lightingDebugMode != DebugLightingMode.Reflections)
			{
				return lightingDebugMode != DebugLightingMode.ReflectionsWithSmoothness;
			}
			return false;
		}
	}

	public bool IsLightingActive => true;

	IDebugDisplaySettingsPanelDisposable IDebugDisplaySettingsData.CreatePanel()
	{
		return new SettingsPanel(this);
	}
}
