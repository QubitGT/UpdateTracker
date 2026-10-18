using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Unity.Profiling;

namespace UnityEngine.Rendering;

public sealed class VolumeManager
{
	private static readonly ProfilerMarker k_ProfilerMarkerUpdate = new ProfilerMarker("VolumeManager.Update");

	private static readonly ProfilerMarker k_ProfilerMarkerReplaceData = new ProfilerMarker("VolumeManager.ReplaceData");

	private static readonly ProfilerMarker k_ProfilerMarkerEvaluateVolumeDefaultState = new ProfilerMarker("VolumeManager.EvaluateVolumeDefaultState");

	private static readonly Lazy<VolumeManager> s_Instance = new Lazy<VolumeManager>(() => new VolumeManager());

	private static readonly Dictionary<Type, List<(string, Type)>> s_SupportedVolumeComponentsForRenderPipeline = new Dictionary<Type, List<(string, Type)>>();

	private Type[] m_BaseComponentTypeArray;

	private readonly VolumeCollection m_VolumeCollection = new VolumeCollection();

	private VolumeComponent[] m_ComponentsDefaultState;

	internal VolumeParameter[] m_ParametersDefaultState;

	private VolumeStack m_DefaultStack;

	private readonly List<VolumeStack> m_CreatedVolumeStacks = new List<VolumeStack>();

	public static VolumeManager instance => s_Instance.Value;

	public VolumeStack stack { get; set; }

	[Obsolete("Please use baseComponentTypeArray instead.")]
	public IEnumerable<Type> baseComponentTypes => baseComponentTypeArray;

	public Type[] baseComponentTypeArray
	{
		get
		{
			if (isInitialized)
			{
				return m_BaseComponentTypeArray;
			}
			throw new InvalidOperationException("VolumeManager.instance.baseComponentTypeArray cannot be called before the VolumeManager is initialized. (See VolumeManager.instance.isInitialized and RenderPipelineManager for creation callback).");
		}
		internal set
		{
			m_BaseComponentTypeArray = value;
		}
	}

	public VolumeProfile globalDefaultProfile { get; private set; }

	public VolumeProfile qualityDefaultProfile { get; private set; }

	public ReadOnlyCollection<VolumeProfile> customDefaultProfiles { get; private set; }

	public bool isInitialized { get; private set; }

	[Obsolete("Please use the Register without a given layer index #from(6000.0)", false)]
	public void Register(Volume volume, int layer)
	{
		if (volume.gameObject.layer != layer)
		{
			Debug.LogWarning($"Trying to register Volume {volume.name} on layer index {layer}, when the GameObject {volume.gameObject.name} is on layer index {volume.gameObject.layer}." + Environment.NewLine + "The Volume Manager will respect the GameObject's layer.");
		}
		Register(volume);
	}

	[Obsolete("Please use the Register without a given layer index #from(6000.0)", false)]
	public void Unregister(Volume volume, int layer)
	{
		if (volume.gameObject.layer != layer)
		{
			Debug.LogWarning($"Trying to unregister Volume {volume.name} on layer index {layer}, when the GameObject {volume.gameObject.name} is on layer index {volume.gameObject.layer}." + Environment.NewLine + "The Volume Manager will respect the GameObject's layer.");
		}
		Unregister(volume);
	}

	internal List<(string, Type)> GetVolumeComponentsForDisplay(Type currentPipelineAssetType)
	{
		if (currentPipelineAssetType == null)
		{
			return new List<(string, Type)>();
		}
		if (!currentPipelineAssetType.IsSubclassOf(typeof(RenderPipelineAsset)))
		{
			throw new ArgumentException("currentPipelineAssetType");
		}
		if (s_SupportedVolumeComponentsForRenderPipeline.TryGetValue(currentPipelineAssetType, out var value))
		{
			return value;
		}
		if (baseComponentTypeArray == null)
		{
			LoadBaseTypes(currentPipelineAssetType);
		}
		value = BuildVolumeComponentDisplayList(baseComponentTypeArray);
		s_SupportedVolumeComponentsForRenderPipeline[currentPipelineAssetType] = value;
		return value;
	}

	private List<(string, Type)> BuildVolumeComponentDisplayList(Type[] types)
	{
		if (types == null)
		{
			throw new ArgumentNullException("types");
		}
		List<(string, Type)> list = new List<(string, Type)>();
		foreach (Type type in types)
		{
			string text = string.Empty;
			bool flag = false;
			object[] customAttributes = type.GetCustomAttributes(inherit: false);
			foreach (object obj in customAttributes)
			{
				if (!(obj is VolumeComponentMenu volumeComponentMenu))
				{
					if (obj is HideInInspector || obj is ObsoleteAttribute)
					{
						flag = true;
					}
				}
				else
				{
					text = volumeComponentMenu.menu;
				}
			}
			if (!flag)
			{
				if (string.IsNullOrEmpty(text))
				{
					text = type.Name;
				}
				list.Add((text, type));
			}
		}
		return list.OrderBy(((string, Type) i) => i.Item1).ToList();
	}

	public VolumeComponent GetVolumeComponentDefaultState(Type volumeComponentType)
	{
		if (!typeof(VolumeComponent).IsAssignableFrom(volumeComponentType))
		{
			return null;
		}
		VolumeComponent[] componentsDefaultState = m_ComponentsDefaultState;
		foreach (VolumeComponent volumeComponent in componentsDefaultState)
		{
			if (volumeComponent.GetType() == volumeComponentType)
			{
				return volumeComponent;
			}
		}
		return null;
	}

	internal VolumeManager()
	{
	}

	public void Initialize(VolumeProfile globalDefaultVolumeProfile = null, VolumeProfile qualityDefaultVolumeProfile = null)
	{
		LoadBaseTypes(GraphicsSettings.currentRenderPipelineAssetType);
		InitializeInternal(globalDefaultVolumeProfile, qualityDefaultVolumeProfile);
	}

	internal void InitializeInternal(VolumeProfile globalDefaultVolumeProfile = null, VolumeProfile qualityDefaultVolumeProfile = null)
	{
		InitializeVolumeComponents();
		globalDefaultProfile = globalDefaultVolumeProfile;
		qualityDefaultProfile = qualityDefaultVolumeProfile;
		EvaluateVolumeDefaultState();
		m_DefaultStack = CreateStackInternal();
		stack = m_DefaultStack;
		isInitialized = true;
	}

	public void Deinitialize()
	{
		DestroyStack(m_DefaultStack);
		m_DefaultStack = null;
		foreach (VolumeStack createdVolumeStack in m_CreatedVolumeStacks)
		{
			createdVolumeStack.Dispose();
		}
		m_CreatedVolumeStacks.Clear();
		baseComponentTypeArray = null;
		globalDefaultProfile = null;
		qualityDefaultProfile = null;
		customDefaultProfiles = null;
		isInitialized = false;
	}

	public void SetGlobalDefaultProfile(VolumeProfile profile)
	{
		globalDefaultProfile = profile;
		EvaluateVolumeDefaultState();
	}

	public void SetQualityDefaultProfile(VolumeProfile profile)
	{
		qualityDefaultProfile = profile;
		EvaluateVolumeDefaultState();
	}

	public void SetCustomDefaultProfiles(List<VolumeProfile> profiles)
	{
		List<VolumeProfile> list = profiles ?? new List<VolumeProfile>();
		list.RemoveAll((VolumeProfile x) => x == null);
		customDefaultProfiles = new ReadOnlyCollection<VolumeProfile>(list);
		EvaluateVolumeDefaultState();
	}

	public void OnVolumeProfileChanged(VolumeProfile profile)
	{
		if (isInitialized && (globalDefaultProfile == profile || qualityDefaultProfile == profile || (customDefaultProfiles != null && customDefaultProfiles.Contains(profile))))
		{
			EvaluateVolumeDefaultState();
		}
	}

	public void OnVolumeComponentChanged(VolumeComponent component)
	{
		List<VolumeProfile> list = new List<VolumeProfile> { globalDefaultProfile, globalDefaultProfile };
		if (customDefaultProfiles != null)
		{
			list.AddRange(customDefaultProfiles);
		}
		foreach (VolumeProfile item in list)
		{
			if (item.components.Contains(component))
			{
				EvaluateVolumeDefaultState();
				break;
			}
		}
	}

	public VolumeStack CreateStack()
	{
		if (!isInitialized)
		{
			throw new InvalidOperationException("VolumeManager.instance.CreateStack() cannot be called before the VolumeManager is initialized. (See VolumeManager.instance.isInitialized and RenderPipelineManager for creation callback).");
		}
		return CreateStackInternal();
	}

	private VolumeStack CreateStackInternal()
	{
		VolumeStack volumeStack = new VolumeStack();
		volumeStack.Reload(m_BaseComponentTypeArray);
		m_CreatedVolumeStacks.Add(volumeStack);
		return volumeStack;
	}

	public void ResetMainStack()
	{
		stack = m_DefaultStack;
	}

	public void DestroyStack(VolumeStack stack)
	{
		m_CreatedVolumeStacks.Remove(stack);
		stack.Dispose();
	}

	private bool IsSupportedByObsoleteVolumeComponentMenuForRenderPipeline(Type t, Type pipelineAssetType)
	{
		bool result = false;
		if (t.GetCustomAttribute<VolumeComponentMenuForRenderPipeline>() != null)
		{
			Debug.LogWarning(string.Format("{0} is deprecated, use {1} and {2} with {3} instead. #from(2023.1)", "VolumeComponentMenuForRenderPipeline", "SupportedOnRenderPipelineAttribute", "VolumeComponentMenu", t));
		}
		return result;
	}

	internal void LoadBaseTypes(Type pipelineAssetType)
	{
		List<Type> value;
		using (ListPool<Type>.Get(out value))
		{
			foreach (Type item in CoreUtils.GetAllTypesDerivedFrom<VolumeComponent>())
			{
				if (!item.IsAbstract && (SupportedOnRenderPipelineAttribute.IsTypeSupportedOnRenderPipeline(item, pipelineAssetType) || IsSupportedByObsoleteVolumeComponentMenuForRenderPipeline(item, pipelineAssetType)))
				{
					value.Add(item);
				}
			}
			m_BaseComponentTypeArray = value.ToArray();
		}
	}

	internal void InitializeVolumeComponents()
	{
		if (m_BaseComponentTypeArray == null || m_BaseComponentTypeArray.Length == 0)
		{
			return;
		}
		BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		Type[] array = m_BaseComponentTypeArray;
		for (int i = 0; i < array.Length; i++)
		{
			MethodInfo method = array[i].GetMethod("Init", bindingAttr);
			if (method != null)
			{
				method.Invoke(null, null);
			}
		}
	}

	private void EvaluateVolumeDefaultState()
	{
		if (m_BaseComponentTypeArray == null || m_BaseComponentTypeArray.Length == 0)
		{
			return;
		}
		using (k_ProfilerMarkerEvaluateVolumeDefaultState.Auto())
		{
			List<VolumeComponent> componentsDefaultStateList = new List<VolumeComponent>();
			Type[] array = m_BaseComponentTypeArray;
			foreach (Type type in array)
			{
				componentsDefaultStateList.Add((VolumeComponent)ScriptableObject.CreateInstance(type));
			}
			ApplyDefaultProfile(globalDefaultProfile);
			ApplyDefaultProfile(qualityDefaultProfile);
			if (customDefaultProfiles != null)
			{
				foreach (VolumeProfile customDefaultProfile in customDefaultProfiles)
				{
					ApplyDefaultProfile(customDefaultProfile);
				}
			}
			List<VolumeParameter> list = new List<VolumeParameter>();
			foreach (VolumeComponent item in componentsDefaultStateList)
			{
				list.AddRange(item.parameters);
			}
			m_ComponentsDefaultState = componentsDefaultStateList.ToArray();
			m_ParametersDefaultState = list.ToArray();
			foreach (VolumeStack createdVolumeStack in m_CreatedVolumeStacks)
			{
				createdVolumeStack.requiresReset = true;
				createdVolumeStack.requiresResetForAllProperties = true;
			}
			void ApplyDefaultProfile(VolumeProfile profile)
			{
				if (!(profile == null))
				{
					for (int j = 0; j < profile.components.Count; j++)
					{
						VolumeComponent profileComponent = profile.components[j];
						VolumeComponent volumeComponent = componentsDefaultStateList.FirstOrDefault((VolumeComponent x) => x.GetType() == profileComponent.GetType());
						if (volumeComponent != null && profileComponent.active)
						{
							profileComponent.Override(volumeComponent, 1f);
						}
					}
				}
			}
		}
	}

	public void Register(Volume volume)
	{
		m_VolumeCollection.Register(volume, volume.gameObject.layer);
	}

	public void Unregister(Volume volume)
	{
		m_VolumeCollection.Unregister(volume, volume.gameObject.layer);
	}

	public bool IsComponentActiveInMask<T>(LayerMask layerMask) where T : VolumeComponent
	{
		return m_VolumeCollection.IsComponentActiveInMask<T>(layerMask);
	}

	internal void SetLayerDirty(int layer)
	{
		m_VolumeCollection.SetLayerIndexDirty(layer);
	}

	internal void UpdateVolumeLayer(Volume volume, int prevLayer, int newLayer)
	{
		m_VolumeCollection.ChangeLayer(volume, prevLayer, newLayer);
	}

	private void OverrideData(VolumeStack stack, Volume volume, float interpFactor)
	{
		List<VolumeComponent> components = volume.profileRef.components;
		int count = components.Count;
		for (int i = 0; i < count; i++)
		{
			VolumeComponent volumeComponent = components[i];
			if (volumeComponent.active)
			{
				VolumeComponent component = stack.GetComponent(volumeComponent.GetType());
				if (component != null)
				{
					volumeComponent.Override(component, interpFactor);
				}
			}
		}
	}

	internal void ReplaceData(VolumeStack stack)
	{
		using (k_ProfilerMarkerReplaceData.Auto())
		{
			VolumeParameter[] parameters = stack.parameters;
			bool requiresResetForAllProperties = stack.requiresResetForAllProperties;
			int num = parameters.Length;
			for (int i = 0; i < num; i++)
			{
				VolumeParameter volumeParameter = parameters[i];
				if (volumeParameter.overrideState || requiresResetForAllProperties)
				{
					volumeParameter.overrideState = false;
					volumeParameter.SetValue(m_ParametersDefaultState[i]);
				}
			}
			stack.requiresResetForAllProperties = false;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public void CheckDefaultVolumeState()
	{
		if (m_ComponentsDefaultState == null || (m_ComponentsDefaultState.Length != 0 && m_ComponentsDefaultState[0] == null))
		{
			EvaluateVolumeDefaultState();
		}
	}

	[Conditional("UNITY_EDITOR")]
	public void CheckStack(VolumeStack stack)
	{
		if (stack.components == null)
		{
			stack.Reload(baseComponentTypeArray);
			return;
		}
		foreach (KeyValuePair<Type, VolumeComponent> component in stack.components)
		{
			if (component.Key == null || component.Value == null)
			{
				stack.Reload(baseComponentTypeArray);
				break;
			}
		}
	}

	private bool CheckUpdateRequired(VolumeStack stack)
	{
		if (m_VolumeCollection.count == 0)
		{
			if (stack.requiresReset)
			{
				stack.requiresReset = false;
				return true;
			}
			return false;
		}
		stack.requiresReset = true;
		return true;
	}

	public void Update(Transform trigger, LayerMask layerMask)
	{
		Update(stack, trigger, layerMask);
	}

	public void Update(VolumeStack stack, Transform trigger, LayerMask layerMask)
	{
		using (k_ProfilerMarkerUpdate.Auto())
		{
			if (!isInitialized || !CheckUpdateRequired(stack))
			{
				return;
			}
			ReplaceData(stack);
			bool flag = trigger == null;
			Vector3 vector = (flag ? Vector3.zero : trigger.position);
			List<Volume> list = GrabVolumes(layerMask);
			Camera component = null;
			if (!flag)
			{
				trigger.TryGetComponent<Camera>(out component);
			}
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				Volume volume = list[i];
				if (volume == null || !volume.enabled || volume.profileRef == null || volume.weight <= 0f)
				{
					continue;
				}
				if (volume.isGlobal)
				{
					OverrideData(stack, volume, Mathf.Clamp01(volume.weight));
				}
				else
				{
					if (flag)
					{
						continue;
					}
					List<Collider> colliders = volume.colliders;
					int count2 = colliders.Count;
					if (count2 == 0)
					{
						continue;
					}
					float num = float.PositiveInfinity;
					for (int j = 0; j < count2; j++)
					{
						Collider collider = colliders[j];
						if (collider.enabled)
						{
							float sqrMagnitude = (collider.ClosestPoint(vector) - vector).sqrMagnitude;
							if (sqrMagnitude < num)
							{
								num = sqrMagnitude;
							}
						}
					}
					float num2 = volume.blendDistance * volume.blendDistance;
					if (!(num > num2))
					{
						float num3 = 1f;
						if (num2 > 0f)
						{
							num3 = 1f - num / num2;
						}
						OverrideData(stack, volume, num3 * Mathf.Clamp01(volume.weight));
					}
				}
			}
		}
	}

	public Volume[] GetVolumes(LayerMask layerMask)
	{
		List<Volume> list = GrabVolumes(layerMask);
		list.RemoveAll((Volume v) => v == null);
		return list.ToArray();
	}

	private List<Volume> GrabVolumes(LayerMask mask)
	{
		return m_VolumeCollection.GrabVolumes(mask);
	}

	private static bool IsVolumeRenderedByCamera(Volume volume, Camera camera)
	{
		return true;
	}
}
