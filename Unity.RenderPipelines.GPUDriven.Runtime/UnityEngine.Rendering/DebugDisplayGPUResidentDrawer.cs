using System;
using System.Reflection;

namespace UnityEngine.Rendering;

public class DebugDisplayGPUResidentDrawer : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	private static class Strings
	{
		public const string drawerSettingsContainerName = "GPU Resident Drawer Settings";

		public static readonly DebugUI.Widget.NameAndTooltip displayBatcherStats = new DebugUI.Widget.NameAndTooltip
		{
			name = "Display Culling Stats",
			tooltip = "Enable the checkbox to display stats for instance culling."
		};

		public const string occlusionCullingTitle = "Occlusion Culling";

		public static readonly DebugUI.Widget.NameAndTooltip occlusionTestOverlayEnable = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occlusion Test Overlay",
			tooltip = "Occlusion test visualisation."
		};

		public static readonly DebugUI.Widget.NameAndTooltip occlusionTestOverlayCountVisible = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occlusion Test Overlay Count Visible",
			tooltip = "Occlusion test visualisation should count visible instances instead of occluded instances."
		};

		public static readonly DebugUI.Widget.NameAndTooltip overrideOcclusionTestToAlwaysPass = new DebugUI.Widget.NameAndTooltip
		{
			name = "Override Occlusion Test To Always Pass",
			tooltip = "Occlusion test always passes."
		};

		public static readonly DebugUI.Widget.NameAndTooltip occluderContextStats = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occluder Context Stats",
			tooltip = "Show all the active occluder context textures."
		};

		public static readonly DebugUI.Widget.NameAndTooltip occluderDebugViewEnable = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occluder Debug View",
			tooltip = "Debug view of occluder texture."
		};

		public static readonly DebugUI.Widget.NameAndTooltip occluderDebugViewIndex = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occluder Debug View Index",
			tooltip = "Index of the view for which the occluder texture is displayed. Use the Occlusion Test Context Stats for a list of the views."
		};

		public static readonly DebugUI.Widget.NameAndTooltip occluderDebugViewRangeMin = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occluder Debug View Range Min",
			tooltip = "Range in which the occluder debug texture are displayed."
		};

		public static readonly DebugUI.Widget.NameAndTooltip occluderDebugViewRangeMax = new DebugUI.Widget.NameAndTooltip
		{
			name = "Occluder Debug View Range Max",
			tooltip = "Range in which the occluder debug texture are displayed."
		};
	}

	[DisplayInfo(name = "Rendering", order = 5)]
	private class SettingsPanel : DebugDisplaySettingsPanel
	{
		public override DebugUI.Flags Flags => DebugUI.Flags.EditorForceUpdate;

		public SettingsPanel(DebugDisplayGPUResidentDrawer data)
		{
			DebugUI.Foldout foldout = new DebugUI.Foldout
			{
				displayName = "GPU Resident Drawer Settings",
				documentationUrl = typeof(DebugDisplayGPUResidentDrawer).GetCustomAttribute<HelpURLAttribute>()?.URL
			};
			AddWidget(foldout);
			string message;
			LogType severity;
			DebugUI.MessageBox item = new DebugUI.MessageBox
			{
				displayName = "Not Supported",
				style = DebugUI.MessageBox.Style.Warning,
				messageCallback = () => (!GPUResidentDrawer.IsGPUResidentDrawerSupportedBySRP(GPUResidentDrawer.GetGlobalSettingsFromRPAsset(), out message, out severity)) ? message : string.Empty,
				isHiddenCallback = () => GPUResidentDrawer.IsEnabled()
			};
			foldout.children.Add(item);
			foldout.children.Add(new DebugUI.Container
			{
				displayName = "Occlusion Culling",
				isHiddenCallback = () => !GPUResidentDrawer.IsEnabled(),
				children = 
				{
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.occlusionTestOverlayEnable,
						getter = () => data.occlusionTestOverlayEnable,
						setter = delegate(bool value)
						{
							data.occlusionTestOverlayEnable = value;
						}
					},
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.occlusionTestOverlayCountVisible,
						getter = () => data.occlusionTestOverlayCountVisible,
						setter = delegate(bool value)
						{
							data.occlusionTestOverlayCountVisible = value;
						}
					},
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.overrideOcclusionTestToAlwaysPass,
						getter = () => data.overrideOcclusionTestToAlwaysPass,
						setter = delegate(bool value)
						{
							data.overrideOcclusionTestToAlwaysPass = value;
						}
					},
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.occluderContextStats,
						getter = () => data.occluderContextStats,
						setter = delegate(bool value)
						{
							data.occluderContextStats = value;
						}
					},
					(DebugUI.Widget)new DebugUI.BoolField
					{
						nameAndTooltip = Strings.occluderDebugViewEnable,
						getter = () => data.occluderDebugViewEnable,
						setter = delegate(bool value)
						{
							data.occluderDebugViewEnable = value;
						}
					},
					(DebugUI.Widget)new DebugUI.IntField
					{
						nameAndTooltip = Strings.occluderDebugViewIndex,
						getter = () => data.occluderDebugViewIndex,
						setter = delegate(int value)
						{
							data.occluderDebugViewIndex = value;
						},
						isHiddenCallback = () => !data.occluderDebugViewEnable,
						min = () => 0,
						max = () => Math.Max(GetOcclusionContextsCounts() - 1, 0)
					},
					(DebugUI.Widget)new DebugUI.FloatField
					{
						nameAndTooltip = Strings.occluderDebugViewRangeMin,
						getter = () => data.occluderDebugViewRange.x,
						setter = delegate(float value)
						{
							data.occluderDebugViewRange.x = value;
						},
						isHiddenCallback = () => !data.occluderDebugViewEnable
					},
					(DebugUI.Widget)new DebugUI.FloatField
					{
						nameAndTooltip = Strings.occluderDebugViewRangeMax,
						getter = () => data.occluderDebugViewRange.y,
						setter = delegate(float value)
						{
							data.occluderDebugViewRange.y = value;
						},
						isHiddenCallback = () => !data.occluderDebugViewEnable
					}
				}
			});
			AddOcclusionContextStatsWidget(data);
			foldout.children.Add(new DebugUI.BoolField
			{
				nameAndTooltip = Strings.displayBatcherStats,
				getter = () => data.displayBatcherStats,
				setter = delegate(bool value)
				{
					data.displayBatcherStats = value;
				},
				isHiddenCallback = () => !GPUResidentDrawer.IsEnabled()
			});
			AddInstanceCullingStatsWidget(data);
		}

		private void AddInstanceCullingStatsWidget(DebugDisplayGPUResidentDrawer data)
		{
			DebugUI.Foldout foldout = new DebugUI.Foldout
			{
				displayName = "Instance Culler Stats",
				isHeader = true,
				opened = true,
				isHiddenCallback = () => !data.displayBatcherStats
			};
			foldout.children.Add(new DebugUI.ValueTuple
			{
				displayName = "View Count",
				values = new DebugUI.Value[1]
				{
					new DebugUI.Value
					{
						refreshRate = 0.2f,
						formatString = "{0}",
						getter = () => GetInstanceCullerViewCount()
					}
				}
			});
			foldout.children.Add(new DebugUI.ValueTuple
			{
				displayName = "Total Visible Instances (Cameras | Lights | Both)",
				values = new DebugUI.Value[3]
				{
					new DebugUI.Value
					{
						refreshRate = 0.2f,
						formatString = "{0}",
						getter = delegate
						{
							int num7 = 0;
							for (int num8 = 0; num8 < GetInstanceCullerViewCount(); num8++)
							{
								InstanceCullerViewStats instanceCullerViewStats6 = GetInstanceCullerViewStats(num8);
								if (instanceCullerViewStats6.viewType == BatchCullingViewType.Camera)
								{
									num7 += instanceCullerViewStats6.visibleInstancesOnGPU;
								}
							}
							return num7;
						}
					},
					new DebugUI.Value
					{
						refreshRate = 0.2f,
						formatString = "{0}",
						getter = delegate
						{
							int num5 = 0;
							for (int num6 = 0; num6 < GetInstanceCullerViewCount(); num6++)
							{
								InstanceCullerViewStats instanceCullerViewStats5 = GetInstanceCullerViewStats(num6);
								if (instanceCullerViewStats5.viewType == BatchCullingViewType.Light)
								{
									num5 += instanceCullerViewStats5.visibleInstancesOnGPU;
								}
							}
							return num5;
						}
					},
					new DebugUI.Value
					{
						refreshRate = 0.2f,
						formatString = "{0}",
						getter = delegate
						{
							int num4 = 0;
							for (int n = 0; n < GetInstanceCullerViewCount(); n++)
							{
								InstanceCullerViewStats instanceCullerViewStats4 = GetInstanceCullerViewStats(n);
								if (instanceCullerViewStats4.viewType != BatchCullingViewType.Filtering && instanceCullerViewStats4.viewType != BatchCullingViewType.Picking && instanceCullerViewStats4.viewType != BatchCullingViewType.SelectionOutline)
								{
									num4 += instanceCullerViewStats4.visibleInstancesOnGPU;
								}
							}
							return num4;
						}
					}
				}
			});
			foldout.children.Add(new DebugUI.ValueTuple
			{
				displayName = "Total Visible Primitives (Cameras | Lights | Both)",
				values = new DebugUI.Value[3]
				{
					new DebugUI.Value
					{
						refreshRate = 0.2f,
						formatString = "{0}",
						getter = delegate
						{
							int num3 = 0;
							for (int m = 0; m < GetInstanceCullerViewCount(); m++)
							{
								InstanceCullerViewStats instanceCullerViewStats3 = GetInstanceCullerViewStats(m);
								if (instanceCullerViewStats3.viewType == BatchCullingViewType.Camera)
								{
									num3 += instanceCullerViewStats3.visiblePrimitivesOnGPU;
								}
							}
							return num3;
						}
					},
					new DebugUI.Value
					{
						refreshRate = 0.2f,
						formatString = "{0}",
						getter = delegate
						{
							int num2 = 0;
							for (int l = 0; l < GetInstanceCullerViewCount(); l++)
							{
								InstanceCullerViewStats instanceCullerViewStats2 = GetInstanceCullerViewStats(l);
								if (instanceCullerViewStats2.viewType == BatchCullingViewType.Light)
								{
									num2 += instanceCullerViewStats2.visiblePrimitivesOnGPU;
								}
							}
							return num2;
						}
					},
					new DebugUI.Value
					{
						refreshRate = 0.2f,
						formatString = "{0}",
						getter = delegate
						{
							int num = 0;
							for (int k = 0; k < GetInstanceCullerViewCount(); k++)
							{
								InstanceCullerViewStats instanceCullerViewStats = GetInstanceCullerViewStats(k);
								if (instanceCullerViewStats.viewType != BatchCullingViewType.Filtering && instanceCullerViewStats.viewType != BatchCullingViewType.Picking && instanceCullerViewStats.viewType != BatchCullingViewType.SelectionOutline)
								{
									num += instanceCullerViewStats.visiblePrimitivesOnGPU;
								}
							}
							return num;
						}
					}
				}
			});
			DebugUI.Table table = new DebugUI.Table
			{
				displayName = "",
				isReadOnly = true
			};
			for (int i = 0; i < 32; i++)
			{
				table.children.Add(AddInstanceCullerViewDataRow(i));
			}
			DebugUI.Foldout foldout2 = new DebugUI.Foldout
			{
				displayName = "Per View Stats",
				isHeader = true,
				opened = false,
				isHiddenCallback = () => !data.displayBatcherStats
			};
			foldout2.children.Add(table);
			foldout.children.Add(foldout2);
			DebugUI.Table table2 = new DebugUI.Table
			{
				displayName = "",
				isReadOnly = true
			};
			for (int j = 0; j < 32; j++)
			{
				table2.children.Add(AddInstanceOcclusionPassDataRow(j));
			}
			DebugUI.Foldout foldout3 = new DebugUI.Foldout
			{
				displayName = "Occlusion Culling Events",
				isHeader = true,
				opened = false,
				isHiddenCallback = () => !data.displayBatcherStats
			};
			foldout3.children.Add(table2);
			foldout.children.Add(foldout3);
			AddWidget(foldout);
		}

		private void AddOcclusionContextStatsWidget(DebugDisplayGPUResidentDrawer data)
		{
			DebugUI.Foldout foldout = new DebugUI.Foldout
			{
				displayName = "Occlusion Context Stats",
				isHeader = true,
				opened = true,
				isHiddenCallback = () => !data.occluderContextStats
			};
			foldout.children.Add(new DebugUI.ValueTuple
			{
				displayName = "Active Occlusion Contexts",
				values = new DebugUI.Value[1]
				{
					new DebugUI.Value
					{
						refreshRate = 0.2f,
						formatString = "{0}",
						getter = () => GetOcclusionContextsCounts()
					}
				}
			});
			DebugUI.Table table = new DebugUI.Table
			{
				displayName = "",
				isReadOnly = true
			};
			for (int i = 0; i < 16; i++)
			{
				table.children.Add(AddOcclusionContextDataRow(i));
			}
			foldout.children.Add(table);
			AddWidget(foldout);
		}
	}

	private const string k_FormatString = "{0}";

	private const float k_RefreshRate = 0.2f;

	private const int k_MaxViewCount = 32;

	private const int k_MaxOcclusionPassCount = 32;

	private const int k_MaxContextCount = 16;

	public bool occluderDebugViewEnable;

	internal bool occluderContextStats;

	internal Vector2 occluderDebugViewRange = new Vector2(0f, 1f);

	internal int occluderDebugViewIndex;

	private bool displayBatcherStats
	{
		get
		{
			return GPUResidentDrawer.GetDebugStats()?.enabled ?? false;
		}
		set
		{
			DebugRendererBatcherStats debugStats = GPUResidentDrawer.GetDebugStats();
			if (debugStats != null)
			{
				debugStats.enabled = value;
			}
		}
	}

	internal bool occlusionTestOverlayEnable
	{
		get
		{
			return GPUResidentDrawer.GetDebugStats()?.occlusionOverlayEnabled ?? false;
		}
		set
		{
			DebugRendererBatcherStats debugStats = GPUResidentDrawer.GetDebugStats();
			if (debugStats != null)
			{
				debugStats.occlusionOverlayEnabled = value;
			}
		}
	}

	private bool occlusionTestOverlayCountVisible
	{
		get
		{
			return GPUResidentDrawer.GetDebugStats()?.occlusionOverlayCountVisible ?? false;
		}
		set
		{
			DebugRendererBatcherStats debugStats = GPUResidentDrawer.GetDebugStats();
			if (debugStats != null)
			{
				debugStats.occlusionOverlayCountVisible = value;
			}
		}
	}

	private bool overrideOcclusionTestToAlwaysPass
	{
		get
		{
			return GPUResidentDrawer.GetDebugStats()?.overrideOcclusionTestToAlwaysPass ?? false;
		}
		set
		{
			DebugRendererBatcherStats debugStats = GPUResidentDrawer.GetDebugStats();
			if (debugStats != null)
			{
				debugStats.overrideOcclusionTestToAlwaysPass = value;
			}
		}
	}

	public bool AreAnySettingsActive => displayBatcherStats;

	public bool IsPostProcessingAllowed => true;

	public bool IsLightingActive => true;

	internal bool GetOccluderViewInstanceID(out int viewInstanceID)
	{
		DebugRendererBatcherStats debugStats = GPUResidentDrawer.GetDebugStats();
		if (debugStats != null && occluderDebugViewIndex >= 0 && occluderDebugViewIndex < debugStats.occluderStats.Length)
		{
			viewInstanceID = debugStats.occluderStats[occluderDebugViewIndex].viewInstanceID;
			return true;
		}
		viewInstanceID = 0;
		return false;
	}

	private static InstanceCullerViewStats GetInstanceCullerViewStats(int viewIndex)
	{
		DebugRendererBatcherStats debugStats = GPUResidentDrawer.GetDebugStats();
		if (debugStats != null && viewIndex < debugStats.instanceCullerStats.Length)
		{
			return debugStats.instanceCullerStats[viewIndex];
		}
		return default(InstanceCullerViewStats);
	}

	private static InstanceOcclusionEventStats GetInstanceOcclusionEventStats(int passIndex)
	{
		DebugRendererBatcherStats debugStats = GPUResidentDrawer.GetDebugStats();
		if (debugStats != null && passIndex < debugStats.instanceOcclusionEventStats.Length)
		{
			return debugStats.instanceOcclusionEventStats[passIndex];
		}
		return default(InstanceOcclusionEventStats);
	}

	private static DebugOccluderStats GetOccluderStats(int occluderIndex)
	{
		DebugRendererBatcherStats debugStats = GPUResidentDrawer.GetDebugStats();
		if (debugStats != null && occluderIndex < debugStats.occluderStats.Length)
		{
			return debugStats.occluderStats[occluderIndex];
		}
		return default(DebugOccluderStats);
	}

	private static int GetOcclusionContextsCounts()
	{
		return GPUResidentDrawer.GetDebugStats()?.occluderStats.Length ?? 0;
	}

	private static int GetInstanceCullerViewCount()
	{
		return GPUResidentDrawer.GetDebugStats()?.instanceCullerStats.Length ?? 0;
	}

	private static int GetInstanceOcclusionEventCount()
	{
		return GPUResidentDrawer.GetDebugStats()?.instanceOcclusionEventStats.Length ?? 0;
	}

	private static DebugUI.Table.Row AddInstanceCullerViewDataRow(int viewIndex)
	{
		return new DebugUI.Table.Row
		{
			displayName = "",
			opened = true,
			isHiddenCallback = () => viewIndex >= GetInstanceCullerViewCount(),
			children = 
			{
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "View Type",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => GetInstanceCullerViewStats(viewIndex).viewType
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "View Instance ID",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => GetInstanceCullerViewStats(viewIndex).viewInstanceID
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Split Index",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => GetInstanceCullerViewStats(viewIndex).splitIndex
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Visible Instances CPU | GPU",
					tooltip = "Visible instances after CPU culling and after GPU culling.",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = delegate
					{
						InstanceCullerViewStats instanceCullerViewStats2 = GetInstanceCullerViewStats(viewIndex);
						return $"{instanceCullerViewStats2.visibleInstancesOnCPU} | {instanceCullerViewStats2.visibleInstancesOnGPU}";
					}
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Visible Primitives CPU | GPU",
					tooltip = "Visible primitives after CPU culling and after GPU culling.",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = delegate
					{
						InstanceCullerViewStats instanceCullerViewStats = GetInstanceCullerViewStats(viewIndex);
						return $"{instanceCullerViewStats.visiblePrimitivesOnCPU} | {instanceCullerViewStats.visiblePrimitivesOnGPU}";
					}
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Draw Commands",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => GetInstanceCullerViewStats(viewIndex).drawCommands
				}
			}
		};
	}

	private static object OccluderVersionString(in InstanceOcclusionEventStats stats)
	{
		if (stats.eventType != InstanceOcclusionEventType.OccluderUpdate && stats.occlusionTest == OcclusionTest.None)
		{
			return "-";
		}
		return stats.occluderVersion;
	}

	private static object OcclusionTestString(in InstanceOcclusionEventStats stats)
	{
		if (stats.eventType != 0)
		{
			return "-";
		}
		return stats.occlusionTest;
	}

	private static object VisibleInstancesString(in InstanceOcclusionEventStats stats)
	{
		if (stats.eventType != 0)
		{
			return "-";
		}
		return stats.visibleInstances;
	}

	private static object CulledInstancesString(in InstanceOcclusionEventStats stats)
	{
		if (stats.eventType != 0)
		{
			return "-";
		}
		return stats.culledInstances;
	}

	private static object VisiblePrimitivesString(in InstanceOcclusionEventStats stats)
	{
		if (stats.eventType != 0)
		{
			return "-";
		}
		return stats.visiblePrimitives;
	}

	private static object CulledPrimitivesString(in InstanceOcclusionEventStats stats)
	{
		if (stats.eventType != 0)
		{
			return "-";
		}
		return stats.culledPrimitives;
	}

	private static DebugUI.Table.Row AddInstanceOcclusionPassDataRow(int eventIndex)
	{
		return new DebugUI.Table.Row
		{
			displayName = "",
			opened = true,
			isHiddenCallback = () => eventIndex >= GetInstanceOcclusionEventCount(),
			children = 
			{
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "View Instance ID",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => GetInstanceOcclusionEventStats(eventIndex).viewInstanceID
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Event Type",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => $"{GetInstanceOcclusionEventStats(eventIndex).eventType}"
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Occluder Version",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = delegate
					{
						InstanceOcclusionEventStats stats6 = GetInstanceOcclusionEventStats(eventIndex);
						return OccluderVersionString(in stats6);
					}
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Subview Mask",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => $"0x{GetInstanceOcclusionEventStats(eventIndex).subviewMask:X}"
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Occlusion Test",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = delegate
					{
						InstanceOcclusionEventStats stats5 = GetInstanceOcclusionEventStats(eventIndex);
						return $"{OcclusionTestString(in stats5)}";
					}
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Visible Instances",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = delegate
					{
						InstanceOcclusionEventStats stats4 = GetInstanceOcclusionEventStats(eventIndex);
						return VisibleInstancesString(in stats4);
					}
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Culled Instances",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = delegate
					{
						InstanceOcclusionEventStats stats3 = GetInstanceOcclusionEventStats(eventIndex);
						return CulledInstancesString(in stats3);
					}
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Visible Primitives",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = delegate
					{
						InstanceOcclusionEventStats stats2 = GetInstanceOcclusionEventStats(eventIndex);
						return VisiblePrimitivesString(in stats2);
					}
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Culled Primitives",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = delegate
					{
						InstanceOcclusionEventStats stats = GetInstanceOcclusionEventStats(eventIndex);
						return CulledPrimitivesString(in stats);
					}
				}
			}
		};
	}

	private static DebugUI.Table.Row AddOcclusionContextDataRow(int index)
	{
		return new DebugUI.Table.Row
		{
			displayName = "",
			opened = true,
			isHiddenCallback = () => index >= GetOcclusionContextsCounts(),
			children = 
			{
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "View Instance ID",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => GetOccluderStats(index).viewInstanceID
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Subview Count",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = () => GetOccluderStats(index).subviewCount
				},
				(DebugUI.Widget)new DebugUI.Value
				{
					displayName = "Size Per Subview",
					refreshRate = 0.2f,
					formatString = "{0}",
					getter = delegate
					{
						Vector2Int occluderMipLayoutSize = GetOccluderStats(index).occluderMipLayoutSize;
						return $"{occluderMipLayoutSize.x}x{occluderMipLayoutSize.y}";
					}
				}
			}
		};
	}

	public bool TryGetScreenClearColor(ref Color color)
	{
		return false;
	}

	IDebugDisplaySettingsPanelDisposable IDebugDisplaySettingsData.CreatePanel()
	{
		return new SettingsPanel(this);
	}
}
