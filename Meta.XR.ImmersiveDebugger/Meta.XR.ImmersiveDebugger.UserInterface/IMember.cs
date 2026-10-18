using Meta.XR.ImmersiveDebugger.Manager;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal interface IMember
{
	GizmoHook GetGizmo();

	void RegisterGizmo(GizmoHook gizmo);

	ActionHook GetAction();

	void RegisterAction(ActionHook action);

	Tweak GetTweak();

	void RegisterTweak(Tweak tweak);

	Watch GetWatch();

	void RegisterWatch(Watch watch);

	void RegisterEnum(TweakEnum tweak);

	void RegisterTexture(WatchTexture watch);
}
