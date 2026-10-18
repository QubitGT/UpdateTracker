using System.Reflection;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class ActionManagerForAddon : SubManagerForAddon
{
	public override string TelemetryAnnotation => "Actions";

	protected override bool RegisterSpecialisedWidget(IMember member, MemberInfo memberInfo, DebugMember memberAttribute, InstanceHandle handle)
	{
		if (memberInfo.MemberType != MemberTypes.Method)
		{
			return false;
		}
		MethodInfo methodInfo = memberInfo as MethodInfo;
		if (methodInfo == null || methodInfo.GetParameters().Length != 0 || methodInfo.ReturnType != typeof(void))
		{
			return false;
		}
		ActionHook action = member.GetAction();
		if (action == null || !action.Matches(memberInfo, handle))
		{
			member.RegisterAction(new ActionHook(memberInfo, handle, memberAttribute));
		}
		return true;
	}
}
