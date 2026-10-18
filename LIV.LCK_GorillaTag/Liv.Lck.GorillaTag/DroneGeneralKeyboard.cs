using UnityEngine.InputSystem;

namespace Liv.Lck.GorillaTag;

public class DroneGeneralKeyboard
{
	public delegate void DroneKeyboardEvent();

	public event DroneKeyboardEvent OnShowUI;

	public event DroneKeyboardEvent OnShiftPressed;

	public event DroneKeyboardEvent OnShiftReleased;

	public void Run()
	{
		Keyboard current = Keyboard.current;
		if (current != null)
		{
			if (current.tabKey.wasPressedThisFrame)
			{
				this.OnShowUI?.Invoke();
			}
			if (current.shiftKey.wasPressedThisFrame)
			{
				this.OnShiftPressed?.Invoke();
			}
			if (current.shiftKey.wasReleasedThisFrame)
			{
				this.OnShiftReleased?.Invoke();
			}
		}
	}
}
