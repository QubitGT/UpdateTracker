using UnityEngine;
using UnityEngine.InputSystem;

namespace Liv.Lck.GorillaTag;

public class DroneMouse
{
	public delegate void DroneMouseEvent();

	public delegate void DroneMouseMoveEvent(Vector2 delta);

	public delegate void DroneMouseMiddleScrollEvent(float delta);

	public event DroneMouseMoveEvent OnMouseMoveLeft;

	public event DroneMouseMoveEvent OnMouseMoveRight;

	public event DroneMouseEvent OnReset;

	public event DroneMouseMiddleScrollEvent OnMouseMiddleScroll;

	public event DroneMouseEvent OnMouseScrollUp;

	public event DroneMouseEvent OnMouseScrollDown;

	public void Run()
	{
		Mouse current = Mouse.current;
		if (current == null)
		{
			return;
		}
		bool isPressed = current.leftButton.isPressed;
		bool isPressed2 = current.rightButton.isPressed;
		if (isPressed && !isPressed2)
		{
			Vector2 delta = current.delta.ReadValue();
			this.OnMouseMoveLeft?.Invoke(delta);
		}
		if (isPressed2 && !isPressed)
		{
			Vector2 delta2 = current.delta.ReadValue();
			this.OnMouseMoveRight?.Invoke(delta2);
		}
		if (current.middleButton.wasPressedThisFrame)
		{
			this.OnReset?.Invoke();
		}
		if (!current.middleButton.isPressed)
		{
			return;
		}
		float y = current.scroll.ReadValue().y;
		this.OnMouseMiddleScroll?.Invoke(y);
		if (!Mathf.Approximately(y, 0f))
		{
			if (y > 0f)
			{
				this.OnMouseScrollUp?.Invoke();
			}
			else if (y < 0f)
			{
				this.OnMouseScrollDown?.Invoke();
			}
		}
	}
}
