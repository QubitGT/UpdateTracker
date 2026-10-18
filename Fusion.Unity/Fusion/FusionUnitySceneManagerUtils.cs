using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion;

public static class FusionUnitySceneManagerUtils
{
	public class SceneEqualityComparer : IEqualityComparer<Scene>
	{
		public bool Equals(Scene x, Scene y)
		{
			return x.handle == y.handle;
		}

		public int GetHashCode(Scene obj)
		{
			return obj.handle;
		}
	}

	private static readonly List<GameObject> _reusableGameObjectList = new List<GameObject>();

	public static bool IsAddedToBuildSettings(this Scene scene)
	{
		if (scene.buildIndex < 0)
		{
			return false;
		}
		if (scene.buildIndex >= SceneManager.sceneCountInBuildSettings)
		{
			return false;
		}
		return true;
	}

	public static LocalPhysicsMode GetLocalPhysicsMode(this Scene scene)
	{
		LocalPhysicsMode localPhysicsMode = LocalPhysicsMode.None;
		if (scene.GetPhysicsScene() != Physics.defaultPhysicsScene)
		{
			localPhysicsMode |= LocalPhysicsMode.Physics3D;
		}
		if (scene.GetPhysicsScene2D() != Physics2D.defaultPhysicsScene)
		{
			localPhysicsMode |= LocalPhysicsMode.Physics2D;
		}
		return localPhysicsMode;
	}

	public static T[] GetComponents<T>(this Scene scene, bool includeInactive) where T : Component
	{
		GameObject[] rootObjects;
		return scene.GetComponents<T>(includeInactive, out rootObjects);
	}

	public static T[] GetComponents<T>(this Scene scene, bool includeInactive, out GameObject[] rootObjects) where T : Component
	{
		rootObjects = scene.GetRootGameObjects();
		List<T> list = new List<T>();
		List<T> list2 = new List<T>();
		GameObject[] array = rootObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponentsInChildren(includeInactive, list);
			foreach (T item in list)
			{
				list2.Add(item);
			}
		}
		return list2.ToArray();
	}

	public static void GetComponents<T>(this Scene scene, List<T> results, bool includeInactive) where T : Component
	{
		List<GameObject> reusableGameObjectList = _reusableGameObjectList;
		scene.GetRootGameObjects(reusableGameObjectList);
		results.Clear();
		List<T> list = new List<T>();
		foreach (GameObject item in reusableGameObjectList)
		{
			item.GetComponentsInChildren(includeInactive, list);
			foreach (T item2 in list)
			{
				results.Add(item2);
			}
		}
	}

	public static T FindComponent<T>(this Scene scene, bool includeInactive = false) where T : Component
	{
		List<GameObject> reusableGameObjectList = _reusableGameObjectList;
		scene.GetRootGameObjects(reusableGameObjectList);
		foreach (GameObject item in reusableGameObjectList)
		{
			T componentInChildren = item.GetComponentInChildren<T>(includeInactive);
			if (componentInChildren != null)
			{
				return componentInChildren;
			}
		}
		return null;
	}

	public static bool CanBeUnloaded(this Scene scene)
	{
		if (!scene.isLoaded)
		{
			return false;
		}
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt != scene && sceneAt.isLoaded)
			{
				return true;
			}
		}
		return false;
	}

	public static string Dump(this Scene scene)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[UnityScene:");
		if (scene.IsValid())
		{
			stringBuilder.Append(scene.name);
			stringBuilder.Append(", isLoaded:").Append(scene.isLoaded);
			stringBuilder.Append(", buildIndex:").Append(scene.buildIndex);
			stringBuilder.Append(", isDirty:").Append(scene.isDirty);
			stringBuilder.Append(", path:").Append(scene.path);
			stringBuilder.Append(", rootCount:").Append(scene.rootCount);
			stringBuilder.Append(", isSubScene:").Append(scene.isSubScene);
		}
		else
		{
			stringBuilder.Append("<Invalid>");
		}
		stringBuilder.Append(", handle:").Append(scene.handle);
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public static string Dump(this LoadSceneParameters loadSceneParameters)
	{
		return $"[LoadSceneParameters: {loadSceneParameters.loadSceneMode}, localPhysicsMode:{loadSceneParameters.localPhysicsMode}]";
	}

	public static int GetSceneBuildIndex(string nameOrPath)
	{
		if (nameOrPath.IndexOf('/') >= 0)
		{
			return SceneUtility.GetBuildIndexByScenePath(nameOrPath);
		}
		for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
		{
			string scenePathByBuildIndex = SceneUtility.GetScenePathByBuildIndex(i);
			GetFileNameWithoutExtensionPosition(scenePathByBuildIndex, out var index, out var length);
			if (length == nameOrPath.Length && string.Compare(scenePathByBuildIndex, index, nameOrPath, 0, length, ignoreCase: true) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	public static int GetSceneIndex(IList<string> scenePathsOrNames, string nameOrPath)
	{
		if (nameOrPath.IndexOf('/') >= 0)
		{
			return scenePathsOrNames.IndexOf(nameOrPath);
		}
		for (int i = 0; i < scenePathsOrNames.Count; i++)
		{
			string text = scenePathsOrNames[i];
			GetFileNameWithoutExtensionPosition(text, out var index, out var length);
			if (length == nameOrPath.Length && string.Compare(text, index, nameOrPath, 0, length, ignoreCase: true) == 0)
			{
				return i;
			}
		}
		return -1;
	}

	public static void GetFileNameWithoutExtensionPosition(string nameOrPath, out int index, out int length)
	{
		int num = nameOrPath.LastIndexOf('/');
		if (num >= 0)
		{
			index = num + 1;
		}
		else
		{
			index = 0;
		}
		int num2 = nameOrPath.LastIndexOf('.');
		if (num2 > index)
		{
			length = num2 - index;
		}
		else
		{
			length = nameOrPath.Length - index;
		}
	}
}
