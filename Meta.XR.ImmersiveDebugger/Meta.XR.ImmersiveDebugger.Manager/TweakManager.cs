using System;
using System.Collections.Generic;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Hierarchy;
using Meta.XR.ImmersiveDebugger.UserInterface;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class TweakManager : IDebugManager
{
	internal readonly Dictionary<Type, List<(MemberInfo, DebugMember)>> TweaksDict = new Dictionary<Type, List<(MemberInfo, DebugMember)>>();

	private IDebugUIPanel _uiPanel;

	private InstanceCache _instanceCache;

	public string TelemetryAnnotation => "Tweaks";

	public void Setup(IDebugUIPanel panel, InstanceCache cache)
	{
		_uiPanel = panel;
		_instanceCache = cache;
	}

	public void ProcessType(Type type)
	{
		TweaksDict.Remove(type);
		List<(MemberInfo, DebugMember)> list = new List<(MemberInfo, DebugMember)>();
		MemberInfo[] members = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.SetProperty);
		foreach (MemberInfo memberInfo in members)
		{
			if (TweakUtils.IsMemberValidForTweak(memberInfo))
			{
				DebugMember customAttribute = memberInfo.GetCustomAttribute<DebugMember>();
				if (customAttribute != null && customAttribute.Tweakable)
				{
					list.Add((memberInfo, customAttribute));
				}
			}
		}
		list.AddRange(InspectedDataRegistry.GetMembersForType(type, (MemberInfo info, DebugMember attribute) => TweakUtils.IsMemberValidForTweak(info) && attribute.Tweakable));
		TweaksDict[type] = list;
		ManagerUtils.RebuildInspectorForType(_uiPanel, _instanceCache, type, list, delegate(IMember memberController, MemberInfo member, DebugMember attribute, InstanceHandle instance)
		{
			Tweak tweak = memberController.GetTweak();
			if (tweak == null || !tweak.Matches(member, instance))
			{
				if (member.IsBaseTypeEqual(typeof(Enum)))
				{
					memberController.RegisterEnum(TweakUtils.Create(member, attribute, instance, member.GetDataType()));
				}
				else
				{
					TweakUtils.ProcessMinMaxRange(member, attribute, instance);
					memberController.RegisterTweak(TweakUtils.Create(member, attribute, instance));
				}
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
		TweaksDict.TryGetValue(type, out var value);
		return value?.Count ?? 0;
	}
}
