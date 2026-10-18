using UnityEngine;

namespace Meta.WitAi;

public static class UnityObjectExtensions
{
	public static void DestroySafely(this Object unityObject)
	{
		if ((bool)unityObject)
		{
			Object.Destroy(unityObject);
		}
	}

	public static T GetOrAddComponent<T>(this GameObject unityObject) where T : Component
	{
		if (!unityObject)
		{
			return null;
		}
		T component = unityObject.GetComponent<T>();
		if ((bool)component)
		{
			return component;
		}
		return unityObject.AddComponent<T>();
	}
}
