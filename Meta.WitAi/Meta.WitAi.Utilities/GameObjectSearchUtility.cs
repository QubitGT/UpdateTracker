using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meta.WitAi.Utilities;

public static class GameObjectSearchUtility
{
	public static T FindSceneObject<T>(bool includeInactive = true) where T : Object
	{
		T[] array = FindSceneObjects<T>(includeInactive, returnImmediately: true);
		if (array != null && array.Length != 0)
		{
			return array[0];
		}
		return null;
	}

	public static T[] FindSceneObjects<T>(bool includeInactive = true, bool returnImmediately = false) where T : Object
	{
		if (!includeInactive)
		{
			return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
		}
		List<T> list = new List<T>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
			for (int j = 0; j < rootGameObjects.Length; j++)
			{
				T[] componentsInChildren = rootGameObjects[j].GetComponentsInChildren<T>(includeInactive);
				if (componentsInChildren != null && componentsInChildren.Length != 0)
				{
					list.AddRange(componentsInChildren);
					if (returnImmediately)
					{
						return list.ToArray();
					}
				}
			}
		}
		return list.ToArray();
	}
}
