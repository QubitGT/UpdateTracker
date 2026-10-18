using System;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class ActionHook : Hook
{
	internal Action Delegate { get; private set; }

	internal ActionHook(MemberInfo memberInfo, InstanceHandle instanceHandle, DebugMember attribute)
		: base(memberInfo, instanceHandle, attribute)
	{
		ActionHook actionHook = this;
		Delegate = delegate
		{
			(memberInfo as MethodInfo)?.Invoke(actionHook._instance, null);
		};
	}
}
