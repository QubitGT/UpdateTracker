using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine.Rendering;

public class DebugDisplaySettingsVolume : IDebugDisplaySettingsData, IDebugDisplaySettingsQuery
{
	private static class Styles
	{
		public static readonly GUIContent none = new GUIContent("None");
	}

	private static class Strings
	{
		public static readonly string cameraNeedsRendering = "Values might not be fully updated if the camera you are inspecting is not rendered.";

		public static readonly string none = "None";

		public static readonly string parameter = "Parameter";

		public static readonly string component = "Component";

		public static readonly string debugViewNotSupported = "N/A";

		public static readonly string volumeInfo = "Volume Info";

		public static readonly string gameObject = "GameObject";

		public static readonly string priority = "Priority";

		public static readonly string resultValue = "Result";

		public static readonly string resultValueTooltip = "The interpolated result value of the parameter. This value is used to render the camera.";

		public static readonly string globalDefaultValue = "Graphics Settings";

		public static readonly string globalDefaultValueTooltip = "Default value for this parameter, defined by the Default Volume Profile in Global Settings.";

		public static readonly string qualityLevelValue = "Quality Settings";

		public static readonly string qualityLevelValueTooltip = "Override value for this parameter, defined by the Volume Profile in the current SRP Asset.";

		public static readonly string global = "Global";

		public static readonly string local = "Local";

		public static readonly string volumeProfile = "Volume Profile";

		public static readonly string parameterNotCalculated = "N/A";
	}

	internal static class WidgetFactory
	{
		private struct VolumeParameterChain
		{
			public DebugUI.Widget.NameAndTooltip nameAndTooltip;

			public VolumeProfile volumeProfile;

			public VolumeComponent volumeComponent;

			public Volume volume;
		}

		private static DebugUI.Value s_EmptyDebugUIValue = new DebugUI.Value
		{
			getter = () => string.Empty
		};

		public static DebugUI.EnumField CreateComponentSelector(SettingsPanel panel, Action<DebugUI.Field<int>, int> refresh)
		{
			int num = 0;
			List<GUIContent> list = new List<GUIContent> { Styles.none };
			List<int> list2 = new List<int> { num++ };
			foreach (var item in panel.data.volumeComponentsPathAndType)
			{
				GUIContent gUIContent = new GUIContent();
				(gUIContent.text, _) = item;
				list.Add(gUIContent);
				list2.Add(num++);
			}
			return new DebugUI.EnumField
			{
				displayName = Strings.component,
				getter = () => panel.data.selectedComponent,
				setter = delegate(int value)
				{
					panel.data.selectedComponent = value;
				},
				enumNames = list.ToArray(),
				enumValues = list2.ToArray(),
				getIndex = () => panel.data.volumeComponentEnumIndex,
				setIndex = delegate(int value)
				{
					panel.data.volumeComponentEnumIndex = value;
				},
				onValueChanged = refresh
			};
		}

		public static DebugUI.CameraSelector CreateCameraSelector(SettingsPanel panel, Action<DebugUI.Field<Object>, Object> refresh)
		{
			return new DebugUI.CameraSelector
			{
				getter = () => panel.data.selectedCamera,
				setter = delegate(Object value)
				{
					panel.data.selectedCamera = value as Camera;
				},
				onValueChanged = refresh
			};
		}

		internal static DebugUI.Widget CreateVolumeParameterWidget(string name, bool isResultParameter, VolumeParameter param)
		{
			return new DebugUI.Value
			{
				displayName = name,
				getter = () => Strings.parameterNotCalculated
			};
		}

		private static VolumeComponent GetSelectedVolumeComponent(VolumeProfile profile, Type selectedType)
		{
			if (profile != null)
			{
				foreach (VolumeComponent component in profile.components)
				{
					if (component.GetType() == selectedType)
					{
						return component;
					}
				}
			}
			return null;
		}

		private static List<VolumeParameterChain> GetResolutionChain(DebugDisplaySettingsVolume data)
		{
			List<VolumeParameterChain> list = new List<VolumeParameterChain>();
			Type selectedComponentType = data.selectedComponentType;
			if (data.selectedCamera == null || selectedComponentType == null)
			{
				return list;
			}
			if (data.resultVolumeComponent == null)
			{
				return list;
			}
			VolumeParameterChain volumeParameterChain = default(VolumeParameterChain);
			volumeParameterChain.nameAndTooltip = new DebugUI.Widget.NameAndTooltip
			{
				name = Strings.resultValue,
				tooltip = Strings.resultValueTooltip
			};
			volumeParameterChain.volumeComponent = data.resultVolumeComponent;
			VolumeParameterChain item = volumeParameterChain;
			list.Add(item);
			ObservableList<Volume> volumesList = data.GetVolumesList();
			for (int num = volumesList.Count - 1; num >= 0; num--)
			{
				Volume volume = volumesList[num];
				VolumeProfile volumeProfile = (volume.HasInstantiatedProfile() ? volume.profile : volume.sharedProfile);
				VolumeComponent selectedVolumeComponent = GetSelectedVolumeComponent(volumeProfile, selectedComponentType);
				if (selectedVolumeComponent != null)
				{
					volumeParameterChain = default(VolumeParameterChain);
					volumeParameterChain.nameAndTooltip = new DebugUI.Widget.NameAndTooltip
					{
						name = volumeProfile.name,
						tooltip = volumeProfile.name
					};
					volumeParameterChain.volumeProfile = volumeProfile;
					volumeParameterChain.volumeComponent = selectedVolumeComponent;
					volumeParameterChain.volume = volume;
					VolumeParameterChain item2 = volumeParameterChain;
					list.Add(item2);
				}
			}
			if (VolumeManager.instance.customDefaultProfiles != null)
			{
				foreach (VolumeProfile customDefaultProfile in VolumeManager.instance.customDefaultProfiles)
				{
					VolumeComponent selectedVolumeComponent2 = GetSelectedVolumeComponent(customDefaultProfile, selectedComponentType);
					if (selectedVolumeComponent2 != null)
					{
						volumeParameterChain = default(VolumeParameterChain);
						volumeParameterChain.nameAndTooltip = new DebugUI.Widget.NameAndTooltip
						{
							name = customDefaultProfile.name,
							tooltip = customDefaultProfile.name
						};
						volumeParameterChain.volumeProfile = customDefaultProfile;
						volumeParameterChain.volumeComponent = selectedVolumeComponent2;
						VolumeParameterChain item3 = volumeParameterChain;
						list.Add(item3);
					}
				}
			}
			if (VolumeManager.instance.qualityDefaultProfile != null)
			{
				VolumeComponent selectedVolumeComponent3 = GetSelectedVolumeComponent(VolumeManager.instance.qualityDefaultProfile, selectedComponentType);
				if (selectedVolumeComponent3 != null)
				{
					volumeParameterChain = default(VolumeParameterChain);
					volumeParameterChain.nameAndTooltip = new DebugUI.Widget.NameAndTooltip
					{
						name = Strings.qualityLevelValue,
						tooltip = Strings.qualityLevelValueTooltip
					};
					volumeParameterChain.volumeProfile = VolumeManager.instance.qualityDefaultProfile;
					volumeParameterChain.volumeComponent = selectedVolumeComponent3;
					VolumeParameterChain item4 = volumeParameterChain;
					list.Add(item4);
				}
			}
			if (VolumeManager.instance.globalDefaultProfile != null)
			{
				VolumeComponent selectedVolumeComponent4 = GetSelectedVolumeComponent(VolumeManager.instance.globalDefaultProfile, selectedComponentType);
				if (selectedVolumeComponent4 != null)
				{
					volumeParameterChain = default(VolumeParameterChain);
					volumeParameterChain.nameAndTooltip = new DebugUI.Widget.NameAndTooltip
					{
						name = Strings.globalDefaultValue,
						tooltip = Strings.globalDefaultValueTooltip
					};
					volumeParameterChain.volumeProfile = VolumeManager.instance.globalDefaultProfile;
					volumeParameterChain.volumeComponent = selectedVolumeComponent4;
					VolumeParameterChain item5 = volumeParameterChain;
					list.Add(item5);
				}
			}
			return list;
		}

		public static DebugUI.Table CreateVolumeTable(DebugDisplaySettingsVolume data)
		{
			Func<bool> isHiddenCallback = () => true;
			DebugUI.Table table = new DebugUI.Table
			{
				displayName = Strings.parameter,
				isReadOnly = true,
				isHiddenCallback = isHiddenCallback
			};
			List<VolumeParameterChain> resolutionChain = GetResolutionChain(data);
			if (resolutionChain.Count == 0)
			{
				return table;
			}
			GenerateTableRows(table, resolutionChain);
			GenerateTableColumns(table, data, resolutionChain);
			return table;
		}

		private static void GenerateTableColumns(DebugUI.Table table, DebugDisplaySettingsVolume data, List<VolumeParameterChain> resolutionChain)
		{
			for (int i = 0; i < resolutionChain.Count; i++)
			{
				VolumeParameterChain chain = resolutionChain[i];
				int num = -1;
				if (chain.volume != null)
				{
					((DebugUI.Table.Row)table.children[++num]).children.Add(new DebugUI.Value
					{
						nameAndTooltip = chain.nameAndTooltip,
						getter = delegate
						{
							string text = (chain.volume.isGlobal ? Strings.global : Strings.local);
							float volumeWeight = data.GetVolumeWeight(chain.volume);
							return chain.volumeComponent.active ? $"{text} ({volumeWeight * 100f:F2}%)" : (text + " (disabled)");
						},
						refreshRate = 0.2f
					});
					((DebugUI.Table.Row)table.children[++num]).children.Add(new DebugUI.ObjectField
					{
						displayName = string.Empty,
						getter = () => chain.volume
					});
					((DebugUI.Table.Row)table.children[++num]).children.Add(new DebugUI.Value
					{
						nameAndTooltip = chain.nameAndTooltip,
						getter = () => chain.volume.priority
					});
				}
				else
				{
					((DebugUI.Table.Row)table.children[++num]).children.Add(new DebugUI.Value
					{
						nameAndTooltip = chain.nameAndTooltip,
						getter = () => string.Empty
					});
					((DebugUI.Table.Row)table.children[++num]).children.Add(s_EmptyDebugUIValue);
					((DebugUI.Table.Row)table.children[++num]).children.Add(s_EmptyDebugUIValue);
				}
				((DebugUI.Table.Row)table.children[++num]).children.Add((chain.volumeProfile != null) ? ((DebugUI.Widget)new DebugUI.ObjectField
				{
					displayName = string.Empty,
					getter = () => chain.volumeProfile
				}) : ((DebugUI.Widget)s_EmptyDebugUIValue));
				((DebugUI.Table.Row)table.children[++num]).children.Add(s_EmptyDebugUIValue);
				bool isResultParameter = i == 0;
				for (int j = 0; j < chain.volumeComponent.parameterList.Length; j++)
				{
					VolumeParameter param = chain.volumeComponent.parameterList[j];
					((DebugUI.Table.Row)table.children[++num]).children.Add(CreateVolumeParameterWidget(chain.nameAndTooltip.name, isResultParameter, param));
				}
			}
		}

		private static void GenerateTableRows(DebugUI.Table table, List<VolumeParameterChain> resolutionChain)
		{
			DebugUI.Table.Row item = new DebugUI.Table.Row
			{
				displayName = Strings.volumeInfo,
				opened = true
			};
			table.children.Add(item);
			DebugUI.Table.Row item2 = new DebugUI.Table.Row
			{
				displayName = Strings.gameObject
			};
			table.children.Add(item2);
			DebugUI.Table.Row item3 = new DebugUI.Table.Row
			{
				displayName = Strings.priority
			};
			table.children.Add(item3);
			DebugUI.Table.Row item4 = new DebugUI.Table.Row
			{
				displayName = Strings.volumeProfile
			};
			table.children.Add(item4);
			DebugUI.Table.Row item5 = new DebugUI.Table.Row
			{
				displayName = string.Empty
			};
			table.children.Add(item5);
			VolumeComponent volumeComponent = resolutionChain[0].volumeComponent;
			for (int i = 0; i < volumeComponent.parameterList.Length; i++)
			{
				_ = volumeComponent.parameterList[i];
				string displayName = i.ToString();
				table.children.Add(new DebugUI.Table.Row
				{
					displayName = displayName
				});
			}
		}
	}

	[DisplayInfo(name = "Volume", order = int.MaxValue)]
	internal class SettingsPanel : DebugDisplaySettingsPanel<DebugDisplaySettingsVolume>
	{
		private DebugUI.Table m_VolumeTable;

		public override DebugUI.Flags Flags => DebugUI.Flags.EditorForceUpdate;

		public override void Dispose()
		{
			base.Dispose();
			base.data.GetVolumesList().ItemAdded -= OnVolumeInfluenceChanged;
			base.data.GetVolumesList().ItemRemoved -= OnVolumeInfluenceChanged;
		}

		public SettingsPanel(DebugDisplaySettingsVolume data)
			: base(data)
		{
			SettingsPanel settingsPanel = this;
			DebugUI.CameraSelector cameraSelector = WidgetFactory.CreateCameraSelector(this, delegate
			{
				settingsPanel.Refresh();
			});
			List<Camera> list = cameraSelector.getObjects() as List<Camera>;
			if (data.selectedCamera == null && list != null && list.Count > 0)
			{
				data.selectedCamera = list[0];
			}
			AddWidget(cameraSelector);
			AddWidget(WidgetFactory.CreateComponentSelector(this, delegate
			{
				settingsPanel.Refresh();
			}));
			Func<bool> isHiddenCallback = () => data.selectedCamera == null || data.selectedComponent <= 0;
			AddWidget(new DebugUI.MessageBox
			{
				displayName = Strings.cameraNeedsRendering,
				style = DebugUI.MessageBox.Style.Warning,
				isHiddenCallback = isHiddenCallback
			});
			m_VolumeTable = WidgetFactory.CreateVolumeTable(data);
			AddWidget(m_VolumeTable);
			data.GetVolumesList().ItemAdded += OnVolumeInfluenceChanged;
			data.GetVolumesList().ItemRemoved += OnVolumeInfluenceChanged;
		}

		private void OnVolumeInfluenceChanged(ObservableList<Volume> sender, ListChangedEventArgs<Volume> e)
		{
			Refresh();
			DebugManager.instance.ReDrawOnScreenDebug();
		}

		private void Refresh()
		{
			if (DebugManager.instance.GetPanel(PanelName) == null)
			{
				return;
			}
			bool flag = false;
			if (m_Data.selectedComponent > 0 && m_Data.selectedCamera != null)
			{
				flag = true;
				DebugUI.Table table = WidgetFactory.CreateVolumeTable(m_Data);
				m_VolumeTable.children.Clear();
				foreach (DebugUI.Widget child in table.children)
				{
					m_VolumeTable.children.Add(child);
				}
			}
			if (flag)
			{
				DebugManager.instance.ReDrawOnScreenDebug();
			}
		}
	}

	private int m_SelectedComponentIndex = -1;

	private Camera m_SelectedCamera;

	private VolumeComponent m_VolumeInterpolatedResults;

	private bool m_StoreStackInterpolatedValues;

	private ObservableList<Volume> m_InfluenceVolumes = new ObservableList<Volume>();

	private List<(Volume volume, float weight)> m_VolumesWeights = new List<(Volume, float)>();

	internal int volumeComponentEnumIndex;

	private const string k_PanelTitle = "Volume";

	[Obsolete("This property has been obsoleted and will be removed in a future version. #from(6000.2)", false)]
	public IVolumeDebugSettings volumeDebugSettings { get; }

	public int selectedComponent
	{
		get
		{
			return m_SelectedComponentIndex;
		}
		set
		{
			if (value != m_SelectedComponentIndex)
			{
				m_SelectedComponentIndex = value;
				OnSelectionChanged();
			}
		}
	}

	public Type selectedComponentType
	{
		get
		{
			if (selectedComponent <= 0)
			{
				return null;
			}
			return volumeComponentsPathAndType[selectedComponent - 1].Item2;
		}
		set
		{
			int num = volumeComponentsPathAndType.FindIndex(((string, Type) t) => t.Item2 == value);
			if (num != -1)
			{
				selectedComponent = num + 1;
			}
		}
	}

	public List<(string, Type)> volumeComponentsPathAndType => VolumeManager.instance.GetVolumeComponentsForDisplay(GraphicsSettings.currentRenderPipelineAssetType);

	public Camera selectedCamera
	{
		get
		{
			return m_SelectedCamera;
		}
		set
		{
			if (value != m_SelectedCamera)
			{
				m_SelectedCamera = value;
				OnSelectionChanged();
			}
		}
	}

	internal VolumeComponent resultVolumeComponent
	{
		get
		{
			if (m_VolumeInterpolatedResults == null)
			{
				m_VolumeInterpolatedResults = ScriptableObject.CreateInstance(selectedComponentType) as VolumeComponent;
			}
			return m_VolumeInterpolatedResults;
		}
	}

	public bool AreAnySettingsActive => false;

	private void DestroyVolumeInterpolatedResults()
	{
		if (m_VolumeInterpolatedResults != null)
		{
			Object.DestroyImmediate(m_VolumeInterpolatedResults);
		}
	}

	private void OnSelectionChanged()
	{
		ClearInterpolationData();
		DestroyVolumeInterpolatedResults();
	}

	private void ClearInterpolationData()
	{
		m_VolumesWeights.Clear();
	}

	private static bool AreVolumesChanged(ObservableList<Volume> influenceVolumes, List<(Volume volume, float weight)> volumesWeights)
	{
		if (influenceVolumes.Count != volumesWeights.Count)
		{
			return true;
		}
		for (int i = 0; i < influenceVolumes.Count; i++)
		{
			if (influenceVolumes[i] != volumesWeights[i].volume)
			{
				return true;
			}
		}
		return false;
	}

	private void OnBeginVolumeStackUpdate(VolumeStack stack, Camera camera)
	{
		if (camera == selectedCamera)
		{
			ClearInterpolationData();
			m_StoreStackInterpolatedValues = selectedCamera != null && selectedComponentType != null;
		}
	}

	private void OnEndVolumeStackUpdate(VolumeStack stack, Camera camera)
	{
		if (!m_StoreStackInterpolatedValues)
		{
			return;
		}
		if (AreVolumesChanged(m_InfluenceVolumes, m_VolumesWeights))
		{
			m_InfluenceVolumes.Clear();
			foreach (var volumesWeight in m_VolumesWeights)
			{
				m_InfluenceVolumes.Add(volumesWeight.volume);
			}
		}
		VolumeComponent component = stack.GetComponent(selectedComponentType);
		for (int i = 0; i < component.parameters.Count; i++)
		{
			resultVolumeComponent.parameters[i].SetValue(component.parameters[i]);
		}
		m_StoreStackInterpolatedValues = false;
	}

	private void OnVolumeStackInterpolated(VolumeStack stack, Volume volume, float interpolationFactor)
	{
		if (m_StoreStackInterpolatedValues)
		{
			m_VolumesWeights.Add((volume, interpolationFactor));
		}
	}

	public float GetVolumeWeight(Volume volume)
	{
		if (m_VolumesWeights.Count == 0)
		{
			return 0f;
		}
		foreach (var volumesWeight in m_VolumesWeights)
		{
			if (volume == volumesWeight.volume)
			{
				return volumesWeight.weight;
			}
		}
		return 0f;
	}

	public ObservableList<Volume> GetVolumesList()
	{
		return m_InfluenceVolumes;
	}

	void IDebugDisplaySettingsData.Reset()
	{
		ClearInterpolationData();
		DestroyVolumeInterpolatedResults();
	}

	[Obsolete("This constructor has been obsoleted and will be removed in a future version. #from(6000.2)", false)]
	public DebugDisplaySettingsVolume(IVolumeDebugSettings volumeDebugSettings)
		: this()
	{
		this.volumeDebugSettings = volumeDebugSettings;
	}

	public DebugDisplaySettingsVolume()
	{
	}

	internal static string ExtractResult(VolumeParameter param)
	{
		if (param == null)
		{
			return Strings.parameterNotCalculated;
		}
		PropertyInfo property = param.GetType().GetProperty("value");
		if (property == null)
		{
			return "-";
		}
		object value = property.GetValue(param);
		Type propertyType = property.PropertyType;
		if (value == null || value.Equals(null))
		{
			return Strings.none + " (" + propertyType.Name + ")";
		}
		MethodInfo method = propertyType.GetMethod("ToString", Type.EmptyTypes);
		if (method == null || method.DeclaringType == typeof(object) || method.DeclaringType == typeof(Object))
		{
			PropertyInfo property2 = property.PropertyType.GetProperty("name");
			if (property2 == null)
			{
				return Strings.debugViewNotSupported;
			}
			return $"{property2.GetValue(value)}" ?? Strings.none;
		}
		return value.ToString();
	}

	public IDebugDisplaySettingsPanelDisposable CreatePanel()
	{
		return new SettingsPanel(this);
	}
}
