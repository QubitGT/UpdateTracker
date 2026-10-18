using System;
using System.Collections;
using UnityEngine.XR.Management;

namespace UnityEngine.XR.OpenXR;

internal class OpenXRRestarter : MonoBehaviour
{
	internal Action onAfterRestart;

	internal Action onAfterShutdown;

	internal Action onQuit;

	internal Action onAfterCoroutine;

	internal Action onAfterSuccessfulRestart;

	private static OpenXRRestarter s_Instance;

	private Coroutine m_Coroutine;

	private static int m_pauseAndRestartCoroutineCount;

	private Object m_PauseAndRestartCoroutineCountLock = new Object();

	private static int m_pauseAndRestartAttempts;

	public bool isRunning => m_Coroutine != null;

	public static float TimeBetweenRestartAttempts { get; set; }

	public static int PauseAndRestartAttempts => m_pauseAndRestartAttempts;

	internal static int PauseAndRestartCoroutineCount => m_pauseAndRestartCoroutineCount;

	public static OpenXRRestarter Instance
	{
		get
		{
			if (s_Instance == null)
			{
				GameObject gameObject = GameObject.Find("~oxrestarter");
				if (gameObject == null)
				{
					gameObject = new GameObject("~oxrestarter");
					gameObject.hideFlags = HideFlags.HideAndDontSave;
					gameObject.AddComponent<OpenXRRestarter>();
				}
				s_Instance = gameObject.GetComponent<OpenXRRestarter>();
			}
			return s_Instance;
		}
	}

	internal static bool DisableApplicationQuit { get; set; }

	static OpenXRRestarter()
	{
		TimeBetweenRestartAttempts = 5f;
		DisableApplicationQuit = false;
	}

	public void ResetCallbacks()
	{
		onAfterRestart = null;
		onAfterSuccessfulRestart = null;
		onAfterShutdown = null;
		onAfterCoroutine = null;
		onQuit = null;
		m_pauseAndRestartAttempts = 0;
	}

	public void Shutdown()
	{
		if (!(OpenXRLoaderBase.Instance == null))
		{
			if (m_Coroutine != null)
			{
				Debug.LogError("Only one shutdown or restart can be executed at a time");
			}
			else
			{
				m_Coroutine = StartCoroutine(RestartCoroutine(shouldRestart: false, shouldShutdown: true));
			}
		}
	}

	public void ShutdownAndRestart()
	{
		if (!(OpenXRLoaderBase.Instance == null))
		{
			if (m_Coroutine != null)
			{
				Debug.LogError("Only one shutdown or restart can be executed at a time");
			}
			else
			{
				m_Coroutine = StartCoroutine(RestartCoroutine(shouldRestart: true, shouldShutdown: true));
			}
		}
	}

	public void PauseAndShutdownAndRestart()
	{
		if (!(OpenXRLoaderBase.Instance == null))
		{
			StartCoroutine(PauseAndShutdownAndRestartCoroutine(TimeBetweenRestartAttempts));
		}
	}

	public void PauseAndRetryInitialization()
	{
		if (!(OpenXRLoaderBase.Instance == null))
		{
			StartCoroutine(PauseAndRetryInitializationCoroutine(TimeBetweenRestartAttempts));
		}
	}

	private void IncrementPauseAndRestartCoroutineCount()
	{
		lock (m_PauseAndRestartCoroutineCountLock)
		{
			m_pauseAndRestartCoroutineCount++;
		}
	}

	private void DecrementPauseAndRestartCoroutineCount()
	{
		lock (m_PauseAndRestartCoroutineCountLock)
		{
			m_pauseAndRestartCoroutineCount--;
		}
	}

	private IEnumerator PauseAndShutdownAndRestartCoroutine(float pauseTimeInSeconds)
	{
		IncrementPauseAndRestartCoroutineCount();
		try
		{
			yield return new WaitForSeconds(pauseTimeInSeconds);
			yield return new WaitForRestartFinish();
			m_pauseAndRestartAttempts++;
			m_Coroutine = StartCoroutine(RestartCoroutine(shouldRestart: true, shouldShutdown: true));
		}
		finally
		{
			onAfterCoroutine?.Invoke();
		}
		DecrementPauseAndRestartCoroutineCount();
	}

	private IEnumerator PauseAndRetryInitializationCoroutine(float pauseTimeInSeconds)
	{
		IncrementPauseAndRestartCoroutineCount();
		try
		{
			yield return new WaitForSeconds(pauseTimeInSeconds);
			yield return new WaitForRestartFinish();
			if (!(XRGeneralSettings.Instance.Manager.activeLoader != null))
			{
				m_pauseAndRestartAttempts++;
				m_Coroutine = StartCoroutine(RestartCoroutine(shouldRestart: true, shouldShutdown: false));
			}
		}
		finally
		{
			onAfterCoroutine?.Invoke();
		}
		DecrementPauseAndRestartCoroutineCount();
	}

	private IEnumerator RestartCoroutine(bool shouldRestart, bool shouldShutdown)
	{
		try
		{
			if (shouldShutdown)
			{
				Debug.Log("Shutting down OpenXR.");
				yield return null;
				XRGeneralSettings.Instance.Manager.DeinitializeLoader();
				yield return null;
				onAfterShutdown?.Invoke();
			}
			if (shouldRestart && OpenXRRuntime.ShouldRestart())
			{
				Debug.Log("Initializing OpenXR.");
				yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
				XRGeneralSettings.Instance.Manager.StartSubsystems();
				if (XRGeneralSettings.Instance.Manager.activeLoader != null)
				{
					m_pauseAndRestartAttempts = 0;
					onAfterSuccessfulRestart?.Invoke();
				}
				onAfterRestart?.Invoke();
			}
			else if (OpenXRRuntime.ShouldQuit())
			{
				onQuit?.Invoke();
				if (!DisableApplicationQuit)
				{
					Application.Quit();
				}
			}
		}
		finally
		{
			OpenXRRestarter openXRRestarter = this;
			openXRRestarter.m_Coroutine = null;
			openXRRestarter.onAfterCoroutine?.Invoke();
		}
	}
}
