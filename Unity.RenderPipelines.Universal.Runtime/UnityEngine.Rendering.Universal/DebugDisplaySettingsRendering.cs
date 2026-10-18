using System;

namespace UnityEngine.Rendering.Universal;

public class DebugDisplaySettingsRendering : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	public enum TaaDebugMode
	{
		None,
		ShowRawFrame,
		ShowRawFrameNoJitter,
		ShowClampedHistory
	}

	private static class Strings
	{
		public const string RangeValidationSettingsContainerName = "Pixel Range Settings";

		public static readonly DebugUI.Widget.NameAndTooltip MapOverlays = new DebugUI.Widget.NameAndTooltip
		{
			name = "Map Overlays",
			tooltip = "Overlays render pipeline textures to validate the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip StpDebugViews = new DebugUI.Widget.NameAndTooltip
		{
			name = "STP Debug Views",
			tooltip = "Debug visualizations provided by STP."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MapSize = new DebugUI.Widget.NameAndTooltip
		{
			name = "Map Size",
			tooltip = "Set the size of the render pipeline texture in the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip AdditionalWireframeModes = new DebugUI.Widget.NameAndTooltip
		{
			name = "Additional Wireframe Modes",
			tooltip = "Debug the scene with additional wireframe shader views that are different from those in the scene view."
		};

		public static readonly DebugUI.Widget.NameAndTooltip WireframeNotSupportedWarning = new DebugUI.Widget.NameAndTooltip
		{
			name = "Warning: This platform might not support wireframe rendering.",
			tooltip = "Some platforms, for example, mobile platforms using OpenGL ES and Vulkan, might not support wireframe rendering."
		};

		public static readonly DebugUI.Widget.NameAndTooltip OverdrawMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Overdraw Mode",
			tooltip = "Debug anywhere materials that overdrawn pixels top of each other."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MaxOverdrawCount = new DebugUI.Widget.NameAndTooltip
		{
			name = "Max Overdraw Count",
			tooltip = "Maximum overdraw count allowed for a single pixel."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MipMapDisableMipCaching = new DebugUI.Widget.NameAndTooltip
		{
			name = "Disable Mip Caching",
			tooltip = "By disabling mip caching, the data on GPU accurately reflects what the TextureStreamer calculates. While this can significantly increase CPU-to-GPU traffic, it can be an invaluable tool to validate that the Streamer behaves as expected."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MipMapDebugView = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug View",
			tooltip = "Use the drop-down to select a mipmap property to debug."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MipMapDebugOpacity = new DebugUI.Widget.NameAndTooltip
		{
			name = "Debug Opacity",
			tooltip = "Opacity of texture mipmap streaming debug colors."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MipMapMaterialTextureSlot = new DebugUI.Widget.NameAndTooltip
		{
			name = "Material Texture Slot",
			tooltip = "Use the drop-down to select the material texture slot to debug (does not affect terrain).\n\nThe slot indices follow the default order by which texture properties appear in the Material Inspector.\nThe default order is itself defined by the order in which (non-hidden) texture properties appear in the shader's \"Properties\" block."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MipMapTerrainTexture = new DebugUI.Widget.NameAndTooltip
		{
			name = "Terrain Texture",
			tooltip = "Use the drop-down to select the terrain Texture to debug the mipmap for."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MipMapDisplayStatusCodes = new DebugUI.Widget.NameAndTooltip
		{
			name = "Display Status Codes",
			tooltip = "Show detailed status codes indicating why textures are not streaming or highlighting points of attention."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MipMapActivityTimespan = new DebugUI.Widget.NameAndTooltip
		{
			name = "Activity Timespan",
			tooltip = "How long a texture should be shown as \"recently updated\"."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MipMapCombinePerMaterial = new DebugUI.Widget.NameAndTooltip
		{
			name = "Combined per Material",
			tooltip = "Combine the information over all slots per material."
		};

		public static readonly DebugUI.Widget.NameAndTooltip PostProcessing = new DebugUI.Widget.NameAndTooltip
		{
			name = "Post-processing",
			tooltip = "Override the controls for Post Processing in the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip MSAA = new DebugUI.Widget.NameAndTooltip
		{
			name = "MSAA",
			tooltip = "Use the checkbox to disable MSAA in the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip HDR = new DebugUI.Widget.NameAndTooltip
		{
			name = "HDR",
			tooltip = "Use the checkbox to disable High Dynamic Range in the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip TaaDebugMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "TAA Debug Mode",
			tooltip = "Choose whether to force TAA to output the raw jittered frame or clamped reprojected history."
		};

		public static readonly DebugUI.Widget.NameAndTooltip PixelValidationMode = new DebugUI.Widget.NameAndTooltip
		{
			name = "Pixel Validation Mode",
			tooltip = "Choose between modes that validate pixel on screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip Channels = new DebugUI.Widget.NameAndTooltip
		{
			name = "Channels",
			tooltip = "Choose the texture channel used to validate the scene."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ValueRangeMin = new DebugUI.Widget.NameAndTooltip
		{
			name = "Value Range Min",
			tooltip = "Any values set below this field will be considered invalid and will appear red on screen."
		};

		public static readonly DebugUI.Widget.NameAndTooltip ValueRangeMax = new DebugUI.Widget.NameAndTooltip
		{
			name = "Value Range Max",
			tooltip = "Any values set above this field will be considered invalid and will appear blue on screen."
		};
	}

	internal static class WidgetFactory
	{
		internal static DebugUI.Widget CreateMapOverlays(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.MapOverlays,
				autoEnum = typeof(DebugFullScreenMode),
				getter = () => (int)panel.data.fullScreenDebugMode,
				setter = delegate(int value)
				{
					panel.data.fullScreenDebugMode = (DebugFullScreenMode)value;
				},
				getIndex = () => (int)panel.data.fullScreenDebugMode,
				setIndex = delegate(int value)
				{
					panel.data.fullScreenDebugMode = (DebugFullScreenMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateStpDebugViews(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.StpDebugViews,
				isHiddenCallback = () => panel.data.fullScreenDebugMode != DebugFullScreenMode.STP,
				enumNames = STP.debugViewDescriptions,
				enumValues = STP.debugViewIndices,
				getter = () => panel.data.stpDebugViewIndex,
				setter = delegate(int value)
				{
					panel.data.stpDebugViewIndex = value;
				},
				getIndex = () => panel.data.stpDebugViewIndex,
				setIndex = delegate(int value)
				{
					panel.data.stpDebugViewIndex = value;
				}
			};
		}

		internal static DebugUI.Widget CreateMapOverlaySize(SettingsPanel panel)
		{
			return new DebugUI.Container
			{
				children = { (DebugUI.Widget)new DebugUI.IntField
				{
					nameAndTooltip = Strings.MapSize,
					getter = () => panel.data.fullScreenDebugModeOutputSizeScreenPercent,
					setter = delegate(int value)
					{
						panel.data.fullScreenDebugModeOutputSizeScreenPercent = value;
					},
					incStep = 10,
					min = () => 0,
					max = () => 100
				} }
			};
		}

		internal static DebugUI.Widget CreateAdditionalWireframeShaderViews(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.AdditionalWireframeModes,
				autoEnum = typeof(DebugWireframeMode),
				getter = () => (int)panel.data.wireframeMode,
				setter = delegate(int value)
				{
					panel.data.wireframeMode = (DebugWireframeMode)value;
				},
				getIndex = () => (int)panel.data.wireframeMode,
				setIndex = delegate(int value)
				{
					panel.data.wireframeMode = (DebugWireframeMode)value;
				},
				onValueChanged = delegate
				{
					DebugManager.instance.ReDrawOnScreenDebug();
				}
			};
		}

		internal static DebugUI.Widget CreateWireframeNotSupportedWarning(SettingsPanel panel)
		{
			return new DebugUI.MessageBox
			{
				nameAndTooltip = Strings.WireframeNotSupportedWarning,
				style = DebugUI.MessageBox.Style.Warning,
				isHiddenCallback = delegate
				{
					GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
					return (graphicsDeviceType != GraphicsDeviceType.OpenGLES3 && graphicsDeviceType != GraphicsDeviceType.Vulkan) || panel.data.wireframeMode == DebugWireframeMode.None;
				}
			};
		}

		internal static DebugUI.Widget CreateOverdrawMode(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.OverdrawMode,
				autoEnum = typeof(DebugOverdrawMode),
				getter = () => (int)panel.data.overdrawMode,
				setter = delegate(int value)
				{
					panel.data.overdrawMode = (DebugOverdrawMode)value;
				},
				getIndex = () => (int)panel.data.overdrawMode,
				setIndex = delegate(int value)
				{
					panel.data.overdrawMode = (DebugOverdrawMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateMaxOverdrawCount(SettingsPanel panel)
		{
			return new DebugUI.Container
			{
				isHiddenCallback = () => panel.data.overdrawMode == DebugOverdrawMode.None,
				children = { (DebugUI.Widget)new DebugUI.IntField
				{
					nameAndTooltip = Strings.MaxOverdrawCount,
					getter = () => panel.data.maxOverdrawCount,
					setter = delegate(int value)
					{
						panel.data.maxOverdrawCount = value;
					},
					incStep = 10,
					min = () => 1,
					max = () => 500
				} }
			};
		}

		internal static DebugUI.Widget CreateMipMapDebugWidget(SettingsPanel panel)
		{
			return new DebugUI.Container
			{
				displayName = "Mipmap Streaming",
				children = 
				{
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.MipMapDisableMipCaching,
						getter = () => Texture.streamingTextureDiscardUnusedMips,
						setter = delegate(bool value)
						{
							Texture.streamingTextureDiscardUnusedMips = value;
						}
					},
					CreateMipMapMode(panel),
					CreateMipMapDebugSettings(panel)
				}
			};
		}

		internal static DebugUI.Widget CreateMipMapMode(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.MipMapDebugView,
				autoEnum = typeof(DebugMipInfoMode),
				getter = () => (int)panel.data.mipInfoMode,
				setter = delegate(int value)
				{
					panel.data.mipInfoMode = (DebugMipInfoMode)value;
				},
				getIndex = () => (int)panel.data.mipInfoMode,
				setIndex = delegate(int value)
				{
					panel.data.mipInfoMode = (DebugMipInfoMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateMipMapDebugSettings(SettingsPanel panel)
		{
			GUIContent[] array = new GUIContent[64];
			int[] array2 = new int[64];
			for (int i = 0; i < 64; i++)
			{
				array[i] = new GUIContent($"Slot {i}");
				array2[i] = i;
			}
			return new DebugUI.Container
			{
				isHiddenCallback = () => panel.data.mipInfoMode == DebugMipInfoMode.None,
				children = 
				{
					(DebugUI.Widget)new DebugUI.FloatField
					{
						nameAndTooltip = Strings.MipMapDebugOpacity,
						getter = () => panel.data.mipDebugOpacity,
						setter = delegate(float value)
						{
							panel.data.mipDebugOpacity = value;
						},
						min = () => 0f,
						max = () => 1f
					},
					CreateMipMapDebugSlotSelector(panel, () => panel.data.canAggregateData, array, array2),
					(DebugUI.Widget)new DebugUI.BoolField
					{
						isHiddenCallback = () => !panel.data.canAggregateData,
						nameAndTooltip = Strings.MipMapCombinePerMaterial,
						getter = () => panel.data.showInfoForAllSlots,
						setter = delegate(bool value)
						{
							panel.data.showInfoForAllSlots = value;
							panel.data.mipDebugStatusMode = ((!value) ? DebugMipMapStatusMode.Texture : DebugMipMapStatusMode.Material);
						}
					},
					(DebugUI.Widget)new DebugUI.Container
					{
						isHiddenCallback = () => !panel.data.canAggregateData || panel.data.showInfoForAllSlots,
						children = 
						{
							CreateMipMapDebugSlotSelector(panel, () => false, array, array2),
							CreateMipMapShowStatusCodeToggle(panel)
						}
					},
					(DebugUI.Widget)new DebugUI.EnumField
					{
						nameAndTooltip = Strings.MipMapTerrainTexture,
						getter = () => (int)panel.data.mipDebugTerrainTexture,
						setter = delegate(int value)
						{
							panel.data.mipDebugTerrainTexture = (DebugMipMapModeTerrainTexture)value;
						},
						autoEnum = typeof(DebugMipMapModeTerrainTexture),
						getIndex = () => (int)panel.data.mipDebugTerrainTexture,
						setIndex = delegate(int value)
						{
							panel.data.mipDebugTerrainTexture = (DebugMipMapModeTerrainTexture)value;
						}
					},
					CreateMipMapDebugCooldownSlider(panel)
				}
			};
		}

		internal static DebugUI.Widget CreateMipMapDebugSlotSelector(SettingsPanel panel, Func<bool> hiddenCB, GUIContent[] texSlotStrings, int[] texSlotValues)
		{
			return new DebugUI.EnumField
			{
				isHiddenCallback = hiddenCB,
				nameAndTooltip = Strings.MipMapMaterialTextureSlot,
				getter = () => panel.data.mipDebugMaterialTextureSlot,
				setter = delegate(int value)
				{
					panel.data.mipDebugMaterialTextureSlot = value;
				},
				getIndex = () => panel.data.mipDebugMaterialTextureSlot,
				setIndex = delegate(int value)
				{
					panel.data.mipDebugMaterialTextureSlot = value;
				},
				enumNames = texSlotStrings,
				enumValues = texSlotValues
			};
		}

		internal static DebugUI.Widget CreateMipMapDebugCooldownSlider(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				isHiddenCallback = () => panel.data.mipInfoMode != DebugMipInfoMode.MipStreamingActivity,
				nameAndTooltip = Strings.MipMapActivityTimespan,
				getter = () => panel.data.mipDebugRecentUpdateCooldown,
				setter = delegate(float value)
				{
					panel.data.mipDebugRecentUpdateCooldown = value;
				},
				min = () => 0f,
				max = () => 60f
			};
		}

		internal static DebugUI.Widget CreateMipMapShowStatusCodeToggle(SettingsPanel panel)
		{
			return new DebugUI.BoolField
			{
				isHiddenCallback = () => panel.data.mipInfoMode != DebugMipInfoMode.MipStreamingStatus,
				nameAndTooltip = Strings.MipMapDisplayStatusCodes,
				getter = () => panel.data.mipDebugStatusShowCode,
				setter = delegate(bool value)
				{
					panel.data.mipDebugStatusShowCode = value;
				}
			};
		}

		internal static DebugUI.Widget CreatePostProcessing(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.PostProcessing,
				autoEnum = typeof(DebugPostProcessingMode),
				getter = () => (int)panel.data.postProcessingDebugMode,
				setter = delegate(int value)
				{
					panel.data.postProcessingDebugMode = (DebugPostProcessingMode)value;
				},
				getIndex = () => (int)panel.data.postProcessingDebugMode,
				setIndex = delegate(int value)
				{
					panel.data.postProcessingDebugMode = (DebugPostProcessingMode)value;
				}
			};
		}

		internal static DebugUI.Widget CreateMSAA(SettingsPanel panel)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.MSAA,
				getter = () => panel.data.enableMsaa,
				setter = delegate(bool value)
				{
					panel.data.enableMsaa = value;
				}
			};
		}

		internal static DebugUI.Widget CreateHDR(SettingsPanel panel)
		{
			return new DebugUI.BoolField
			{
				nameAndTooltip = Strings.HDR,
				getter = () => panel.data.enableHDR,
				setter = delegate(bool value)
				{
					panel.data.enableHDR = value;
				}
			};
		}

		internal static DebugUI.Widget CreateTaaDebugMode(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.TaaDebugMode,
				autoEnum = typeof(TaaDebugMode),
				getter = () => (int)panel.data.taaDebugMode,
				setter = delegate(int value)
				{
					panel.data.taaDebugMode = (TaaDebugMode)value;
				},
				getIndex = () => (int)panel.data.taaDebugMode,
				setIndex = delegate(int value)
				{
					panel.data.taaDebugMode = (TaaDebugMode)value;
				},
				onValueChanged = delegate
				{
					DebugManager.instance.ReDrawOnScreenDebug();
				}
			};
		}

		internal static DebugUI.Widget CreatePixelValidationMode(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.PixelValidationMode,
				autoEnum = typeof(DebugValidationMode),
				getter = () => (int)panel.data.validationMode,
				setter = delegate(int value)
				{
					panel.data.validationMode = (DebugValidationMode)value;
				},
				getIndex = () => (int)panel.data.validationMode,
				setIndex = delegate(int value)
				{
					panel.data.validationMode = (DebugValidationMode)value;
				},
				onValueChanged = delegate
				{
					DebugManager.instance.ReDrawOnScreenDebug();
				}
			};
		}

		internal static DebugUI.Widget CreatePixelValidationChannels(SettingsPanel panel)
		{
			return new DebugUI.EnumField
			{
				nameAndTooltip = Strings.Channels,
				autoEnum = typeof(PixelValidationChannels),
				getter = () => (int)panel.data.validationChannels,
				setter = delegate(int value)
				{
					panel.data.validationChannels = (PixelValidationChannels)value;
				},
				getIndex = () => (int)panel.data.validationChannels,
				setIndex = delegate(int value)
				{
					panel.data.validationChannels = (PixelValidationChannels)value;
				}
			};
		}

		internal static DebugUI.Widget CreatePixelValueRangeMin(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.ValueRangeMin,
				getter = () => panel.data.validationRangeMin,
				setter = delegate(float value)
				{
					panel.data.validationRangeMin = value;
				},
				incStep = 0.01f
			};
		}

		internal static DebugUI.Widget CreatePixelValueRangeMax(SettingsPanel panel)
		{
			return new DebugUI.FloatField
			{
				nameAndTooltip = Strings.ValueRangeMax,
				getter = () => panel.data.validationRangeMax,
				setter = delegate(float value)
				{
					panel.data.validationRangeMax = value;
				},
				incStep = 0.01f
			};
		}
	}

	[DisplayInfo(name = "Rendering", order = 1)]
	internal class SettingsPanel : DebugDisplaySettingsPanel<DebugDisplaySettingsRendering>
	{
		public SettingsPanel(DebugDisplaySettingsRendering data)
			: base(data)
		{
			AddWidget(new DebugUI.RuntimeDebugShadersMessageBox());
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Rendering Debug",
				flags = DebugUI.Flags.FrequentlyUsed,
				opened = true,
				children = 
				{
					WidgetFactory.CreateMapOverlays(this),
					WidgetFactory.CreateStpDebugViews(this),
					WidgetFactory.CreateMapOverlaySize(this),
					WidgetFactory.CreateHDR(this),
					WidgetFactory.CreateMSAA(this),
					WidgetFactory.CreateTaaDebugMode(this),
					WidgetFactory.CreatePostProcessing(this),
					WidgetFactory.CreateAdditionalWireframeShaderViews(this),
					WidgetFactory.CreateWireframeNotSupportedWarning(this),
					WidgetFactory.CreateOverdrawMode(this),
					WidgetFactory.CreateMaxOverdrawCount(this),
					WidgetFactory.CreateMipMapDebugWidget(this)
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "Pixel Validation",
				opened = true,
				children = 
				{
					WidgetFactory.CreatePixelValidationMode(this),
					(DebugUI.Widget)new DebugUI.Container
					{
						displayName = "Pixel Range Settings",
						isHiddenCallback = () => data.validationMode != DebugValidationMode.HighlightOutsideOfRange,
						children = 
						{
							WidgetFactory.CreatePixelValidationChannels(this),
							WidgetFactory.CreatePixelValueRangeMin(this),
							WidgetFactory.CreatePixelValueRangeMax(this)
						}
					}
				}
			});
			AddWidget(new DebugUI.Foldout
			{
				displayName = "HDR Output",
				opened = true,
				children = 
				{
					(DebugUI.Widget)new DebugUI.MessageBox
					{
						displayName = "The values on the Rendering Debugger editor window might not be accurate. Please use the playmode debug UI (Ctrl+Backspace).",
						style = DebugUI.MessageBox.Style.Warning
					},
					(DebugUI.Widget)DebugDisplaySettingsHDROutput.CreateHDROuputDisplayTable()
				}
			});
		}
	}

	private DebugWireframeMode m_WireframeMode;

	private bool m_Overdraw;

	private DebugOverdrawMode m_OverdrawMode;

	public DebugWireframeMode wireframeMode
	{
		get
		{
			return m_WireframeMode;
		}
		set
		{
			m_WireframeMode = value;
			UpdateDebugSceneOverrideMode();
		}
	}

	[Obsolete("overdraw has been deprecated. Use overdrawMode instead.", true)]
	public bool overdraw
	{
		get
		{
			return m_Overdraw;
		}
		set
		{
			m_Overdraw = value;
			UpdateDebugSceneOverrideMode();
		}
	}

	public DebugOverdrawMode overdrawMode
	{
		get
		{
			return m_OverdrawMode;
		}
		set
		{
			m_OverdrawMode = value;
			UpdateDebugSceneOverrideMode();
		}
	}

	public int maxOverdrawCount { get; set; } = 10;


	public DebugFullScreenMode fullScreenDebugMode { get; set; }

	internal int stpDebugViewIndex { get; set; }

	public int fullScreenDebugModeOutputSizeScreenPercent { get; set; } = 50;


	internal DebugSceneOverrideMode sceneOverrideMode { get; set; }

	public DebugMipInfoMode mipInfoMode { get; set; }

	public bool mipDebugStatusShowCode { get; set; }

	public DebugMipMapStatusMode mipDebugStatusMode { get; set; }

	public float mipDebugOpacity { get; set; } = 1f;


	public float mipDebugRecentUpdateCooldown { get; set; } = 3f;


	public int mipDebugMaterialTextureSlot { get; set; }

	public bool showInfoForAllSlots { get; set; } = true;


	internal bool canAggregateData
	{
		get
		{
			if (mipInfoMode != DebugMipInfoMode.MipStreamingStatus)
			{
				return mipInfoMode == DebugMipInfoMode.MipStreamingActivity;
			}
			return true;
		}
	}

	public DebugMipMapModeTerrainTexture mipDebugTerrainTexture { get; set; }

	public DebugPostProcessingMode postProcessingDebugMode { get; set; } = DebugPostProcessingMode.Auto;


	public bool enableMsaa { get; set; } = true;


	public bool enableHDR { get; set; } = true;


	public TaaDebugMode taaDebugMode { get; set; }

	public DebugValidationMode validationMode { get; set; }

	public PixelValidationChannels validationChannels { get; set; }

	public float validationRangeMin { get; set; }

	public float validationRangeMax { get; set; } = 1f;


	public bool AreAnySettingsActive
	{
		get
		{
			if (postProcessingDebugMode == DebugPostProcessingMode.Auto && fullScreenDebugMode == DebugFullScreenMode.None && sceneOverrideMode == DebugSceneOverrideMode.None && mipInfoMode == DebugMipInfoMode.None && validationMode == DebugValidationMode.None && enableMsaa && enableHDR)
			{
				return taaDebugMode != TaaDebugMode.None;
			}
			return true;
		}
	}

	public bool IsPostProcessingAllowed
	{
		get
		{
			if (postProcessingDebugMode != 0 && sceneOverrideMode == DebugSceneOverrideMode.None)
			{
				return mipInfoMode == DebugMipInfoMode.None;
			}
			return false;
		}
	}

	public bool IsLightingActive
	{
		get
		{
			if (sceneOverrideMode == DebugSceneOverrideMode.None)
			{
				return mipInfoMode == DebugMipInfoMode.None;
			}
			return false;
		}
	}

	private void UpdateDebugSceneOverrideMode()
	{
		switch (wireframeMode)
		{
		case DebugWireframeMode.Wireframe:
			sceneOverrideMode = DebugSceneOverrideMode.Wireframe;
			break;
		case DebugWireframeMode.SolidWireframe:
			sceneOverrideMode = DebugSceneOverrideMode.SolidWireframe;
			break;
		case DebugWireframeMode.ShadedWireframe:
			sceneOverrideMode = DebugSceneOverrideMode.ShadedWireframe;
			break;
		default:
			sceneOverrideMode = ((overdrawMode != 0) ? DebugSceneOverrideMode.Overdraw : DebugSceneOverrideMode.None);
			break;
		}
	}

	public bool TryGetScreenClearColor(ref Color color)
	{
		if (mipInfoMode != 0)
		{
			color = Color.black;
			return true;
		}
		switch (sceneOverrideMode)
		{
		case DebugSceneOverrideMode.None:
		case DebugSceneOverrideMode.ShadedWireframe:
			return false;
		case DebugSceneOverrideMode.Overdraw:
			color = Color.black;
			return true;
		case DebugSceneOverrideMode.Wireframe:
		case DebugSceneOverrideMode.SolidWireframe:
			color = new Color(0.1f, 0.1f, 0.1f, 1f);
			return true;
		default:
			throw new ArgumentOutOfRangeException("color");
		}
	}

	IDebugDisplaySettingsPanelDisposable IDebugDisplaySettingsData.CreatePanel()
	{
		return new SettingsPanel(this);
	}
}
