using System.Collections;
using UnityEngine;
using UnityEngine.XR;

namespace Valve.VR;

public class SteamVR_Behaviour : MonoBehaviour
{
	private const string openVRDeviceName = "OpenVR";

	public static bool forcingInitialization = false;

	private static SteamVR_Behaviour _instance;

	public bool initializeSteamVROnAwake = true;

	public bool doNotDestroy = true;

	[HideInInspector]
	public SteamVR_Render steamvr_render;

	internal static bool isPlaying = false;

	private static bool initializing = false;

	private Coroutine initializeCoroutine;

	private bool loadedOpenVRDeviceSuccess;

	protected static int lastFrameCount = -1;

	public static SteamVR_Behaviour instance
	{
		get
		{
			if (_instance == null)
			{
				Initialize();
			}
			return _instance;
		}
	}

	public static void Initialize(bool forceUnityVRToOpenVR = false)
	{
		if (!(_instance == null) || initializing)
		{
			return;
		}
		initializing = true;
		GameObject gameObject = null;
		if (forceUnityVRToOpenVR)
		{
			forcingInitialization = true;
		}
		SteamVR_Render steamVR_Render = Object.FindFirstObjectByType<SteamVR_Render>();
		if (steamVR_Render != null)
		{
			gameObject = steamVR_Render.gameObject;
		}
		SteamVR_Behaviour steamVR_Behaviour = Object.FindFirstObjectByType<SteamVR_Behaviour>();
		if (steamVR_Behaviour != null)
		{
			gameObject = steamVR_Behaviour.gameObject;
		}
		if (gameObject == null)
		{
			GameObject gameObject2 = new GameObject("[SteamVR]");
			_instance = gameObject2.AddComponent<SteamVR_Behaviour>();
			_instance.steamvr_render = gameObject2.AddComponent<SteamVR_Render>();
		}
		else
		{
			steamVR_Behaviour = gameObject.GetComponent<SteamVR_Behaviour>();
			if (steamVR_Behaviour == null)
			{
				steamVR_Behaviour = gameObject.AddComponent<SteamVR_Behaviour>();
			}
			if (steamVR_Render != null)
			{
				steamVR_Behaviour.steamvr_render = steamVR_Render;
			}
			else
			{
				steamVR_Behaviour.steamvr_render = gameObject.GetComponent<SteamVR_Render>();
				if (steamVR_Behaviour.steamvr_render == null)
				{
					steamVR_Behaviour.steamvr_render = gameObject.AddComponent<SteamVR_Render>();
				}
			}
			_instance = steamVR_Behaviour;
		}
		if (_instance != null && _instance.doNotDestroy)
		{
			Object.DontDestroyOnLoad(_instance.transform.root.gameObject);
		}
		initializing = false;
	}

	protected void Awake()
	{
		isPlaying = true;
		if (initializeSteamVROnAwake && !forcingInitialization)
		{
			InitializeSteamVR();
		}
	}

	public void InitializeSteamVR(bool forceUnityVRToOpenVR = false)
	{
		if (forceUnityVRToOpenVR)
		{
			forcingInitialization = true;
			if (initializeCoroutine != null)
			{
				StopCoroutine(initializeCoroutine);
			}
			if (XRSettings.loadedDeviceName == "OpenVR")
			{
				EnableOpenVR();
			}
			else
			{
				initializeCoroutine = StartCoroutine(DoInitializeSteamVR(forceUnityVRToOpenVR));
			}
		}
		else
		{
			SteamVR.Initialize();
		}
	}

	private IEnumerator DoInitializeSteamVR(bool forceUnityVRToOpenVR = false)
	{
		XRDevice.deviceLoaded += XRDevice_deviceLoaded;
		XRSettings.LoadDeviceByName("OpenVR");
		while (!loadedOpenVRDeviceSuccess)
		{
			yield return null;
		}
		XRDevice.deviceLoaded -= XRDevice_deviceLoaded;
		EnableOpenVR();
	}

	private void XRDevice_deviceLoaded(string deviceName)
	{
		if (deviceName == "OpenVR")
		{
			loadedOpenVRDeviceSuccess = true;
			return;
		}
		Debug.LogError("<b>[SteamVR]</b> Tried to async load: OpenVR. Loaded: " + deviceName, this);
		loadedOpenVRDeviceSuccess = true;
	}

	private void EnableOpenVR()
	{
		XRSettings.enabled = true;
		SteamVR.Initialize();
		initializeCoroutine = null;
		forcingInitialization = false;
	}

	protected void OnEnable()
	{
		Application.onBeforeRender += OnBeforeRender;
		SteamVR_Events.System(EVREventType.VREvent_Quit).Listen(OnQuit);
	}

	protected void OnDisable()
	{
		Application.onBeforeRender -= OnBeforeRender;
		SteamVR_Events.System(EVREventType.VREvent_Quit).Remove(OnQuit);
	}

	protected void OnBeforeRender()
	{
		PreCull();
	}

	protected void PreCull()
	{
		if (OpenVR.Input != null && Time.frameCount != lastFrameCount)
		{
			lastFrameCount = Time.frameCount;
			SteamVR_Input.OnPreCull();
		}
	}

	protected void FixedUpdate()
	{
		if (OpenVR.Input != null)
		{
			SteamVR_Input.FixedUpdate();
		}
	}

	protected void LateUpdate()
	{
		if (OpenVR.Input != null)
		{
			SteamVR_Input.LateUpdate();
		}
	}

	protected void Update()
	{
		if (OpenVR.Input != null)
		{
			SteamVR_Input.Update();
		}
	}

	protected void OnQuit(VREvent_t vrEvent)
	{
		Application.Quit();
	}
}
