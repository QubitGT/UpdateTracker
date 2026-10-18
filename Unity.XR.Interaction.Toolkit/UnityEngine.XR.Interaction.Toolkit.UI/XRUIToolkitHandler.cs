using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputForUI;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

internal static class XRUIToolkitHandler
{
	private class InteractorInfo
	{
		public IXRInteractor interactor;

		public int index;
	}

	private const int k_MaxInteractors = 8;

	private const int k_InvalidIndex = -1;

	private static readonly Vector3 k_ResetPos = new Vector3(0f, -1000f, 0f);

	private static readonly Dictionary<IXRInteractor, InteractorInfo> s_RegisteredInteractors = new Dictionary<IXRInteractor, InteractorInfo>();

	private static readonly Dictionary<IXRInteractor, InteractorHitData> s_InteractorHitData = new Dictionary<IXRInteractor, InteractorHitData>();

	private static readonly bool[] s_UsedIndices = new bool[8];

	private static readonly Dictionary<int, bool> s_LastWasDown = new Dictionary<int, bool>();

	private static readonly Dictionary<int, bool> s_WasReset = new Dictionary<int, bool>();

	private static PanelInputConfiguration s_PanelInputConfigurationRef;

	private static bool s_EventSystemValidated;

	private static bool s_PanelInputConfigurationValidated;

	private static bool s_DidCheckPanelInputConfiguration;

	private static readonly Dictionary<IXRInteractor, VisualElement> s_InteractorElements = new Dictionary<IXRInteractor, VisualElement>();

	private static readonly Dictionary<uint, float> s_InitialZDepth = new Dictionary<uint, float>();

	public static bool uiToolkitSupportEnabled { get; set; }

	public static int count => s_RegisteredInteractors.Count;

	public static int Register(IXRInteractor interactor)
	{
		if (s_RegisteredInteractors.TryGetValue(interactor, out var value))
		{
			Debug.LogWarning($"interactor {interactor} is already registered with XR UI Toolkit Handler.");
			return value.index;
		}
		int num = -1;
		for (int i = 0; i < 8; i++)
		{
			if (!s_UsedIndices[i])
			{
				num = i;
				s_UsedIndices[i] = true;
				break;
			}
		}
		if (num == -1)
		{
			Debug.LogError("No available indices for pointer registration.");
			return -1;
		}
		InteractorInfo value2 = new InteractorInfo
		{
			interactor = interactor,
			index = num
		};
		s_RegisteredInteractors.Add(interactor, value2);
		return num;
	}

	public static void Unregister(IXRInteractor interactor)
	{
		if (s_RegisteredInteractors.TryGetValue(interactor, out var value))
		{
			s_LastWasDown.Remove(value.index);
			s_WasReset.Remove(value.index);
			s_UsedIndices[value.index] = false;
			s_RegisteredInteractors.Remove(interactor);
			s_InteractorHitData.Remove(interactor);
			ClearZDepthForInteractor(interactor);
		}
	}

	public static bool TryGetPointerIndex(IXRInteractor interactor, out int index)
	{
		if (s_RegisteredInteractors.TryGetValue(interactor, out var value))
		{
			index = value.index;
			return true;
		}
		index = -1;
		return false;
	}

	public static void UpdateInteractorHitData(IXRInteractor interactor, InteractorHitData hitData)
	{
		s_InteractorHitData[interactor] = hitData;
	}

	public static bool TryGetInteractorHitData(IXRInteractor interactor, out InteractorHitData hitData)
	{
		return s_InteractorHitData.TryGetValue(interactor, out hitData);
	}

	public static void ClearInteractorHitData(IXRInteractor interactor)
	{
		ClearZDepthForInteractor(interactor);
		s_InteractorHitData.Remove(interactor);
	}

	public static void Clear()
	{
		s_RegisteredInteractors.Clear();
		s_LastWasDown.Clear();
		s_WasReset.Clear();
		s_InteractorHitData.Clear();
		s_InteractorElements.Clear();
		for (int i = 0; i < 8; i++)
		{
			s_UsedIndices[i] = false;
		}
	}

	public static bool IsRegistered(IXRInteractor interactor)
	{
		return s_RegisteredInteractors.ContainsKey(interactor);
	}

	public static void HandlePointerUpdate(IXRInteractor interactor, Vector3 pos, Quaternion rot, bool isUiSelectInputActive, bool shouldReset)
	{
		if (!TryGetPointerIndex(interactor, out var index))
		{
			return;
		}
		s_LastWasDown.TryAdd(index, value: false);
		s_WasReset.TryAdd(index, shouldReset);
		if (!shouldReset || !s_WasReset[index])
		{
			if (ShouldCheckPanelInputConfigurationValidation())
			{
				ValidatePanelInputConfiguration();
			}
			Vector3 worldPosition = (shouldReset ? k_ResetPos : pos);
			Quaternion worldOrientation = (shouldReset ? Quaternion.identity : rot);
			PointerEvent pointerEvent = default(PointerEvent);
			pointerEvent.pointerIndex = index;
			pointerEvent.type = PointerEvent.Type.PointerMoved;
			pointerEvent.worldPosition = worldPosition;
			pointerEvent.worldOrientation = worldOrientation;
			pointerEvent.eventSource = EventSource.TrackedDevice;
			pointerEvent.maxDistance = 10f;
			Event ev = Event.From(pointerEvent);
			EventProvider.Dispatch(in ev);
			bool flag = !shouldReset && isUiSelectInputActive;
			if (flag != s_LastWasDown[index])
			{
				s_LastWasDown[index] = flag;
				PointerEvent.Type type = (flag ? PointerEvent.Type.ButtonPressed : PointerEvent.Type.ButtonReleased);
				pointerEvent = default(PointerEvent);
				pointerEvent.pointerIndex = index;
				pointerEvent.type = type;
				pointerEvent.button = PointerEvent.Button.Primary;
				pointerEvent.clickCount = 1;
				pointerEvent.worldPosition = worldPosition;
				pointerEvent.worldOrientation = worldOrientation;
				pointerEvent.eventSource = EventSource.TrackedDevice;
				pointerEvent.maxDistance = 10f;
				ev = Event.From(pointerEvent);
				EventProvider.Dispatch(in ev);
			}
			s_WasReset[index] = shouldReset;
			if (shouldReset)
			{
				ClearInteractorHitData(interactor);
			}
		}
	}

	public static bool TryGetPointerHitData(IXRInteractor interactor, out PointerHitData hitData)
	{
		hitData = default(PointerHitData);
		if (!TryGetPointerIndex(interactor, out var index))
		{
			return false;
		}
		PointerDeviceState.TrackedPointerState trackedState = PointerDeviceState.GetTrackedState(PointerId.trackedPointerIdBase + index);
		if (trackedState == null)
		{
			return false;
		}
		hitData = new PointerHitData
		{
			worldPosition = trackedState.worldPosition,
			worldOrientation = trackedState.worldOrientation,
			hitDistance = trackedState.hit.distance,
			hitCollider = trackedState.hit.collider,
			hitDocument = trackedState.hit.document,
			hitElement = trackedState.hit.element
		};
		return true;
	}

	public static float SetZDepthForInteractor(VisualElement ve, IXRInteractor interactor, float z)
	{
		s_InteractorElements[interactor] = ve;
		Translate value = ve.style.translate.value;
		if (!s_InitialZDepth.TryAdd(ve.controlid, value.z))
		{
			s_InitialZDepth[ve.controlid] = value.z;
		}
		ve.style.translate = new Translate(value.x.value, value.y.value, z);
		return z;
	}

	private static float ResetDepth(VisualElement ve)
	{
		Translate value = ve.style.translate.value;
		if (s_InitialZDepth.TryGetValue(ve.controlid, out var value2))
		{
			ve.style.translate = new Translate(value.x.value, value.y.value, value2);
		}
		else
		{
			ve.style.translate = new Translate(value.x.value, value.y.value, 0f);
		}
		return value2;
	}

	public static void ClearZDepthForInteractor(IXRInteractor interactor)
	{
		if (s_InteractorElements.TryGetValue(interactor, out var value) && value != null)
		{
			ResetDepth(value);
			s_InteractorElements.Remove(interactor);
		}
	}

	public static void UpdateEventSystem()
	{
		if (count > 0)
		{
			UIElementsRuntimeUtility.UpdateEventSystem();
		}
	}

	public static bool IsValidUIToolkitInteraction(List<Collider> colliders)
	{
		if (colliders.Count > 0)
		{
			return HasUIDocument(colliders[0]);
		}
		return false;
	}

	public static bool HasUIDocument(Collider collider)
	{
		UIDocument component;
		return collider.TryGetComponent<UIDocument>(out component);
	}

	private static bool ShouldCheckPanelInputConfigurationValidation()
	{
		if (s_PanelInputConfigurationRef != PanelInputConfiguration.current)
		{
			s_EventSystemValidated = false;
			s_PanelInputConfigurationValidated = false;
			s_DidCheckPanelInputConfiguration = false;
			return true;
		}
		return !s_DidCheckPanelInputConfiguration && (!s_EventSystemValidated || !s_PanelInputConfigurationValidated);
	}

	private static void ValidatePanelInputConfiguration()
	{
		s_DidCheckPanelInputConfiguration = true;
		s_PanelInputConfigurationValidated = false;
		if (s_EventSystemValidated || !(EventSystem.current == null))
		{
			s_EventSystemValidated = true;
			PanelInputConfiguration panelInputConfiguration = (s_PanelInputConfigurationRef = PanelInputConfiguration.current);
			if (panelInputConfiguration == null)
			{
				Debug.LogWarning("Detected an Event System component that could interfere with UI Toolkit input. Create a Panel Input Configuration component and configured it by setting Panel Input Redirection to No input redirection to prevent interactions with the Event System.");
			}
			else if (panelInputConfiguration.panelInputRedirection != PanelInputConfiguration.PanelInputRedirection.Never)
			{
				Debug.LogWarning("Detected an Event System component that could interfere with UI Toolkit input. Configure your Panel Input Configuration component to set Panel Input Redirection to No input redirection to prevent interactions with the Event System.");
			}
			else
			{
				s_PanelInputConfigurationValidated = true;
			}
		}
	}
}
