using System;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Hierarchy;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal interface IDebugManager
{
	string TelemetryAnnotation { get; }

	void Setup(IDebugUIPanel panel, InstanceCache cache);

	void ProcessType(Type type);

	void ProcessTypeFromInspector(Type type, InstanceHandle handle, MemberInfo memberInfo, DebugMember memberAttribute);

	void ProcessTypeFromHierarchy(Item item, MemberInfo memberInfo);

	int GetCountPerType(Type type);
}
