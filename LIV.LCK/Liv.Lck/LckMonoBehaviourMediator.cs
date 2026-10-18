using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Liv.Lck;

[DefaultExecutionOrder(-1000)]
public class LckMonoBehaviourMediator : MonoBehaviour
{
	public enum ApplicationLifecycleEventType
	{
		Quit,
		Pause,
		Resume,
		HMDIdle,
		HMDActive
	}

	public delegate void LckApplicationLifecycleEventDelegate(ApplicationLifecycleEventType applicationLifecycleEventType);

	private static readonly Queue<Action> _executionQueue = new Queue<Action>();

	private static LckMonoBehaviourMediator _instance;

	private const float DurationForHMDToBecomeIdle = 10f;

	private float _hMDIdleTime;

	private Dictionary<string, Coroutine> _activeCoroutines = new Dictionary<string, Coroutine>();

	private bool _hMDFound;

	private bool _hMDWasMoving;

	private bool _hMDIsIdle;

	private InputDevice _hmd;

	public static LckMonoBehaviourMediator Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject obj = new GameObject("LckMonoBehaviourMediator");
				_instance = obj.AddComponent<LckMonoBehaviourMediator>();
				UnityEngine.Object.DontDestroyOnLoad(obj);
			}
			return _instance;
		}
	}

	public static event LckApplicationLifecycleEventDelegate OnApplicationLifecycleEvent;

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		LckMonoBehaviourMediator.OnApplicationLifecycleEvent?.Invoke(pauseStatus ? ApplicationLifecycleEventType.Pause : ApplicationLifecycleEventType.Resume);
	}

	private void OnApplicationQuit()
	{
		LckMonoBehaviourMediator.OnApplicationLifecycleEvent?.Invoke(ApplicationLifecycleEventType.Quit);
	}

	private void Update()
	{
		HMDMountedOnHeadStateChange();
		ProcessExectionQueue();
	}

	private void HMDMountedOnHeadStateChange()
	{
		if (_hMDFound)
		{
			_hmd.TryGetFeatureValue(CommonUsages.deviceVelocity, out var value);
			if (value.magnitude > 0.01f)
			{
				if (_hMDIsIdle)
				{
					_hMDIsIdle = false;
					LckMonoBehaviourMediator.OnApplicationLifecycleEvent?.Invoke(ApplicationLifecycleEventType.HMDActive);
				}
				_hMDWasMoving = true;
				_hMDIdleTime = 0f;
			}
			else if (_hMDWasMoving)
			{
				_hMDIdleTime += Time.deltaTime;
				if (_hMDIdleTime >= 10f)
				{
					_hMDIsIdle = true;
					LckMonoBehaviourMediator.OnApplicationLifecycleEvent?.Invoke(ApplicationLifecycleEventType.HMDIdle);
					_hMDWasMoving = false;
				}
			}
			return;
		}
		List<InputDevice> list = new List<InputDevice>();
		InputDevices.GetDevices(list);
		foreach (InputDevice item in list)
		{
			if (item.characteristics.HasFlag(InputDeviceCharacteristics.HeadMounted))
			{
				_hmd = item;
				_hMDFound = true;
			}
		}
	}

	private static void ProcessExectionQueue()
	{
		lock (_executionQueue)
		{
			while (_executionQueue.Count > 0)
			{
				_executionQueue.Dequeue()();
			}
		}
	}

	public static T[] FindObjectsOfComponentType<T>() where T : UnityEngine.Object
	{
		return UnityEngine.Object.FindObjectsOfType<T>();
	}

	public static T AddComponentToMediator<T>() where T : Component
	{
		return Instance.gameObject.AddComponent<T>();
	}

	public static Coroutine StartCoroutine(string coroutineName, IEnumerator routine)
	{
		return Instance.StartCoroutineInternal(coroutineName, routine);
	}

	public static void StopCoroutineByName(string coroutineName)
	{
		if (_instance != null)
		{
			Instance.StopCoroutineInternal(coroutineName);
		}
	}

	public static void StopAllActiveCoroutines()
	{
		if (_instance != null)
		{
			Instance.StopAllCoroutinesInternal();
		}
	}

	public void EnqueueMainThreadAction(Action action)
	{
		lock (_executionQueue)
		{
			_executionQueue.Enqueue(action);
		}
	}

	private Coroutine StartCoroutineInternal(string coroutineName, IEnumerator routine)
	{
		if (_activeCoroutines.ContainsKey(coroutineName))
		{
			StopCoroutineInternal(coroutineName);
		}
		Coroutine coroutine = StartCoroutine(routine);
		_activeCoroutines[coroutineName] = coroutine;
		return coroutine;
	}

	private void StopCoroutineInternal(string coroutineName)
	{
		if (_activeCoroutines.TryGetValue(coroutineName, out var value))
		{
			try
			{
				StopCoroutine(value);
			}
			catch (Exception)
			{
			}
			_activeCoroutines.Remove(coroutineName);
		}
	}

	private void StopAllCoroutinesInternal()
	{
		StopAllCoroutines();
		_activeCoroutines.Clear();
	}

	private void OnDestroy()
	{
		StopAllCoroutinesInternal();
	}
}
