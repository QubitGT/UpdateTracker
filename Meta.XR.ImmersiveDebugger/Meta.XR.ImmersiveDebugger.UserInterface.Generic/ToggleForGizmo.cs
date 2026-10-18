using Meta.XR.ImmersiveDebugger.Manager;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

internal class ToggleForGizmo : Toggle
{
	private GizmoHook _hook;

	public GizmoHook Hook
	{
		get
		{
			return _hook;
		}
		set
		{
			if (_hook != value)
			{
				_hook = value;
				base.StateChanged = null;
				base.State = value.GetState?.Invoke() ?? false;
				base.StateChanged = value.SetState;
			}
		}
	}

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		base.Callback = delegate
		{
			base.State = !base.State;
		};
	}
}
