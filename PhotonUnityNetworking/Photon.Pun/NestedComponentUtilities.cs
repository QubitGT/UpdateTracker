using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Photon.Pun;

public static class NestedComponentUtilities
{
	private static Queue<Transform> nodesQueue = new Queue<Transform>();

	public static Dictionary<Type, ICollection> searchLists = new Dictionary<Type, ICollection>();

	private static Stack<Transform> nodeStack = new Stack<Transform>();

	public static T EnsureRootComponentExists<T, NestedT>(this Transform transform) where T : Component where NestedT : Component
	{
		NestedT parentComponent = transform.GetParentComponent<NestedT>();
		if ((bool)parentComponent)
		{
			T component = parentComponent.GetComponent<T>();
			if ((bool)component)
			{
				return component;
			}
			return parentComponent.gameObject.AddComponent<T>();
		}
		return null;
	}

	public static T GetParentComponent<T>(this Transform t) where T : Component
	{
		T component = t.GetComponent<T>();
		if ((bool)component)
		{
			return component;
		}
		Transform parent = t.parent;
		while ((bool)parent)
		{
			component = parent.GetComponent<T>();
			if ((bool)component)
			{
				return component;
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
			T component = t.GetComponent<T>();
			if ((bool)component)
			{
				list.Add(component);
			}
			t = t.parent;
		}
	}

	public static T GetNestedComponentInChildren<T, NestedT>(this Transform t, bool includeInactive) where T : class where NestedT : class
	{
		T component = t.GetComponent<T>();
		if (component != null)
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
				if ((includeInactive || child.gameObject.activeSelf) && child.GetComponent<NestedT>() == null)
				{
					component = child.GetComponent<T>();
					if (component != null)
					{
						return component;
					}
					nodesQueue.Enqueue(child);
				}
			}
		}
		return component;
	}

	public static T GetNestedComponentInParent<T, NestedT>(this Transform t) where T : class where NestedT : class
	{
		T val = null;
		Transform transform = t;
		do
		{
			val = transform.GetComponent<T>();
			if (val != null)
			{
				return val;
			}
			if (transform.GetComponent<NestedT>() != null)
			{
				return null;
			}
			transform = transform.parent;
		}
		while ((object)transform != null);
		return null;
	}

	public static T GetNestedComponentInParents<T, NestedT>(this Transform t) where T : class where NestedT : class
	{
		T component = t.GetComponent<T>();
		if (component != null)
		{
			return component;
		}
		Transform parent = t.parent;
		while ((object)parent != null)
		{
			component = parent.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			if (parent.GetComponent<NestedT>() != null)
			{
				return null;
			}
			parent = parent.parent;
		}
		return null;
	}

	public static void GetNestedComponentsInParents<T, NestedT>(this Transform t, List<T> list) where T : class where NestedT : class
	{
		t.GetComponents(list);
		if (t.GetComponent<NestedT>() != null)
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
			if (parent.GetComponent<NestedT>() != null)
			{
				break;
			}
			parent = parent.parent;
		}
		while ((object)parent != null);
		if (nodeStack.Count != 0)
		{
			Type typeFromHandle = typeof(T);
			List<T> list2;
			if (!searchLists.ContainsKey(typeFromHandle))
			{
				list2 = new List<T>();
				searchLists.Add(typeFromHandle, list2);
			}
			else
			{
				list2 = searchLists[typeFromHandle] as List<T>;
			}
			while (nodeStack.Count > 0)
			{
				nodeStack.Pop().GetComponents(list2);
				list.AddRange(list2);
			}
		}
	}

	public static List<T> GetNestedComponentsInChildren<T, NestedT>(this Transform t, List<T> list, bool includeInactive = true) where T : class where NestedT : class
	{
		Type typeFromHandle = typeof(T);
		List<T> list2;
		if (!searchLists.ContainsKey(typeFromHandle))
		{
			searchLists.Add(typeFromHandle, list2 = new List<T>());
		}
		else
		{
			list2 = searchLists[typeFromHandle] as List<T>;
		}
		nodesQueue.Clear();
		if (list == null)
		{
			list = new List<T>();
		}
		t.GetComponents(list);
		int i = 0;
		for (int childCount = t.childCount; i < childCount; i++)
		{
			Transform child = t.GetChild(i);
			if ((includeInactive || child.gameObject.activeSelf) && child.GetComponent<NestedT>() == null)
			{
				nodesQueue.Enqueue(child);
			}
		}
		while (nodesQueue.Count > 0)
		{
			Transform transform = nodesQueue.Dequeue();
			transform.GetComponents(list2);
			list.AddRange(list2);
			int j = 0;
			for (int childCount2 = transform.childCount; j < childCount2; j++)
			{
				Transform child2 = transform.GetChild(j);
				if ((includeInactive || child2.gameObject.activeSelf) && child2.GetComponent<NestedT>() == null)
				{
					nodesQueue.Enqueue(child2);
				}
			}
		}
		return list;
	}

	public static List<T> GetNestedComponentsInChildren<T>(this Transform t, List<T> list, bool includeInactive = true, params Type[] stopOn) where T : class
	{
		Type typeFromHandle = typeof(T);
		List<T> list2;
		if (!searchLists.ContainsKey(typeFromHandle))
		{
			searchLists.Add(typeFromHandle, list2 = new List<T>());
		}
		else
		{
			list2 = searchLists[typeFromHandle] as List<T>;
		}
		nodesQueue.Clear();
		t.GetComponents(list);
		int i = 0;
		for (int childCount = t.childCount; i < childCount; i++)
		{
			Transform child = t.GetChild(i);
			if (!includeInactive && !child.gameObject.activeSelf)
			{
				continue;
			}
			bool flag = false;
			int j = 0;
			for (int num = stopOn.Length; j < num; j++)
			{
				if ((object)child.GetComponent(stopOn[j]) != null)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				nodesQueue.Enqueue(child);
			}
		}
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
				bool flag2 = false;
				int l = 0;
				for (int num2 = stopOn.Length; l < num2; l++)
				{
					if ((object)child2.GetComponent(stopOn[l]) != null)
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					nodesQueue.Enqueue(child2);
				}
			}
		}
		return list;
	}

	public static void GetNestedComponentsInChildren<T, SearchT, NestedT>(this Transform t, bool includeInactive, List<T> list) where T : class where SearchT : class
	{
		list.Clear();
		if (!includeInactive && !t.gameObject.activeSelf)
		{
			return;
		}
		Type typeFromHandle = typeof(SearchT);
		List<SearchT> list2;
		if (!searchLists.ContainsKey(typeFromHandle))
		{
			searchLists.Add(typeFromHandle, list2 = new List<SearchT>());
		}
		else
		{
			list2 = searchLists[typeFromHandle] as List<SearchT>;
		}
		nodesQueue.Clear();
		nodesQueue.Enqueue(t);
		while (nodesQueue.Count > 0)
		{
			Transform transform = nodesQueue.Dequeue();
			list2.Clear();
			transform.GetComponents(list2);
			foreach (SearchT item2 in list2)
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
				if ((includeInactive || child.gameObject.activeSelf) && child.GetComponent<NestedT>() == null)
				{
					nodesQueue.Enqueue(child);
				}
			}
		}
	}
}
