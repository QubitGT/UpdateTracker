using System;
using System.Collections.Generic;
using System.Reflection;

namespace Meta.XR.ImmersiveDebugger;

internal static class InspectedDataRegistry
{
	private static readonly Dictionary<Type, List<InspectedMember>> InspectedMembersRegistry = new Dictionary<Type, List<InspectedMember>>();

	internal static void Add(Type type, InspectedMember inspectedMember)
	{
		if (!InspectedMembersRegistry.TryGetValue(type, out var value))
		{
			value = new List<InspectedMember>();
			InspectedMembersRegistry[type] = value;
		}
		value.Add(inspectedMember);
	}

	internal static void Reset()
	{
		InspectedMembersRegistry?.Clear();
	}

	internal static List<(T, DebugMember)> GetMembersForType<T>(Type type, Func<T, DebugMember, bool> filterCallback = null) where T : MemberInfo
	{
		List<(T, DebugMember)> list = new List<(T, DebugMember)>();
		if (!InspectedMembersRegistry.TryGetValue(type, out var value))
		{
			return list;
		}
		foreach (InspectedMember item in value)
		{
			T val = item.MemberInfo as T;
			if (val != null && (filterCallback == null || filterCallback(val, item.attribute)))
			{
				list.Add((val, item.attribute));
			}
		}
		return list;
	}
}
