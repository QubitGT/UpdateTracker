using System.Collections.Generic;

namespace UnityEngine.Rendering.Universal;

internal class ShadowCasterGroup2DManager
{
	private static List<ShadowCasterGroup2D> s_ShadowCasterGroups;

	public static List<ShadowCasterGroup2D> shadowCasterGroups => s_ShadowCasterGroups;

	public static void CacheValues()
	{
		if (shadowCasterGroups == null)
		{
			return;
		}
		for (int i = 0; i < shadowCasterGroups.Count; i++)
		{
			if (shadowCasterGroups[i] != null)
			{
				shadowCasterGroups[i].CacheValues();
			}
		}
	}

	public static void AddShadowCasterGroupToList(ShadowCasterGroup2D shadowCaster, List<ShadowCasterGroup2D> list)
	{
		if (!list.Contains(shadowCaster))
		{
			int num = 0;
			for (num = 0; num < list.Count && shadowCaster.m_Priority >= list[num].m_Priority; num++)
			{
			}
			list.Insert(num, shadowCaster);
		}
	}

	public static void RemoveShadowCasterGroupFromList(ShadowCasterGroup2D shadowCaster, List<ShadowCasterGroup2D> list)
	{
		list.Remove(shadowCaster);
	}

	private static CompositeShadowCaster2D FindTopMostCompositeShadowCaster(ShadowCaster2D shadowCaster)
	{
		CompositeShadowCaster2D result = null;
		Transform parent = shadowCaster.transform.parent;
		while (parent != null)
		{
			if (parent.TryGetComponent<CompositeShadowCaster2D>(out var component))
			{
				result = component;
			}
			parent = parent.parent;
		}
		return result;
	}

	public static int GetRendereringPriority(ShadowCaster2D shadowCaster)
	{
		int result = 0;
		if (shadowCaster.TryGetComponent<Renderer>(out var component))
		{
			result = component.sortingOrder;
		}
		return result;
	}

	public static bool AddToShadowCasterGroup(ShadowCaster2D shadowCaster, ref ShadowCasterGroup2D shadowCasterGroup, ref int priority)
	{
		ShadowCasterGroup2D component = FindTopMostCompositeShadowCaster(shadowCaster);
		int num = 0;
		if (component == null)
		{
			num = GetRendereringPriority(shadowCaster);
			shadowCaster.TryGetComponent<ShadowCasterGroup2D>(out component);
		}
		if (component != null && (shadowCasterGroup != component || priority != num))
		{
			component.RegisterShadowCaster2D(shadowCaster);
			shadowCasterGroup = component;
			priority = num;
			return true;
		}
		return false;
	}

	public static void RemoveFromShadowCasterGroup(ShadowCaster2D shadowCaster, ShadowCasterGroup2D shadowCasterGroup)
	{
		if (shadowCasterGroup != null)
		{
			shadowCasterGroup.UnregisterShadowCaster2D(shadowCaster);
		}
		if (shadowCasterGroup == shadowCaster)
		{
			RemoveGroup(shadowCasterGroup);
		}
	}

	public static void AddGroup(ShadowCasterGroup2D group)
	{
		if (!(group == null))
		{
			if (s_ShadowCasterGroups == null)
			{
				s_ShadowCasterGroups = new List<ShadowCasterGroup2D>();
			}
			AddShadowCasterGroupToList(group, s_ShadowCasterGroups);
		}
	}

	public static void RemoveGroup(ShadowCasterGroup2D group)
	{
		if (group != null && s_ShadowCasterGroups != null)
		{
			RemoveShadowCasterGroupFromList(group, s_ShadowCasterGroups);
		}
	}
}
