namespace UnityEngine.Rendering.Universal;

public class DebugDisplaySettingsMaterial : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	public enum AlbedoDebugValidationPreset
	{
		DefaultLuminance,
		BlackAcrylicPaint,
		DarkSoil,
		WornAsphalt,
		DryClaySoil,
		GreenGrass,
		OldConcrete,
		RedClayTile,
		DrySand,
		NewConcrete,
		WhiteAcrylicPaint,
		FreshSnow,
		BlueSky,
		Foliage,
		Custom
	}

	private struct AlbedoDebugValidationPresetData
	{
		public string name;

		public Color color;

		public float minLuminance;

		public float maxLuminance;
	}

	private static class Strings
	{
		public const string AlbedoSettingsContainerName = "Albedo Settings";

		public const string MetallicSettingsContainerName = "Metallic Settings";

		public const string RenderingLayerMasksSettingsContainerName = "Rendering Layer Masks Settings";

		public static readonly DebugUI.Widget.NameAndTooltip MaterialOverride = new DebugUI.Widget.NameAndTooltip
		{
			name = "Material Override",
			tooltip = "Use the drop-down to select a Material property to visualize on every GameObject on screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip VertexAttribute = new DebugUI.Widget.NameAndTooltip
		{
			name = "Vertex Attribute",
			tooltip = "Use the drop-down to select a 3D GameObject attribute, like Texture Coordinates or Vertex Color, to visualize on screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MaterialValidationMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Material Validation Mode",
			tooltip = "Debug and validate material properties."
		};

		public static readonly DebugUI.Widget.NameAndTooltip RenderingLayersSelectedLight = new DebugUI.Widget.NameAndTooltip
		{
			name = "Filter Rendering Layers by Light",
			tooltip = "Highlight Renderers affected by Selected Light"
		};

		public static readonly DebugUI.Widget.NameAndTooltip SelectedLightShadowLayerMask = new DebugUI.Widget.NameAndTooltip
		{
			name = "Use Light's Shadow Layer Mask",
			tooltip = "Highlight Renderers that cast shadows for the Selected Light"
		};

		public static readonly DebugUI.Widget.NameAndTooltip FilterRenderingLayerMask = new DebugUI.Widget.NameAndTooltip
		{
			name = "Filter Layers",
			tooltip = "Use the dropdown to filter Rendering Layers that you want to visualize"
		};

		public static readonly DebugUI.Widget.NameAndTooltip ValidationPreset = new DebugUI.Widget.NameAndTooltip
		{
			name = "Validation Preset",
			tooltip = "Validate using a list of preset surfaces and inputs based on real-world surfaces."
		};

		public static readonly DebugUI.Widget.NameAndTooltip AlbedoCustomColor = new DebugUI.Widget.NameAndTooltip
		{
			name = "Target Color",
			tooltip = "Custom target color for albedo validation."
		};

		public static readonly DebugUI.Widget.NameAndTooltip AlbedoMinLuminance = new DebugUI.Widget.NameAndTooltip
		{
			name = "Min Luminance",
			tooltip = "Any values set below this field are invalid and appear red on screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip AlbedoMaxLuminance = new DebugUI.Widget.NameAndTooltip
		{
			name = "Max Luminance",
			tooltip = "Any values set above this field are invalid and appear blue on screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip AlbedoHueTolerance = new DebugUI.Widget.NameAndTooltip
		{
			name = "Hue Tolerance",
			tooltip = "Validate a material based on a specific hue."
		};

		public static readonly DebugUI.Widget.NameAndTooltip AlbedoSaturationTolerance = new DebugUI.Widget.NameAndTooltip
		{
			name = "Saturation Tolerance",
			tooltip = "Validate a material based on a specific Saturation."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MetallicMinValue = new DebugUI.Widget.NameAndTooltip
		{
			name = "Min Value",
			tooltip = "Any values set below this field are invalid and appear red on screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MetallicMaxValue = new DebugUI.Widget.NameAndTooltip
		{
			name = "Max Value",
			tooltip = "Any values set above this field are invalid and appear blue on screen."
		};
	}

	internal static class WidgetFactory
	{
		internal static DebugUI.Widget CreateMaterialOverride(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.MaterialOverride,
				autoEnum = typeof(DebugMaterialMode),
				getter = () => (int)panel.data.materialDebugMode,
				setter = delegate(int value)
				{
					panel.data.materialDebugMode = (DebugMaterialMode)value;
				},
				getIndex = () => (int)panel.data.materialDebugMode,
				setIndex = delegate(int value)
				{
					panel.data.materialDebugMode = (DebugMaterialMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateVertexAttribute(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.VertexAttribute,
				autoEnum = typeof(DebugVertexAttributeMode),
				getter = () => (int)panel.data.vertexAttributeDebugMode,
				setter = delegate(int value)
				{
					panel.data.vertexAttributeDebugMode = (DebugVertexAttributeMode)value;
				},
				getIndex = () => (int)panel.data.vertexAttributeDebugMode,
				setIndex = delegate(int value)
				{
					panel.data.vertexAttributeDebugMode = (DebugVertexAttributeMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateMaterialValidationMode(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.MaterialValidationMode,
				autoEnum = typeof(DebugMaterialValidationMode),
				getter = () => (int)panel.data.materialValidationMode,
				setter = delegate(int value)
				{
					panel.data.materialValidationMode = (DebugMaterialValidationMode)value;
				},
				getIndex = () => (int)panel.data.materialValidationMode,
				setIndex = delegate(int value)
				{
					panel.data.materialValidationMode = (DebugMaterialValidationMode)value;
				},
				onValueChanged = delegate
				{
					DebugManager.instance.ReDrawOnScreenDebug();
				}
			};
		}

		internal static DebugUI.Widget CreateRenderingLayersSelectedLight(SettingsPanel panel)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.RenderingLayersSelectedLight,
				getter = () => panel.data.renderingLayersSelectedLight,
				setter = delegate(bool value)
				{
					panel.data.renderingLayersSelectedLight = value;
				},
				flags = DebugUI.Flags.EditorOnly
			};
		}

		internal static DebugUI.Widget CreateSelectedLightShadowLayerMask(SettingsPanel panel)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.SelectedLightShadowLayerMask,
				getter = () => panel.data.selectedLightShadowLayerMask,
				setter = delegate(bool value)
				{
					panel.data.selectedLightShadowLayerMask = value;
				},
				flags = DebugUI.Flags.EditorOnly,
				isHiddenCallback = () => !panel.data.renderingLayersSelectedLight
			};
		}

		internal static DebugUI.RenderingLayerField CreateFilterRenderingLayerMasks(SettingsPanel panel)
		{
			return new DebugUI.RenderingLayerField
			{
				nameAndTooltip = Strings.FilterRenderingLayerMask,
				getter = () => panel.data.renderingLayerMask,
				setter = delegate(RenderingLayerMask value)
				{
					panel.data.renderingLayerMask = value;
				},
				getRenderingLayerColor = (int index) => panel.data.debugRenderingLayersColors[index],
				setRenderingLayerColor = delegate(Vector4 value, int index)
				{
					panel.data.debugRenderingLayersColors[index] = value;
				},
				isHiddenCallback = () => panel.data.renderingLayersSelectedLight
			};
		}

		internal static DebugUI.Widget CreateAlbedoPreset(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.ValidationPreset,
				autoEnum = typeof(AlbedoDebugValidationPreset),
				getter = () => (int)panel.data.albedoValidationPreset,
				setter = delegate(int value)
				{
					panel.data.albedoValidationPreset = (AlbedoDebugValidationPreset)value;
				},
				getIndex = () => (int)panel.data.albedoValidationPreset,
				setIndex = delegate(int value)
				{
					panel.data.albedoValidationPreset = (AlbedoDebugValidationPreset)value;
				},
				onValueChanged = delegate
				{
					DebugManager.instance.ReDrawOnScreenDebug();
				}
			};
		}

		internal static DebugUI.Widget CreateAlbedoCustomColor(SettingsPanel panel)
		{
			return new DebugUI.ColorField
			{
				nameAndTooltip = Strings.AlbedoCustomColor,
				getter = () => panel.data.albedoCompareColor,
				setter = delegate(Color value)
				{
					panel.data.albedoCompareColor = value;
				},
				isHiddenCallback = () => panel.data.albedoValidationPreset != AlbedoDebugValidationPreset.Custom
			};
		}

		internal static DebugUI.Widget CreateAlbedoMinLuminance(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.AlbedoMinLuminance,
				getter = () => panel.data.albedoMinLuminance,
				setter = delegate(float value)
				{
					panel.data.albedoMinLuminance = value;
				},
				incStep = 0.01f
			};
		}

		internal static DebugUI.Widget CreateAlbedoMaxLuminance(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.AlbedoMaxLuminance,
				getter = () => panel.data.albedoMaxLuminance,
				setter = delegate(float value)
				{
					panel.data.albedoMaxLuminance = value;
				},
				incStep = 0.01f
			};
		}

		internal static DebugUI.Widget CreateAlbedoHueTolerance(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.AlbedoHueTolerance,
				getter = () => panel.data.albedoHueTolerance,
				setter = delegate(float value)
				{
					panel.data.albedoHueTolerance = value;
				},
				incStep = 0.01f,
				isHiddenCallback = () => panel.data.albedoValidationPreset == AlbedoDebugValidationPreset.DefaultLuminance
			};
		}

		internal static DebugUI.Widget CreateAlbedoSaturationTolerance(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.AlbedoSaturationTolerance,
				getter = () => panel.data.albedoSaturationTolerance,
				setter = delegate(float value)
				{
					panel.data.albedoSaturationTolerance = value;
				},
				incStep = 0.01f,
				isHiddenCallback = () => panel.data.albedoValidationPreset == AlbedoDebugValidationPreset.DefaultLuminance
			};
		}

		internal static DebugUI.Widget CreateMetallicMinValue(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.MetallicMinValue,
				getter = () => panel.data.metallicMinValue,
				setter = delegate(float value)
				{
					panel.data.metallicMinValue = value;
				},
				incStep = 0.01f
			};
		}

		internal static DebugUI.Widget CreateMetallicMaxValue(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.MetallicMaxValue,
				getter = () => panel.data.metallicMaxValue,
				setter = delegate(float value)
				{
					panel.data.metallicMaxValue = value;
				},
				incStep = 0.01f
			};
		}
	}

	[DisplayInfo(name = "Material", order = 2)]
	internal class SettingsPanel : DebugDisplaySettingsPanel<DebugDisplaySettingsMaterial>
	{
		public SettingsPanel(DebugDisplaySettingsMaterial data)
			: base(data)
		{
			AddWidget(new DebugUI.RuntimeDebugShadersMessageBox());
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Material Filters",
				flags = DebugUI.Flags.FrequentlyUsed,
				opened = true,
				children = 
				{
					WidgetFactory.CreateMaterialOverride(this),
					(DebugUI.Widget)new DebugUI.Container
					{
						displayName = "Rendering Layer Masks Settings",
						isHiddenCallback = () => data.materialDebugMode != DebugMaterialMode.RenderingLayerMasks,
						children = 
						{
							WidgetFactory.CreateRenderingLayersSelectedLight(this),
							WidgetFactory.CreateSelectedLightShadowLayerMask(this),
							(DebugUI.Widget)WidgetFactory.CreateFilterRenderingLayerMasks(this)
						}
					},
					WidgetFactory.CreateVertexAttribute(this)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Material Validation",
				opened = true,
				children = 
				{
					WidgetFactory.CreateMaterialValidationMode(this),
					(DebugUI.Widget)new DebugUI.Container
					{
						displayName = "Albedo Settings",
						isHiddenCallback = () => data.materialValidationMode != DebugMaterialValidationMode.Albedo,
						children = 
						{
							WidgetFactory.CreateAlbedoPreset(this),
							WidgetFactory.CreateAlbedoCustomColor(this),
							WidgetFactory.CreateAlbedoMinLuminance(this),
							WidgetFactory.CreateAlbedoMaxLuminance(this),
							WidgetFactory.CreateAlbedoHueTolerance(this),
							WidgetFactory.CreateAlbedoSaturationTolerance(this)
						}
					},
					(DebugUI.Widget)new DebugUI.Container
					{
						displayName = "Metallic Settings",
						isHiddenCallback = () => data.materialValidationMode != DebugMaterialValidationMode.Metallic,
						children = 
						{
							WidgetFactory.CreateMetallicMinValue(this),
							WidgetFactory.CreateMetallicMaxValue(this)
						}
					}
				}
			});
		}
	}

	private AlbedoDebugValidationPresetData[] m_AlbedoDebugValidationPresetData = new AlbedoDebugValidationPresetData[15]
	{
		new AlbedoDebugValidationPresetData
		{
			name = "Default Luminance",
			color = new Color(0.49803922f, 0.49803922f, 0.49803922f),
			minLuminance = 0.01f,
			maxLuminance = 0.9f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Black Acrylic Paint",
			color = new Color(0.21960784f, 0.21960784f, 0.21960784f),
			minLuminance = 0.03f,
			maxLuminance = 0.07f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Dark Soil",
			color = new Color(1f / 3f, 0.23921569f, 0.19215687f),
			minLuminance = 0.05f,
			maxLuminance = 0.14f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Worn Asphalt",
			color = new Color(0.35686275f, 0.35686275f, 0.35686275f),
			minLuminance = 0.1f,
			maxLuminance = 0.15f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Dry Clay Soil",
			color = new Color(0.5372549f, 0.47058824f, 0.4f),
			minLuminance = 0.15f,
			maxLuminance = 0.35f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Green Grass",
			color = new Color(41f / 85f, 0.5137255f, 0.2901961f),
			minLuminance = 0.16f,
			maxLuminance = 0.26f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Old Concrete",
			color = new Color(0.5294118f, 8f / 15f, 0.5137255f),
			minLuminance = 0.17f,
			maxLuminance = 0.3f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Red Clay Tile",
			color = new Color(0.77254903f, 25f / 51f, 20f / 51f),
			minLuminance = 0.23f,
			maxLuminance = 0.33f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Dry Sand",
			color = new Color(59f / 85f, 0.654902f, 44f / 85f),
			minLuminance = 0.2f,
			maxLuminance = 0.45f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "New Concrete",
			color = new Color(37f / 51f, 0.7137255f, 35f / 51f),
			minLuminance = 0.32f,
			maxLuminance = 0.55f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "White Acrylic Paint",
			color = new Color(0.8901961f, 0.8901961f, 0.8901961f),
			minLuminance = 0.75f,
			maxLuminance = 0.85f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Fresh Snow",
			color = new Color(81f / 85f, 81f / 85f, 81f / 85f),
			minLuminance = 0.85f,
			maxLuminance = 0.95f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Blue Sky",
			color = new Color(31f / 85f, 41f / 85f, 0.6156863f),
			minLuminance = new Color(31f / 85f, 41f / 85f, 0.6156863f).linear.maxColorComponent - 0.05f,
			maxLuminance = new Color(31f / 85f, 41f / 85f, 0.6156863f).linear.maxColorComponent + 0.05f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Foliage",
			color = new Color(0.35686275f, 36f / 85f, 13f / 51f),
			minLuminance = new Color(0.35686275f, 36f / 85f, 13f / 51f).linear.maxColorComponent - 0.05f,
			maxLuminance = new Color(0.35686275f, 36f / 85f, 13f / 51f).linear.maxColorComponent + 0.05f
		},
		new AlbedoDebugValidationPresetData
		{
			name = "Custom",
			color = new Color(0.49803922f, 0.49803922f, 0.49803922f),
			minLuminance = 0.01f,
			maxLuminance = 0.9f
		}
	};

	private AlbedoDebugValidationPreset m_AlbedoValidationPreset;

	private float m_AlbedoHueTolerance = 0.104f;

	private float m_AlbedoSaturationTolerance = 0.214f;

	public Vector4[] debugRenderingLayersColors = new Vector4[32]
	{
		new Vector4(230f, 159f, 0f) / 255f,
		new Vector4(86f, 180f, 233f) / 255f,
		new Vector4(255f, 182f, 291f) / 255f,
		new Vector4(0f, 158f, 115f) / 255f,
		new Vector4(240f, 228f, 66f) / 255f,
		new Vector4(0f, 114f, 178f) / 255f,
		new Vector4(213f, 94f, 0f) / 255f,
		new Vector4(170f, 68f, 170f) / 255f,
		new Vector4(1f, 0.5f, 0.5f),
		new Vector4(0.5f, 1f, 0.5f),
		new Vector4(0.5f, 0.5f, 1f),
		new Vector4(0.5f, 1f, 1f),
		new Vector4(0.75f, 0.25f, 1f),
		new Vector4(0.25f, 1f, 0.75f),
		new Vector4(0.25f, 0.25f, 0.75f),
		new Vector4(0.75f, 0.25f, 0.25f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f),
		new Vector4(0f, 0f, 0f)
	};

	public AlbedoDebugValidationPreset albedoValidationPreset
	{
		get
		{
			return m_AlbedoValidationPreset;
		}
		set
		{
			m_AlbedoValidationPreset = value;
			AlbedoDebugValidationPresetData albedoDebugValidationPresetData = m_AlbedoDebugValidationPresetData[(int)value];
			albedoMinLuminance = albedoDebugValidationPresetData.minLuminance;
			albedoMaxLuminance = albedoDebugValidationPresetData.maxLuminance;
			albedoCompareColor = albedoDebugValidationPresetData.color;
		}
	}

	public float albedoMinLuminance { get; set; } = 0.01f;


	public float albedoMaxLuminance { get; set; } = 0.9f;


	public float albedoHueTolerance
	{
		get
		{
			if (m_AlbedoValidationPreset != 0)
			{
				return m_AlbedoHueTolerance;
			}
			return 1f;
		}
		set
		{
			m_AlbedoHueTolerance = value;
		}
	}

	public float albedoSaturationTolerance
	{
		get
		{
			if (m_AlbedoValidationPreset != 0)
			{
				return m_AlbedoSaturationTolerance;
			}
			return 1f;
		}
		set
		{
			m_AlbedoSaturationTolerance = value;
		}
	}

	public Color albedoCompareColor { get; set; } = new Color(0.49803922f, 0.49803922f, 0.49803922f, 1f);


	public float metallicMinValue { get; set; }

	public float metallicMaxValue { get; set; } = 0.9f;


	public bool renderingLayersSelectedLight { get; set; }

	public bool selectedLightShadowLayerMask { get; set; }

	public uint renderingLayerMask { get; set; }

	public DebugMaterialValidationMode materialValidationMode { get; set; }

	public DebugMaterialMode materialDebugMode { get; set; }

	public DebugVertexAttributeMode vertexAttributeDebugMode { get; set; }

	public bool AreAnySettingsActive
	{
		get
		{
			if (materialDebugMode == DebugMaterialMode.None && vertexAttributeDebugMode == DebugVertexAttributeMode.None)
			{
				return materialValidationMode != DebugMaterialValidationMode.None;
			}
			return true;
		}
	}

	public bool IsPostProcessingAllowed => !AreAnySettingsActive;

	public bool IsLightingActive => !AreAnySettingsActive;

	public uint GetDebugLightLayersMask()
	{
		return 65535u;
	}

	IDebugDisplaySettingsPanelDisposable IDebugDisplaySettingsData.CreatePanel()
	{
		return new SettingsPanel(this);
	}
}
