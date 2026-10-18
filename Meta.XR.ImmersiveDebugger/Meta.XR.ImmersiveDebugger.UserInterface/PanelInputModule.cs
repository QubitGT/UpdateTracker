using System.Collections.Generic;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal class PanelInputModule : OVRInputModule
{
	public class RaycastComparer : IComparer<RaycastResult>
	{
		public int Compare(RaycastResult lhs, RaycastResult rhs)
		{
			PanelRaycaster panelRaycaster = lhs.module as PanelRaycaster;
			PanelRaycaster panelRaycaster2 = rhs.module as PanelRaycaster;
			if (panelRaycaster != null && panelRaycaster2 != null && panelRaycaster.sortOrder != panelRaycaster2.sortOrder)
			{
				return panelRaycaster2.sortOrder.CompareTo(panelRaycaster.sortOrder);
			}
			if (lhs.depth != rhs.depth && lhs.module.rootRaycaster == rhs.module.rootRaycaster)
			{
				return rhs.depth.CompareTo(lhs.depth);
			}
			if (lhs.distance != rhs.distance)
			{
				return lhs.distance.CompareTo(rhs.distance);
			}
			return lhs.index.CompareTo(rhs.index);
		}
	}

	internal static bool Processing;

	private Interface _debugInterface;

	private OVRInput.Controller _controller;

	private static OVRPlugin.HandState _handState = default(OVRPlugin.HandState);

	private static readonly List<PanelRaycaster> _raycasters = new List<PanelRaycaster>();

	private static IComparer<RaycastResult> _comparer = new RaycastComparer();

	public static void RegisterRaycaster(PanelRaycaster raycaster)
	{
		if (!_raycasters.Contains(raycaster))
		{
			_raycasters.Add(raycaster);
		}
	}

	public static void UnregisterRaycaster(PanelRaycaster raycaster)
	{
		if (_raycasters.Contains(raycaster))
		{
			_raycasters.Remove(raycaster);
		}
	}

	internal void SetDebugInterface(Interface debugInterface)
	{
		_debugInterface = debugInterface;
	}

	protected override void Awake()
	{
		GameObject gameObject = new GameObject("rayHelper");
		rayTransform = gameObject.transform;
		rayTransform.SetParent(base.transform);
	}

	public override bool ShouldActivateModule()
	{
		return false;
	}

	public override bool IsModuleSupported()
	{
		return false;
	}

	private void Update()
	{
		if (!_debugInterface || _debugInterface.Visibility)
		{
			Process();
		}
	}

	private bool Raycast(PointerEventData data, out RaycastResult raycast)
	{
		foreach (PanelRaycaster raycaster in _raycasters)
		{
			if (raycaster.IsValid)
			{
				raycaster.RaycastOnRaycastableGraphics(data, m_RaycastResultCache);
			}
		}
		m_RaycastResultCache.Sort(_comparer);
		raycast = BaseInputModule.FindFirstRaycast(m_RaycastResultCache);
		data.pointerCurrentRaycast = raycast;
		m_RaycastResultCache.Clear();
		return raycast.isValid;
	}

	private MouseState GetMouseStateFromRaycast(OVRInput.Controller controller, Transform rayOrigin)
	{
		if ((bool)m_Cursor)
		{
			m_Cursor.SetCursorRay(rayOrigin);
		}
		GetPointerData(-1, out var data, create: true);
		data.Reset();
		data.worldSpaceRay = new Ray(rayOrigin.position, rayOrigin.forward);
		data.scrollDelta = GetExtraScrollDelta();
		data.button = PointerEventData.InputButton.Left;
		data.useDragThreshold = true;
		if (Raycast(data, out var raycast))
		{
			PanelRaycaster panelRaycaster = raycast.module as PanelRaycaster;
			if ((bool)panelRaycaster)
			{
				data.position = panelRaycaster.GetScreenPosition(raycast);
				if ((bool)m_Cursor && raycast.gameObject.TryGetComponent<RectTransform>(out var component))
				{
					Vector3 worldPosition = raycast.worldPosition;
					Vector3 rectTransformNormal = OVRInputModule.GetRectTransformNormal(component);
					m_Cursor.SetCursorStartDest(rayOrigin.position, worldPosition, rectTransformNormal);
				}
			}
		}
		GetPointerData(-2, out var data2, create: true);
		CopyFromTo(data, data2);
		data2.button = PointerEventData.InputButton.Right;
		GetPointerData(-3, out var data3, create: true);
		CopyFromTo(data, data3);
		data3.button = PointerEventData.InputButton.Middle;
		PointerEventData.FramePressState framePressState = ComputeControllerState(controller);
		if (m_Cursor is Meta.XR.ImmersiveDebugger.UserInterface.Generic.Cursor cursor)
		{
			cursor.SetClickState(framePressState);
		}
		m_MouseState.SetButtonState(PointerEventData.InputButton.Left, framePressState, data);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Right, PointerEventData.FramePressState.NotChanged, data2);
		m_MouseState.SetButtonState(PointerEventData.InputButton.Middle, PointerEventData.FramePressState.NotChanged, data3);
		return m_MouseState;
	}

	public override void Process()
	{
		Processing = true;
		_controller = ChooseBestController(_controller);
		UpdateRayTransform(rayTransform, _controller);
		ProcessMouseEvent(GetMouseStateFromRaycast(_controller, rayTransform));
		_objectsHitThisFrame.Clear();
		Processing = false;
	}

	private static PointerEventData.FramePressState ComputeControllerState(OVRInput.Controller controller)
	{
		OVRInput.Button clickButton = RuntimeSettings.Instance.ClickButton;
		bool down = OVRInput.GetDown(clickButton, controller);
		bool up = OVRInput.GetUp(clickButton, controller);
		if (down && up)
		{
			return PointerEventData.FramePressState.PressedAndReleased;
		}
		if (down)
		{
			return PointerEventData.FramePressState.Pressed;
		}
		if (up)
		{
			return PointerEventData.FramePressState.Released;
		}
		return PointerEventData.FramePressState.NotChanged;
	}

	private static OVRInput.Controller ChooseBestController(OVRInput.Controller previousController)
	{
		OVRInput.Controller controller = previousController;
		OVRInput.Controller activeControllerForHand = OVRInput.GetActiveControllerForHand(OVRInput.Handedness.LeftHanded);
		OVRInput.Controller activeControllerForHand2 = OVRInput.GetActiveControllerForHand(OVRInput.Handedness.RightHanded);
		if (controller == OVRInput.Controller.None || (controller != activeControllerForHand && controller != activeControllerForHand2))
		{
			controller = ((activeControllerForHand2 == OVRInput.Controller.None) ? activeControllerForHand : ((activeControllerForHand != 0) ? ((OVRInput.GetDominantHand() == OVRInput.Handedness.LeftHanded) ? activeControllerForHand : activeControllerForHand2) : activeControllerForHand2));
		}
		if (controller != activeControllerForHand && OVRInput.Get(OVRInput.Button.Any, activeControllerForHand))
		{
			controller = activeControllerForHand;
		}
		if (controller != activeControllerForHand2 && OVRInput.Get(OVRInput.Button.Any, activeControllerForHand2))
		{
			controller = activeControllerForHand2;
		}
		if (controller == OVRInput.Controller.None)
		{
			controller = OVRInput.Controller.RTouch;
		}
		return controller;
	}

	private void UpdateRayTransform(Transform rayTransform, OVRInput.Controller controller)
	{
		OVRPlugin.Hand hand = controller switch
		{
			OVRInput.Controller.LHand => OVRPlugin.Hand.HandLeft, 
			OVRInput.Controller.RHand => OVRPlugin.Hand.HandRight, 
			_ => OVRPlugin.Hand.None, 
		};
		if (hand != OVRPlugin.Hand.None)
		{
			OVRPlugin.GetHandState(OVRPlugin.Step.Render, hand, ref _handState);
		}
		Vector3 position = controller switch
		{
			OVRInput.Controller.RHand => _handState.PointerPose.Position.FromFlippedZVector3f(), 
			OVRInput.Controller.LHand => _handState.PointerPose.Position.FromFlippedZVector3f(), 
			_ => OVRInput.GetLocalControllerPosition(controller), 
		};
		Quaternion orientation = controller switch
		{
			OVRInput.Controller.RHand => _handState.PointerPose.Orientation.FromFlippedZQuatf(), 
			OVRInput.Controller.LHand => _handState.PointerPose.Orientation.FromFlippedZQuatf(), 
			_ => OVRInput.GetLocalControllerRotation(controller), 
		};
		OVRPose oVRPose = default(OVRPose);
		oVRPose.position = position;
		oVRPose.orientation = orientation;
		OVRPose trackingSpacePose = oVRPose;
		trackingSpacePose = trackingSpacePose.ToWorldSpacePose(_debugInterface.Camera);
		rayTransform.SetPositionAndRotation(trackingSpacePose.position, trackingSpacePose.orientation);
	}
}
