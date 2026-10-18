using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class UnityObjectUtils
{
	public static void Destroy(Object obj, bool withUndo = false)
	{
		if (Application.isPlaying)
		{
			Object.Destroy(obj);
		}
	}

	public static T ConvertUnityObjectToType<T>(Object objectIn) where T : class
	{
		T val = objectIn as T;
		if (val == null && objectIn != null)
		{
			GameObject gameObject = objectIn as GameObject;
			if (gameObject != null)
			{
				return gameObject.GetComponent<T>();
			}
			Component component = objectIn as Component;
			if (component != null)
			{
				val = component.GetComponent<T>();
			}
		}
		return val;
	}

	public static void RemoveDestroyedObjects<T>(List<T> list) where T : Object
	{
		List<T> collection = CollectionPool<List<T>, T>.GetCollection();
		foreach (T item in list)
		{
			if (item == null)
			{
				collection.Add(item);
			}
		}
		foreach (T item2 in collection)
		{
			list.Remove(item2);
		}
		CollectionPool<List<T>, T>.RecycleCollection(collection);
	}

	public static void RemoveDestroyedKeys<TKey, TValue>(Dictionary<TKey, TValue> dictionary) where TKey : Object
	{
		List<TKey> collection = CollectionPool<List<TKey>, TKey>.GetCollection();
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			TKey key = item.Key;
			if (key == null)
			{
				collection.Add(key);
			}
		}
		foreach (TKey item2 in collection)
		{
			dictionary.Remove(item2);
		}
		CollectionPool<List<TKey>, TKey>.RecycleCollection(collection);
	}
}
