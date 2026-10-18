using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Oculus.Interaction;

public class Context : MonoBehaviour
{
	public class Instance
	{
		private readonly string _name;

		private Context _instance;

		public Instance(string name)
		{
			_name = name;
		}

		public Context GetInstance()
		{
			if (_instance == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.name = _name;
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
				}
				_instance = gameObject.AddComponent<Context>();
			}
			return _instance;
		}
	}

	private static SynchronizationContext _unityMainThreadSynchronizationContext = null;

	private static Queue<Action> _unityMainThreadWork = new Queue<Action>();

	private static Mutex _unityMainThreadWorkMutex = new Mutex();

	private readonly ConcurrentDictionary<Type, object> _singletons = new ConcurrentDictionary<Type, object>();

	public static Instance Global { get; } = new Instance("Global Context");


	public event Action WhenDestroyed;

	public static void ExecuteOnMainThread(Action work)
	{
		_unityMainThreadWorkMutex.WaitOne();
		if (_unityMainThreadSynchronizationContext != null)
		{
			_unityMainThreadSynchronizationContext.Post(delegate
			{
				work();
			}, null);
		}
		else
		{
			_unityMainThreadWork.Enqueue(work);
		}
		_unityMainThreadWorkMutex.ReleaseMutex();
	}

	private void Awake()
	{
		if (_unityMainThreadSynchronizationContext == null)
		{
			_unityMainThreadWorkMutex.WaitOne();
			_unityMainThreadSynchronizationContext = SynchronizationContext.Current;
			Action result;
			while (_unityMainThreadWork.TryDequeue(out result))
			{
				result();
			}
			_unityMainThreadWorkMutex.ReleaseMutex();
		}
	}

	public T GetOrCreateSingleton<T>() where T : class, new()
	{
		Type typeFromHandle = typeof(T);
		if (!_singletons.TryGetValue(typeFromHandle, out var value))
		{
			value = new T();
			_singletons.TryAdd(typeFromHandle, value);
		}
		return value as T;
	}

	public T GetOrCreateSingleton<T>(Func<T> factory) where T : class
	{
		Type typeFromHandle = typeof(T);
		if (!_singletons.TryGetValue(typeFromHandle, out var value))
		{
			value = factory();
			_singletons.TryAdd(typeFromHandle, value);
		}
		return value as T;
	}

	private void OnDestroy()
	{
		this.WhenDestroyed?.Invoke();
	}
}
