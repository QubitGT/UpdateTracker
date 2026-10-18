using System;
using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public static class SingletonMonoBehaviour
{
	public class InstantiationSettings : Attribute
	{
		public bool dontDestroyOnLoad;
	}
}
public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
	private static T _instance;

	public static T Instance
	{
		get
		{
			if (_instance == null && Application.isPlaying)
			{
				T[] array = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
				if (array.Length != 0)
				{
					_instance = array[0];
				}
			}
			return _instance;
		}
	}

	private static void InitializeSingleton()
	{
		if (Attribute.GetCustomAttribute(typeof(T), typeof(SingletonMonoBehaviour.InstantiationSettings)) is SingletonMonoBehaviour.InstantiationSettings { dontDestroyOnLoad: not false })
		{
			UnityEngine.Object.DontDestroyOnLoad(_instance.transform);
		}
	}

	protected virtual void Awake()
	{
		if (_instance == null)
		{
			_instance = this as T;
			InitializeSingleton();
		}
		else if (_instance != this)
		{
			Debug.LogWarning($"An instance of {typeof(T)} already exists, destroying this instance.");
			UnityEngine.Object.Destroy(this);
		}
	}

	protected virtual void OnDestroy()
	{
		_instance = null;
	}
}
