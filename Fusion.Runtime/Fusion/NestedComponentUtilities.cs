using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion;

public static class NestedComponentUtilities
{
	private static class RecyclableList<T> where T : class
	{
		public static List<T> List = new List<T>();
	}

	private static Queue<Transform> nodesQueue = new Queue<Transform>();

	private static Stack<Transform> nodeStack = new Stack<Transform>();

	public static T EnsureRootComponentExists<T, TStopOn>(this Transform transform) where T : Component where TStopOn : Component
	{
		TStopOn parentComponent = transform.GetParentComponent<TStopOn>();
		if ((bool)parentComponent)
		{
			if (parentComponent.TryGetComponent<T>(out var component))
			{
				return component;
			}
			return parentComponent.gameObject.AddComponent<T>();
		}
		return null;
	}

	public static T GetParentComponent<T>(this Transform t) where T : Component
	{
		if (t.TryGetComponent<T>(out var component))
		{
			return component;
		}
		Transform parent = t.parent;
		while ((bool)parent)
		{
			if (parent.TryGetComponent<T>(out var component2))
			{
				return component2;
			}
			parent = parent.parent;
		}
		return null;
	}

	public static void GetNestedComponentsInParents<T>(this Transform t, List<T> list) where T : Component
	{
		list.Clear();
		while (t != null)
		{
			if (t.TryGetComponent<T>(out var component))
			{
				list.Add(component);
			}
			t = t.parent;
		}
	}

	public static T GetNestedComponentInChildren<T, TStopOn>(this Transform t, bool includeInactive) where T : class where TStopOn : class
	{
		if (t.TryGetComponent<T>(out var component))
		{
			return component;
		}
		nodesQueue.Clear();
		nodesQueue.Enqueue(t);
		while (nodesQueue.Count > 0)
		{
			Transform transform = nodesQueue.Dequeue();
			int i = 0;
			for (int childCount = transform.childCount; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				if ((includeInactive || child.gameObject.activeSelf) && !child.TryGetComponent<TStopOn>(out var _))
				{
					if (child.TryGetComponent<T>(out var component3))
					{
						return component3;
					}
					nodesQueue.Enqueue(child);
				}
			}
		}
		return null;
	}

	public static T GetNestedComponentInParent<T, TStopOn>(this Transform t) where T : class where TStopOn : class
	{
		Transform transform = t;
		do
		{
			if (transform.TryGetComponent<T>(out var component))
			{
				return component;
			}
			if (transform.TryGetComponent<TStopOn>(out var _))
			{
				return null;
			}
			transform = transform.parent;
		}
		while ((object)transform != null);
		return null;
	}

	public static T GetNestedComponentInParents<T, TStopOn>(this Transform t) where T : class where TStopOn : class
	{
		if (t.TryGetComponent<T>(out var component))
		{
			return component;
		}
		Transform parent = t.parent;
		while ((bool)parent)
		{
			if (parent.TryGetComponent<T>(out var component2))
			{
				return component2;
			}
			if (parent.TryGetComponent<TStopOn>(out var _))
			{
				return null;
			}
			parent = parent.parent;
		}
		return null;
	}

	public static void GetNestedComponentsInParents<T, TStop>(this Transform t, List<T> list) where T : class where TStop : class
	{
		t.GetComponents(list);
		if (t.TryGetComponent<TStop>(out var component))
		{
			return;
		}
		Transform parent = t.parent;
		if ((object)parent == null)
		{
			return;
		}
		nodeStack.Clear();
		do
		{
			nodeStack.Push(parent);
			if (parent.TryGetComponent<TStop>(out component))
			{
				break;
			}
			parent = parent.parent;
		}
		while (!(parent == null));
		if (nodeStack.Count == 0)
		{
			return;
		}
		List<T> list2 = RecyclableList<T>.List;
		try
		{
			while (nodeStack.Count > 0)
			{
				Transform transform = nodeStack.Pop();
				transform.GetComponents(list2);
				list.AddRange(list2);
			}
		}
		finally
		{
			list2.Clear();
		}
	}

	public static List<T> GetNestedComponentsInChildren<T, TStopOn>(this Transform t, List<T> list, bool includeInactive = true) where T : class where TStopOn : class
	{
		nodesQueue.Clear();
		if (list == null)
		{
			list = new List<T>();
		}
		t.GetComponents(list);
		int i = 0;
		TStopOn component;
		for (int childCount = t.childCount; i < childCount; i++)
		{
			Transform child = t.GetChild(i);
			if ((includeInactive || child.gameObject.activeSelf) && !child.TryGetComponent<TStopOn>(out component))
			{
				nodesQueue.Enqueue(child);
			}
		}
		List<T> list2 = RecyclableList<T>.List;
		try
		{
			while (nodesQueue.Count > 0)
			{
				Transform transform = nodesQueue.Dequeue();
				transform.GetComponents(list2);
				list.AddRange(list2);
				int j = 0;
				for (int childCount2 = transform.childCount; j < childCount2; j++)
				{
					Transform child2 = transform.GetChild(j);
					if ((includeInactive || child2.gameObject.activeSelf) && !child2.TryGetComponent<TStopOn>(out component))
					{
						nodesQueue.Enqueue(child2);
					}
				}
			}
		}
		finally
		{
			list2.Clear();
		}
		return list;
	}

	public static List<T> GetNestedComponentsInChildren<T>(this Transform t, List<T> list, bool includeInactive = true, params Type[] stopOn) where T : class
	{
		nodesQueue.Clear();
		t.GetComponents(list);
		int i = 0;
		Component component;
		for (int childCount = t.childCount; i < childCount; i++)
		{
			Transform child = t.GetChild(i);
			if (!includeInactive && !child.gameObject.activeSelf)
			{
				continue;
			}
			int j = 0;
			for (int num = stopOn.Length; j < num; j++)
			{
				if (child.TryGetComponent(stopOn[j], out component))
				{
				}
			}
			nodesQueue.Enqueue(child);
		}
		List<T> list2 = RecyclableList<T>.List;
		try
		{
			while (nodesQueue.Count > 0)
			{
				Transform transform = nodesQueue.Dequeue();
				transform.GetComponents(list2);
				list.AddRange(list2);
				int k = 0;
				for (int childCount2 = transform.childCount; k < childCount2; k++)
				{
					Transform child2 = transform.GetChild(k);
					if (!includeInactive && !child2.gameObject.activeSelf)
					{
						continue;
					}
					int l = 0;
					for (int num2 = stopOn.Length; l < num2; l++)
					{
						if (child2.TryGetComponent(stopOn[l], out component))
						{
						}
					}
					nodesQueue.Enqueue(child2);
				}
			}
		}
		finally
		{
			list2.Clear();
		}
		return list;
	}

	public static void GetNestedComponentsInChildren<T, TSearch, TStop>(this Transform t, bool includeInactive, List<T> list) where T : class where TSearch : class
	{
		list.Clear();
		if (!includeInactive && !t.gameObject.activeSelf)
		{
			return;
		}
		List<TSearch> list2 = RecyclableList<TSearch>.List;
		nodesQueue.Clear();
		nodesQueue.Enqueue(t);
		try
		{
			while (nodesQueue.Count > 0)
			{
				Transform transform = nodesQueue.Dequeue();
				transform.GetComponents(list2);
				foreach (TSearch item2 in list2)
				{
					if (item2 is T item)
					{
						list.Add(item);
					}
				}
				int i = 0;
				for (int childCount = transform.childCount; i < childCount; i++)
				{
					Transform child = transform.GetChild(i);
					if ((includeInactive || child.gameObject.activeSelf) && child.GetComponent<TStop>() == null)
					{
						nodesQueue.Enqueue(child);
					}
				}
			}
		}
		finally
		{
			list2.Clear();
		}
	}

	public static T[] FindObjectsOfTypeInOrder<T>(this Scene scene, bool includeInactive = false) where T : class
	{
		List<T> list = new List<T>();
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		GameObject[] array = rootGameObjects;
		foreach (GameObject gameObject in array)
		{
			list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive));
		}
		return list.ToArray();
	}

	public static void FindObjectsOfTypeInOrder<T>(this Scene scene, List<T> list, bool includeInactive = false) where T : class
	{
		list.Clear();
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		GameObject[] array = rootGameObjects;
		foreach (GameObject gameObject in array)
		{
			list.AddRange(gameObject.GetComponentsInChildren<T>(includeInactive));
		}
	}

	public static TCast[] FindObjectsOfTypeInOrder<T, TCast>(this Scene scene, bool includeInactive = false) where T : class where TCast : class
	{
		List<TCast> list = new List<TCast>();
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		GameObject[] array = rootGameObjects;
		foreach (GameObject gameObject in array)
		{
			T[] componentsInChildren = gameObject.GetComponentsInChildren<T>(includeInactive);
			T[] array2 = componentsInChildren;
			foreach (T val in array2)
			{
				if (val is TCast item)
				{
					list.Add(item);
				}
			}
		}
		return list.ToArray();
	}

	public static void FindObjectsOfTypeInOrder<T, TCast>(this Scene scene, List<TCast> list, bool includeInactive = false) where T : class where TCast : class
	{
		list.Clear();
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		GameObject[] array = rootGameObjects;
		foreach (GameObject gameObject in array)
		{
			T[] componentsInChildren = gameObject.GetComponentsInChildren<T>(includeInactive);
			T[] array2 = componentsInChildren;
			foreach (T val in array2)
			{
				if (val is TCast item)
				{
					list.Add(item);
				}
			}
		}
	}
}
