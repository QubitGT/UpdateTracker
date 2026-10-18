using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Backtrace.Unity.Model.Breadcrumbs;

internal sealed class BacktraceBreadcrumbsEventHandler
{
	private readonly BacktraceBreadcrumbs _breadcrumbs;

	private BacktraceBreadcrumbType _registeredLevel;

	private NetworkReachability _networkStatus;

	private Thread _thread;

	public bool HasRegisteredEvents { get; set; }

	public BacktraceBreadcrumbsEventHandler(BacktraceBreadcrumbs breadcrumbs)
	{
		_thread = Thread.CurrentThread;
		_breadcrumbs = breadcrumbs;
		HasRegisteredEvents = false;
	}

	public void Register(BacktraceBreadcrumbType level)
	{
		_registeredLevel = level;
		if (_registeredLevel.HasFlag(BacktraceBreadcrumbType.Navigation))
		{
			HasRegisteredEvents = true;
			SceneManager.activeSceneChanged += HandleSceneChanged;
			SceneManager.sceneLoaded += SceneManager_sceneLoaded;
			SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
		}
		if (_registeredLevel.HasFlag(BacktraceBreadcrumbType.System))
		{
			HasRegisteredEvents = true;
			Application.lowMemory += HandleLowMemory;
			Application.quitting += HandleApplicationQuitting;
			Application.focusChanged += Application_focusChanged;
		}
		if (_registeredLevel.HasFlag(BacktraceBreadcrumbType.Log))
		{
			HasRegisteredEvents = true;
			Application.logMessageReceived += HandleMessage;
			Application.logMessageReceivedThreaded += HandleBackgroundMessage;
		}
	}

	public void Unregister()
	{
		if (HasRegisteredEvents)
		{
			if (_registeredLevel.HasFlag(BacktraceBreadcrumbType.Navigation))
			{
				SceneManager.activeSceneChanged -= HandleSceneChanged;
				SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
				SceneManager.sceneUnloaded -= SceneManager_sceneUnloaded;
			}
			if (_registeredLevel.HasFlag(BacktraceBreadcrumbType.System))
			{
				Application.lowMemory -= HandleLowMemory;
				Application.quitting -= HandleApplicationQuitting;
				Application.focusChanged -= Application_focusChanged;
			}
			if (_registeredLevel.HasFlag(BacktraceBreadcrumbType.Log))
			{
				Application.logMessageReceived -= HandleMessage;
				Application.logMessageReceivedThreaded -= HandleBackgroundMessage;
			}
		}
	}

	private void SceneManager_sceneUnloaded(Scene scene)
	{
		string message = $"SceneManager:scene {scene.name} unloaded";
		Log(message, LogType.Log, BreadcrumbLevel.Navigation);
	}

	private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		string message = $"SceneManager:scene {scene.name} loaded";
		Log(message, LogType.Log, BreadcrumbLevel.Navigation, new Dictionary<string, string> { 
		{
			"LoadSceneMode",
			loadSceneMode.ToString()
		} });
	}

	private void HandleSceneChanged(Scene sceneFrom, Scene sceneTo)
	{
		string message = string.Format("SceneManager:scene changed from {0} to {1}", string.IsNullOrEmpty(sceneFrom.name) ? "(no scene)" : sceneFrom.name, sceneTo.name);
		Log(message, LogType.Log, BreadcrumbLevel.Navigation, new Dictionary<string, string>
		{
			{ "from", sceneFrom.name },
			{ "to", sceneTo.name }
		});
	}

	private void HandleLowMemory()
	{
		Log("Application:low memory", LogType.Warning, BreadcrumbLevel.System);
	}

	private void HandleApplicationQuitting()
	{
		Log("Application:quitting", LogType.Log, BreadcrumbLevel.System);
	}

	private void HandleBackgroundMessage(string condition, string stackTrace, LogType type)
	{
		if (Thread.CurrentThread != _thread)
		{
			HandleMessage(condition, stackTrace, type);
		}
	}

	private void HandleMessage(string condition, string stackTrace, LogType type)
	{
		Dictionary<string, string> attributes = ((type == LogType.Error || type == LogType.Exception) ? new Dictionary<string, string> { { "stackTrace", stackTrace } } : null);
		Log(condition, type, BreadcrumbLevel.Log, attributes);
	}

	private void Application_focusChanged(bool hasFocus)
	{
		Log("Application:focus changed.", LogType.Assert, BreadcrumbLevel.System, new Dictionary<string, string> { 
		{
			"hasFocus",
			hasFocus.ToString()
		} });
	}

	private void Log(string message, LogType level, BreadcrumbLevel breadcrumbLevel, IDictionary<string, string> attributes = null)
	{
		UnityEngineLogLevel type = BacktraceBreadcrumbs.ConvertLogTypeToLogLevel(level);
		if (_breadcrumbs.ShouldLog(breadcrumbLevel, type))
		{
			_breadcrumbs.AddBreadcrumbs(message, breadcrumbLevel, type, attributes);
		}
	}

	private void LogNewNetworkStatus(NetworkReachability status)
	{
		_networkStatus = status;
		Log($"Network:{status}", LogType.Log, BreadcrumbLevel.System);
	}

	internal void Update()
	{
		if (_registeredLevel.HasFlag(BacktraceBreadcrumbType.System) && Application.internetReachability != _networkStatus)
		{
			LogNewNetworkStatus(Application.internetReachability);
		}
	}
}
