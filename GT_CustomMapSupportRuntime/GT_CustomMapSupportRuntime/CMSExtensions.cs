using System;
using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public static class CMSExtensions
{
	public static string GetHierarchyPath(this Transform transform)
	{
		string text = transform.name;
		while ((bool)transform.parent)
		{
			transform = transform.parent;
			text = transform.name + "/" + text;
		}
		return "/" + text;
	}

	public static string GetHierarchyPath(this Transform transform, int maxDepth)
	{
		string text = transform.name;
		int num = 0;
		while ((bool)transform.parent && num < maxDepth)
		{
			transform = transform.parent;
			text = transform.name + "/" + text;
			num++;
		}
		return "/" + text;
	}

	public static string GetHierarchyPath(this Transform transform, Transform stopper)
	{
		string text = transform.name;
		while ((bool)transform.parent && transform.parent != stopper)
		{
			transform = transform.parent;
			text = transform.name + "/" + text;
		}
		return "/" + text;
	}

	public static string GetHierarchyPath(this GameObject gameObject)
	{
		return gameObject.transform.GetHierarchyPath();
	}

	public static string GetHierarchyPath(this GameObject gameObject, int limit)
	{
		return gameObject.transform.GetHierarchyPath(limit);
	}

	public static string[] GetHierarchyPaths(this GameObject[] gobj)
	{
		string[] array = new string[gobj.Length];
		for (int i = 0; i < gobj.Length; i++)
		{
			array[i] = gobj[i].GetHierarchyPath();
		}
		return array;
	}

	public static string[] GetHierarchyPaths(this Transform[] xform)
	{
		string[] array = new string[xform.Length];
		for (int i = 0; i < xform.Length; i++)
		{
			array[i] = xform[i].GetHierarchyPath();
		}
		return array;
	}

	public static T? GetComponentByHierarchyPath<T>(this GameObject root, string path) where T : Component
	{
		string[] array = path.Split(new string[1] { "/->/" }, StringSplitOptions.None);
		if (array.Length < 2)
		{
			return null;
		}
		string[] array2 = array[0].Split(new string[1] { "/" }, StringSplitOptions.RemoveEmptyEntries);
		Transform transform = root.transform;
		for (int i = 1; i < array2.Length; i++)
		{
			string n = array2[i];
			transform = transform.Find(n);
			if (transform == null)
			{
				return null;
			}
		}
		Type type = Type.GetType(array[1].Split('#')[0]);
		if (type == null)
		{
			return null;
		}
		Component component = transform.GetComponent(type);
		if (component == null)
		{
			return null;
		}
		return component as T;
	}

	public static int GetHierarchyDepth(this Transform xform)
	{
		int num = 0;
		Transform parent = xform.parent;
		while (parent != null)
		{
			num++;
			parent = parent.parent;
		}
		return num;
	}
}
