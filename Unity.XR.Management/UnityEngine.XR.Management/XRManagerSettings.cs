using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace UnityEngine.XR.Management;

public sealed class XRManagerSettings : ScriptableObject
{
	[HideInInspector]
	private bool m_InitializationComplete;

	[HideInInspector]
	[SerializeField]
	private bool m_RequiresSettingsUpdate;

	[SerializeField]
	[Tooltip("Determines if the XR Manager instance is responsible for creating and destroying the appropriate loader instance.")]
	[FormerlySerializedAs("AutomaticLoading")]
	private bool m_AutomaticLoading;

	[SerializeField]
	[Tooltip("Determines if the XR Manager instance is responsible for starting and stopping subsystems for the active loader instance.")]
	[FormerlySerializedAs("AutomaticRunning")]
	private bool m_AutomaticRunning;

	[SerializeField]
	[Tooltip("List of XR Loader instances arranged in desired load order.")]
	[FormerlySerializedAs("Loaders")]
	private List<XRLoader> m_Loaders = new List<XRLoader>();

	[SerializeField]
	[HideInInspector]
	private HashSet<XRLoader> m_RegisteredLoaders = new HashSet<XRLoader>();

	public bool automaticLoading
	{
		get
		{
			return m_AutomaticLoading;
		}
		set
		{
			m_AutomaticLoading = value;
		}
	}

	public bool automaticRunning
	{
		get
		{
			return m_AutomaticRunning;
		}
		set
		{
			m_AutomaticRunning = value;
		}
	}

	[Obsolete("'XRManagerSettings.loaders' property is obsolete. Use 'XRManagerSettings.activeLoaders' instead to get a list of the current loaders.")]
	public List<XRLoader> loaders => m_Loaders;

	public IReadOnlyList<XRLoader> activeLoaders => m_Loaders;

	public bool isInitializationComplete => m_InitializationComplete;

	[HideInInspector]
	public XRLoader activeLoader { get; private set; }

	internal List<XRLoader> currentLoaders
	{
		get
		{
			return m_Loaders;
		}
		set
		{
			m_Loaders = value;
		}
	}

	internal HashSet<XRLoader> registeredLoaders => m_RegisteredLoaders;

	public T ActiveLoaderAs<T>() where T : XRLoader
	{
		return activeLoader as T;
	}

	public void InitializeLoaderSync()
	{
		if (activeLoader != null)
		{
			Debug.LogWarning("XR Management has already initialized an active loader in this scene. Please make sure to stop all subsystems and deinitialize the active loader before initializing a new one.");
			return;
		}
		foreach (XRLoader currentLoader in currentLoaders)
		{
			if (currentLoader != null && CheckGraphicsAPICompatibility(currentLoader) && currentLoader.Initialize())
			{
				activeLoader = currentLoader;
				m_InitializationComplete = true;
				return;
			}
		}
		activeLoader = null;
	}

	public IEnumerator InitializeLoader()
	{
		if (activeLoader != null)
		{
			Debug.LogWarning("XR Management has already initialized an active loader in this scene. Please make sure to stop all subsystems and deinitialize the active loader before initializing a new one.");
			yield break;
		}
		foreach (XRLoader currentLoader in currentLoaders)
		{
			if (currentLoader != null && CheckGraphicsAPICompatibility(currentLoader) && currentLoader.Initialize())
			{
				activeLoader = currentLoader;
				m_InitializationComplete = true;
				yield break;
			}
			yield return null;
		}
		activeLoader = null;
	}

	public bool TryAddLoader(XRLoader loader, int index = -1)
	{
		if (loader == null || currentLoaders.Contains(loader))
		{
			return false;
		}
		if (!m_RegisteredLoaders.Contains(loader))
		{
			return false;
		}
		if (index < 0 || index >= currentLoaders.Count)
		{
			currentLoaders.Add(loader);
		}
		else
		{
			currentLoaders.Insert(index, loader);
		}
		return true;
	}

	public bool TryRemoveLoader(XRLoader loader)
	{
		bool result = true;
		if (currentLoaders.Contains(loader))
		{
			result = currentLoaders.Remove(loader);
		}
		return result;
	}

	public bool TrySetLoaders(List<XRLoader> reorderedLoaders)
	{
		List<XRLoader> list = new List<XRLoader>(activeLoaders);
		currentLoaders.Clear();
		foreach (XRLoader reorderedLoader in reorderedLoaders)
		{
			if (!TryAddLoader(reorderedLoader))
			{
				currentLoaders = list;
				return false;
			}
		}
		return true;
	}

	private void Awake()
	{
		foreach (XRLoader currentLoader in currentLoaders)
		{
			if (!m_RegisteredLoaders.Contains(currentLoader))
			{
				m_RegisteredLoaders.Add(currentLoader);
			}
		}
	}

	private bool CheckGraphicsAPICompatibility(XRLoader loader)
	{
		GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
		List<GraphicsDeviceType> supportedGraphicsDeviceTypes = loader.GetSupportedGraphicsDeviceTypes(buildingPlayer: false);
		if (supportedGraphicsDeviceTypes.Count > 0 && !supportedGraphicsDeviceTypes.Contains(graphicsDeviceType))
		{
			Debug.LogWarning($"The {loader.name} does not support the initialized graphics device, {graphicsDeviceType.ToString()}. Please change the preffered Graphics API in PlayerSettings. Attempting to start the next XR loader.");
			return false;
		}
		return true;
	}

	public void StartSubsystems()
	{
		if (!m_InitializationComplete)
		{
			Debug.LogWarning("Call to StartSubsystems without an initialized manager.Please make sure wait for initialization to complete before calling this API.");
		}
		else if (activeLoader != null)
		{
			activeLoader.Start();
		}
	}

	public void StopSubsystems()
	{
		if (!m_InitializationComplete)
		{
			Debug.LogWarning("Call to StopSubsystems without an initialized manager.Please make sure wait for initialization to complete before calling this API.");
		}
		else if (activeLoader != null)
		{
			activeLoader.Stop();
		}
	}

	public void DeinitializeLoader()
	{
		if (!m_InitializationComplete)
		{
			Debug.LogWarning("Call to DeinitializeLoader without an initialized manager.Please make sure wait for initialization to complete before calling this API.");
			return;
		}
		StopSubsystems();
		if (activeLoader != null)
		{
			activeLoader.Deinitialize();
			activeLoader = null;
		}
		m_InitializationComplete = false;
	}

	private void Start()
	{
		if (automaticLoading && automaticRunning)
		{
			StartSubsystems();
		}
	}

	private void OnDisable()
	{
		if (automaticLoading && automaticRunning)
		{
			StopSubsystems();
		}
	}

	private void OnDestroy()
	{
		if (automaticLoading)
		{
			DeinitializeLoader();
		}
	}
}
