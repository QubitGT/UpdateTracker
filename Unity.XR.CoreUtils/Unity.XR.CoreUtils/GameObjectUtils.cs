using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.XR.CoreUtils;

public static class GameObjectUtils
{
	private static readonly List<GameObject> k_GameObjects = new List<GameObject>();

	private static readonly List<Transform> k_Transforms = new List<Transform>();

	public static event Action<GameObject> GameObjectInstantiated;

	public static GameObject Create()
	{
		GameObject gameObject = new GameObject();
		GameObjectUtils.GameObjectInstantiated?.Invoke(gameObject);
		return gameObject;
	}

	public static GameObject Create(string name)
	{
		GameObject gameObject = new GameObject(name);
		GameObjectUtils.GameObjectInstantiated?.Invoke(gameObject);
		return gameObject;
	}

	public static GameObject Instantiate(GameObject original, Transform parent = null, bool worldPositionStays = true)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(original, parent, worldPositionStays);
		if (gameObject != null && GameObjectUtils.GameObjectInstantiated != null)
		{
			GameObjectUtils.GameObjectInstantiated(gameObject);
		}
		return gameObject;
	}

	public static GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation)
	{
		return Instantiate(original, null, position, rotation);
	}

	public static GameObject Instantiate(GameObject original, Transform parent, Vector3 position, Quaternion rotation)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(original, position, rotation, parent);
		if (gameObject != null && GameObjectUtils.GameObjectInstantiated != null)
		{
			GameObjectUtils.GameObjectInstantiated(gameObject);
		}
		return gameObject;
	}

	public static GameObject CloneWithHideFlags(GameObject original, Transform parent = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(original, parent);
		CopyHideFlagsRecursively(original, gameObject);
		return gameObject;
	}

	private static void CopyHideFlagsRecursively(GameObject copyFrom, GameObject copyTo)
	{
		copyTo.hideFlags = copyFrom.hideFlags;
		Transform transform = copyFrom.transform;
		Transform transform2 = copyTo.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			CopyHideFlagsRecursively(transform.GetChild(i).gameObject, transform2.GetChild(i).gameObject);
		}
	}

	public static T ExhaustiveComponentSearch<T>(GameObject desiredSource) where T : Component
	{
		T val = null;
		if (desiredSource != null)
		{
			val = desiredSource.GetComponentInChildren<T>(includeInactive: true);
		}
		if (val == null)
		{
			val = UnityEngine.Object.FindAnyObjectByType<T>();
		}
		_ = val != null;
		return val;
	}

	public static T ExhaustiveTaggedComponentSearch<T>(GameObject desiredSource, string tag) where T : Component
	{
		T val = null;
		if (desiredSource != null)
		{
			T[] componentsInChildren = desiredSource.GetComponentsInChildren<T>(includeInactive: true);
			foreach (T val2 in componentsInChildren)
			{
				if (val2.gameObject.CompareTag(tag))
				{
					val = val2;
					break;
				}
			}
		}
		if (val == null)
		{
			GameObject[] array = GameObject.FindGameObjectsWithTag(tag);
			for (int i = 0; i < array.Length; i++)
			{
				val = array[i].GetComponent<T>();
				if (val != null)
				{
					break;
				}
			}
		}
		if (val == null)
		{
			val = UnityEngine.Object.FindAnyObjectByType<T>();
		}
		return val;
	}

	public static T GetComponentInScene<T>(Scene scene) where T : Component
	{
		scene.GetRootGameObjects(k_GameObjects);
		foreach (GameObject k_GameObject in k_GameObjects)
		{
			T componentInChildren = k_GameObject.GetComponentInChildren<T>();
			if ((bool)componentInChildren)
			{
				return componentInChildren;
			}
		}
		return null;
	}

	public static void GetComponentsInScene<T>(Scene scene, List<T> components, bool includeInactive = false) where T : Component
	{
		scene.GetRootGameObjects(k_GameObjects);
		foreach (GameObject k_GameObject in k_GameObjects)
		{
			if (includeInactive || k_GameObject.activeInHierarchy)
			{
				components.AddRange(k_GameObject.GetComponentsInChildren<T>(includeInactive));
			}
		}
	}

	public static T GetComponentInActiveScene<T>() where T : Component
	{
		return GetComponentInScene<T>(SceneManager.GetActiveScene());
	}

	public static void GetComponentsInActiveScene<T>(List<T> components, bool includeInactive = false) where T : Component
	{
		GetComponentsInScene(SceneManager.GetActiveScene(), components, includeInactive);
	}

	public static void GetComponentsInAllScenes<T>(List<T> components, bool includeInactive = false) where T : Component
	{
		int sceneCount = SceneManager.sceneCount;
		for (int i = 0; i < sceneCount; i++)
		{
			Scene sceneAt = SceneManager.GetSceneAt(i);
			if (sceneAt.isLoaded)
			{
				GetComponentsInScene(sceneAt, components, includeInactive);
			}
		}
	}

	public static void GetChildGameObjects(this GameObject go, List<GameObject> childGameObjects)
	{
		Transform transform = go.transform;
		int childCount = transform.childCount;
		if (childCount != 0)
		{
			childGameObjects.EnsureCapacity(childCount);
			for (int i = 0; i < childCount; i++)
			{
				childGameObjects.Add(transform.GetChild(i).gameObject);
			}
		}
	}

	public static GameObject GetNamedChild(this GameObject go, string name)
	{
		k_Transforms.Clear();
		go.GetComponentsInChildren(k_Transforms);
		Transform transform = k_Transforms.Find((Transform currentTransform) => currentTransform.name == name);
		k_Transforms.Clear();
		if (transform != null)
		{
			return transform.gameObject;
		}
		return null;
	}
}
