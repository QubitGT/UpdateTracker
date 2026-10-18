using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityEngine.Rendering;

internal static class DelegateHashCodeUtils
{
	private static readonly Lazy<Dictionary<int, bool>> s_MethodHashCodeToSkipTargetHashMap = new Lazy<Dictionary<int, bool>>(() => new Dictionary<int, bool>(64));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetFuncHashCode(Delegate del)
	{
		int hashCode = RuntimeHelpers.GetHashCode(del.Method);
		if (!s_MethodHashCodeToSkipTargetHashMap.Value.TryGetValue(hashCode, out var value))
		{
			int num;
			if (del.Target != null)
			{
				Type declaringType = del.Method.DeclaringType;
				num = (((object)declaringType != null && declaringType.IsNestedPrivate && Attribute.IsDefined(del.Method.DeclaringType, typeof(CompilerGeneratedAttribute), inherit: false)) ? 1 : 0);
			}
			else
			{
				num = 1;
			}
			value = (byte)num != 0;
			s_MethodHashCodeToSkipTargetHashMap.Value[hashCode] = value;
		}
		if (!value)
		{
			return hashCode ^ RuntimeHelpers.GetHashCode(del.Target);
		}
		return hashCode;
	}

	internal static int GetTotalCacheCount()
	{
		return s_MethodHashCodeToSkipTargetHashMap.Value.Count;
	}

	internal static void ClearCache()
	{
		s_MethodHashCodeToSkipTargetHashMap.Value.Clear();
	}
}
