using System;
using System.Collections.Generic;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Hierarchy;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class ActionManager : IDebugManager
{
	internal readonly Dictionary<Type, List<(MethodInfo, DebugMember)>> ActionsDict = new Dictionary<Type, List<(MethodInfo, DebugMember)>>();

	private IDebugUIPanel _uiPanel;

	private InstanceCache _instanceCache;

	public string TelemetryAnnotation => "Actions";

	public void Setup(IDebugUIPanel uiPanel, InstanceCache instanceCache)
	{
		_uiPanel = uiPanel;
		_instanceCache = instanceCache;
	}

	public void ProcessType(Type type)
	{
		ActionsDict.Remove(type);
		List<(MethodInfo, DebugMember)> list = new List<(MethodInfo, DebugMember)>();
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo methodInfo in methods)
		{
			DebugMember customAttribute = methodInfo.GetCustomAttribute<DebugMember>();
			if (customAttribute != null)
			{
				list.Add((methodInfo, customAttribute));
			}
		}
		list.AddRange(InspectedDataRegistry.GetMembersForType<MethodInfo>(type));
		ActionsDict[type] = list;
		ManagerUtils.RebuildInspectorForType(_uiPanel, _instanceCache, type, list, delegate(IMember memberController, MethodInfo member, DebugMember attribute, InstanceHandle instance)
		{
			ActionHook action = memberController.GetAction();
			if (action == null || !action.Matches(member, instance))
			{
				memberController.RegisterAction(new ActionHook(member, instance, attribute));
			}
		});
	}

	public void ProcessTypeFromInspector(Type type, InstanceHandle handle, MemberInfo memberInfo, DebugMember memberAttribute)
	{
		throw new NotImplementedException();
	}

	public void ProcessTypeFromHierarchy(Item item, MemberInfo memberInfo)
	{
		throw new NotImplementedException();
	}

	public int GetCountPerType(Type type)
	{
		ActionsDict.TryGetValue(type, out var value);
		return value?.Count ?? 0;
	}
}
