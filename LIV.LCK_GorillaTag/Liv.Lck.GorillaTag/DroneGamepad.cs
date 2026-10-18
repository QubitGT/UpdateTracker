using UnityEngine;
using UnityEngine.InputSystem;

namespace Liv.Lck.GorillaTag;

public class DroneGamepad
{
	public delegate void Gamepad2DMove(Vector2 move);

	public delegate void Gamepad1DMove(float move);

	public event Gamepad2DMove OnMove;

	public event Gamepad2DMove OnTiltAndRotate;

	public event Gamepad1DMove OnMoveUpAndDown;

	public void Run()
	{
		Gamepad current = Gamepad.current;
		if (current != null)
		{
			Vector2 move = current.leftStick.ReadValue();
			Vector2 move2 = current.rightStick.ReadValue();
			this.OnMove?.Invoke(move);
			this.OnTiltAndRotate?.Invoke(move2);
			float num = current.leftTrigger.ReadValue();
			float num2 = current.rightTrigger.ReadValue() - num;
			if (Mathf.Abs(num2) > 0.05f)
			{
				this.OnMoveUpAndDown?.Invoke(num2);
			}
		}
	}
}
