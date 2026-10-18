using System;
using System.Collections.Generic;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Hierarchy;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class WatchManager : IDebugManager
{
	internal readonly Dictionary<Type, List<(MemberInfo, DebugMember)>> WatchesDict = new Dictionary<Type, List<(MemberInfo, DebugMember)>>();

	private IDebugUIPanel _uiPanel;

	private InstanceCache _instanceCache;

	public string TelemetryAnnotation => "Watches";

	public void Setup(IDebugUIPanel panel, InstanceCache cache)
	{
		_uiPanel = panel;
		_instanceCache = cache;
	}

	public void ProcessType(Type type)
	{
		WatchesDict.Remove(type);
		List<(MemberInfo, DebugMember)> list = new List<(MemberInfo, DebugMember)>();
		MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty);
		foreach (MemberInfo memberInfo in members)
		{
			DebugMember customAttribute = memberInfo.GetCustomAttribute<DebugMember>();
			if (customAttribute != null && IsMemberValidForWatch(memberInfo))
			{
				list.Add((memberInfo, customAttribute));
			}
		}
		list.AddRange(InspectedDataRegistry.GetMembersForType(type, (MemberInfo info, DebugMember _) => IsMemberValidForWatch(info)));
		WatchesDict[type] = list;
		ManagerUtils.RebuildInspectorForType(_uiPanel, _instanceCache, type, list, delegate(IMember memberController, MemberInfo member, DebugMember attribute, InstanceHandle instance)
		{
			Watch watch = memberController.GetWatch();
			if (watch == null || !watch.Matches(member, instance))
			{
				if (member.IsTypeEqual(typeof(Texture2D)))
				{
					memberController.RegisterTexture(WatchUtils.Create(member, instance, attribute) as WatchTexture);
				}
				else
				{
					memberController.RegisterWatch(WatchUtils.Create(member, instance, attribute));
				}
			}
		});
	}

	internal static bool IsMemberValidForWatch(MemberInfo member)
	{
		MemberTypes memberType = member.MemberType;
		return (((memberType == MemberTypes.Property || memberType == MemberTypes.Field) & !member.IsBaseTypeEqual(typeof(Enum))) | member.IsTypeEqual(typeof(Texture2D))) & (!(member is PropertyInfo propertyInfo) || propertyInfo.CanRead);
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
		WatchesDict.TryGetValue(type, out var value);
		return value?.Count ?? 0;
	}
}
