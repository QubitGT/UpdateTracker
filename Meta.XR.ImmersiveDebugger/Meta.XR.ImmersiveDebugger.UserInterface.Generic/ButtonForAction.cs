using Meta.XR.ImmersiveDebugger.Manager;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

internal class ButtonForAction : ButtonWithLabel
{
	private ActionHook _hook;

	internal ActionHook Action
	{
		get
		{
			return _hook;
		}
		set
		{
			_hook = value;
			base.Callback = value.Delegate;
		}
	}
}
