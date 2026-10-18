using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

namespace Unity.XR.OpenVR;

public class OpenVREvents
{
	private static OpenVREvents instance;

	private OpenVREvent[] events;

	private int[] eventIndicies;

	private VREvent_t vrEvent;

	private uint vrEventSize;

	private bool preloadedEvents;

	private const int maxEventsPerUpdate = 64;

	private static bool debugLogAllEvents = false;

	private static bool enabled = true;

	private bool exiting;

	public static void Initialize(bool lazyLoadEvents = false)
	{
		instance = new OpenVREvents(lazyLoadEvents);
	}

	public bool IsInitialized()
	{
		return instance != null;
	}

	public OpenVREvents(bool lazyLoadEvents = false)
	{
		if (OpenVRHelpers.IsUsingSteamVRInput())
		{
			enabled = false;
			return;
		}
		instance = this;
		events = new OpenVREvent[19999];
		vrEvent = default(VREvent_t);
		vrEventSize = (uint)Marshal.SizeOf(typeof(VREvent_t));
		if (!lazyLoadEvents)
		{
			for (int i = 0; i < events.Length; i++)
			{
				events[i] = new OpenVREvent();
			}
		}
		else
		{
			preloadedEvents = true;
		}
		RegisterDefaultEvents();
	}

	public void RegisterDefaultEvents()
	{
		AddListener(EVREventType.VREvent_Quit, On_VREvent_Quit);
	}

	public static void AddListener(EVREventType eventType, UnityAction<VREvent_t> action, bool removeOtherListeners = false)
	{
		instance.Add(eventType, action, removeOtherListeners);
	}

	public void Add(EVREventType eventType, UnityAction<VREvent_t> action, bool removeOtherListeners = false)
	{
		if (!enabled)
		{
			Debug.LogError("[OpenVR XR Plugin] This events class is currently not enabled, please use SteamVR_Events instead.");
			return;
		}
		if (!preloadedEvents && events[(int)eventType] == null)
		{
			events[(int)eventType] = new OpenVREvent();
		}
		if (removeOtherListeners)
		{
			events[(int)eventType].RemoveAllListeners();
		}
		events[(int)eventType].AddListener(action);
	}

	public static void RemoveListener(EVREventType eventType, UnityAction<VREvent_t> action)
	{
		instance.Remove(eventType, action);
	}

	public void Remove(EVREventType eventType, UnityAction<VREvent_t> action)
	{
		if (preloadedEvents || events[(int)eventType] != null)
		{
			events[(int)eventType].RemoveListener(action);
		}
	}

	public static void Update()
	{
		instance.PollEvents();
	}

	public void PollEvents()
	{
		if (Valve.VR.OpenVR.System == null || !enabled)
		{
			return;
		}
		for (int i = 0; i < 64; i++)
		{
			if (Valve.VR.OpenVR.System == null)
			{
				break;
			}
			if (!Valve.VR.OpenVR.System.PollNextEvent(ref vrEvent, vrEventSize))
			{
				break;
			}
			int eventType = (int)vrEvent.eventType;
			if (debugLogAllEvents)
			{
				EVREventType eVREventType = (EVREventType)eventType;
				Debug.Log($"[{Time.frameCount}] {eVREventType.ToString()}");
			}
			if (events[eventType] != null)
			{
				events[eventType].Invoke(vrEvent);
			}
		}
	}

	private void On_VREvent_Quit(VREvent_t pEvent)
	{
		if (!exiting)
		{
			exiting = true;
			if (Valve.VR.OpenVR.System != null)
			{
				Valve.VR.OpenVR.System.AcknowledgeQuit_Exiting();
			}
			Debug.Log("<b>[OpenVR]</b> Quit requested from OpenVR. Exiting application via Application.Quit");
			Application.Quit();
		}
	}
}
