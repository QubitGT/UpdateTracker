using System;
using UnityEngine.InputSystem;

namespace UnityEngine.VFX.Utility;

[RequireComponent(typeof(Collider))]
internal class VFXMouseEventBinder : VFXEventBinderBase
{
	public enum Activation
	{
		OnMouseUp,
		OnMouseDown,
		OnMouseEnter,
		OnMouseExit,
		OnMouseOver,
		OnMouseDrag
	}

	public Activation activation = Activation.OnMouseDown;

	private ExposedProperty position = "position";

	[Tooltip("Computes intersection in world space and sets it to the position EventAttribute")]
	public bool RaycastMousePosition;

	private InputAction mouseDown;

	private InputAction mouseUp;

	private InputAction mouseDragStart;

	private InputAction mouseDragStop;

	private InputAction mouseEnter;

	private bool mouseOver;

	private bool drag;

	protected override void SetEventAttribute(object[] parameters)
	{
		if (RaycastMousePosition)
		{
			Ray ray = Camera.main.ScreenPointToRay(GetMousePosition());
			if (GetComponent<Collider>().Raycast(ray, out var hitInfo, float.MaxValue))
			{
				eventAttribute.SetVector3(position, hitInfo.point);
			}
		}
	}

	private void Awake()
	{
		InputActionMap map = new InputActionMap("VFX Mouse Event Binder");
		mouseDown = map.AddAction("Mouse Down", InputActionType.Value, "<Mouse>/leftButton", "press(behavior=0)");
		mouseDown.performed += delegate
		{
			RayCastAndTriggerEvent(DoOnMouseDown);
		};
		mouseUp = map.AddAction("Mouse Up", InputActionType.Value, "<Mouse>/leftButton", "press(behavior=1)");
		mouseUp.performed += delegate
		{
			RayCastAndTriggerEvent(DoOnMouseUp);
		};
		mouseDragStart = map.AddAction("Mouse Drag Start", InputActionType.Value, "<Mouse>/leftButton", "press(behavior=0)");
		mouseDragStop = map.AddAction("Mouse Drag Stop", InputActionType.Value, "<Mouse>/leftButton", "press(behavior=1)");
	}

	private void RaycastMainCamera()
	{
		Ray ray = Camera.main.ScreenPointToRay(GetMousePosition());
		RaycastHit hitInfo;
		bool flag = GetComponent<Collider>().Raycast(ray, out hitInfo, float.MaxValue);
		if (mouseOver != flag)
		{
			mouseOver = flag;
			if (flag)
			{
				DoOnMouseOver();
			}
			else
			{
				DoOnMouseExit();
			}
		}
	}

	private void RayCastDrag()
	{
		RayCastAndTriggerEvent(DoOnMouseDrag);
	}

	private void RayCastAndTriggerEvent(Action trigger)
	{
		Ray ray = Camera.main.ScreenPointToRay(GetMousePosition());
		if (GetComponent<Collider>().Raycast(ray, out var _, float.MaxValue))
		{
			trigger();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		mouseDown.Enable();
		mouseUp.Enable();
		mouseDragStart.Enable();
	}

	private void OnDisable()
	{
		mouseDown.Disable();
		mouseUp.Disable();
		mouseDragStart.Disable();
	}

	private static Vector2 GetMousePosition()
	{
		return Pointer.current.position.ReadValue();
	}

	private void DoOnMouseDown()
	{
		if (activation == Activation.OnMouseDown)
		{
			SendEventToVisualEffect();
		}
	}

	private void DoOnMouseUp()
	{
		if (activation == Activation.OnMouseUp)
		{
			SendEventToVisualEffect();
		}
	}

	private void DoOnMouseDrag()
	{
		if (activation == Activation.OnMouseDrag)
		{
			SendEventToVisualEffect();
		}
	}

	private void DoOnMouseOver()
	{
		if (activation == Activation.OnMouseOver)
		{
			SendEventToVisualEffect();
		}
	}

	private void DoOnMouseEnter()
	{
		if (activation == Activation.OnMouseEnter)
		{
			SendEventToVisualEffect();
		}
	}

	private void DoOnMouseExit()
	{
		if (activation == Activation.OnMouseExit)
		{
			SendEventToVisualEffect();
		}
	}

	private void OnMouseDown()
	{
		DoOnMouseDown();
	}

	private void OnMouseUp()
	{
		DoOnMouseUp();
	}

	private void OnMouseDrag()
	{
		DoOnMouseDrag();
	}

	private void OnMouseOver()
	{
		DoOnMouseOver();
	}

	private void OnMouseEnter()
	{
		DoOnMouseEnter();
	}

	private void OnMouseExit()
	{
		DoOnMouseExit();
	}
}
