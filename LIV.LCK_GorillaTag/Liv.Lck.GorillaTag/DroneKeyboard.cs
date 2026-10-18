using UnityEngine.InputSystem;

namespace Liv.Lck.GorillaTag;

public class DroneKeyboard
{
	public delegate void DroneKeyboardEvent();

	public event DroneKeyboardEvent OnMoveForward;

	public event DroneKeyboardEvent OnMoveBackward;

	public event DroneKeyboardEvent OnMoveLeft;

	public event DroneKeyboardEvent OnMoveRight;

	public event DroneKeyboardEvent OnMoveUp;

	public event DroneKeyboardEvent OnMoveDown;

	public event DroneKeyboardEvent OnRotateLeft;

	public event DroneKeyboardEvent OnRotateRight;

	public event DroneKeyboardEvent OnTiltUp;

	public event DroneKeyboardEvent OnTiltDown;

	public event DroneKeyboardEvent OnBurstStarted;

	public event DroneKeyboardEvent OnBurstEnded;

	public void Run()
	{
		Keyboard current = Keyboard.current;
		if (current != null)
		{
			if (current.wKey.isPressed)
			{
				this.OnMoveForward?.Invoke();
			}
			else if (current.sKey.isPressed)
			{
				this.OnMoveBackward?.Invoke();
			}
			if (current.aKey.isPressed)
			{
				this.OnMoveLeft?.Invoke();
			}
			else if (current.dKey.isPressed)
			{
				this.OnMoveRight?.Invoke();
			}
			if (current.qKey.isPressed)
			{
				this.OnMoveDown?.Invoke();
			}
			else if (current.eKey.isPressed)
			{
				this.OnMoveUp?.Invoke();
			}
			if (current.leftArrowKey.isPressed)
			{
				this.OnRotateLeft?.Invoke();
			}
			else if (current.rightArrowKey.isPressed)
			{
				this.OnRotateRight?.Invoke();
			}
			if (current.upArrowKey.isPressed)
			{
				this.OnTiltUp?.Invoke();
			}
			else if (current.downArrowKey.isPressed)
			{
				this.OnTiltDown?.Invoke();
			}
			if (current.spaceKey.wasPressedThisFrame)
			{
				this.OnBurstStarted?.Invoke();
			}
			if (current.spaceKey.wasReleasedThisFrame)
			{
				this.OnBurstEnded?.Invoke();
			}
		}
	}
}
