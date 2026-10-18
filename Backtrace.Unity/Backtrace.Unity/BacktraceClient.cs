using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Backtrace.Unity.Common;
using Backtrace.Unity.Interfaces;
using Backtrace.Unity.Model;
using Backtrace.Unity.Model.Breadcrumbs;
using Backtrace.Unity.Model.Database;
using Backtrace.Unity.Model.JsonData;
using Backtrace.Unity.Runtime.Native;
using Backtrace.Unity.Services;
using Backtrace.Unity.Types;
using UnityEngine;

namespace Backtrace.Unity;

public class BacktraceClient : MonoBehaviour, IBacktraceClient
{
	public const string VERSION = "3.9.1";

	internal const string DefaultBacktraceGameObjectName = "BacktraceClient";

	public BacktraceConfiguration Configuration;

	private BacktraceBreadcrumbs _breadcrumbs;

	private AttributeProvider _attributeProvider;

	private BacktraceMetrics _metrics;

	private System.Random _random;

	internal Stack<BacktraceReport> BackgroundExceptions = new Stack<BacktraceReport>();

	private HashSet<string> _clientReportAttachments;

	private static BacktraceClient _instance;

	public IBacktraceDatabase Database;

	private IBacktraceApi _backtraceApi;

	private ReportLimitWatcher _reportLimitWatcher;

	private BacktraceLogManager _backtraceLogManager;

	internal Action<BacktraceReport> _onClientReportLimitReached;

	public Func<BacktraceData, BacktraceData> BeforeSend;

	public Func<ReportFilterType, Exception, string, bool> SkipReport;

	public Action<Exception> OnUnhandledApplicationException;

	private INativeClient _nativeClient;

	private Thread _current;

	public IBacktraceBreadcrumbs Breadcrumbs => _breadcrumbs;

	public bool Enabled { get; private set; }

	internal AttributeProvider AttributeProvider
	{
		get
		{
			if (_attributeProvider == null)
			{
				_attributeProvider = new AttributeProvider();
			}
			return _attributeProvider;
		}
		set
		{
			_attributeProvider = value;
		}
	}

	public IBacktraceMetrics Metrics
	{
		get
		{
			if (_metrics == null && Configuration != null && Configuration.EnableMetricsSupport)
			{
				string universeName = Configuration.GetUniverseName();
				string token = Configuration.GetToken();
				_metrics = new BacktraceMetrics(AttributeProvider, Configuration.GetEventAggregationIntervalTimerInMs(), BacktraceMetrics.GetDefaultUniqueEventsUrl(universeName, token), BacktraceMetrics.GetDefaultSummedEventsUrl(universeName, token))
				{
					IgnoreSslValidation = Configuration.IgnoreSslValidation
				};
			}
			return _metrics;
		}
	}

	internal System.Random Random
	{
		get
		{
			if (_random == null)
			{
				_random = new System.Random();
			}
			return _random;
		}
	}

	public string this[string index]
	{
		get
		{
			return AttributeProvider[index];
		}
		set
		{
			AttributeProvider[index] = value;
			if (_nativeClient != null)
			{
				_nativeClient.SetAttribute(index, value);
			}
		}
	}

	public static BacktraceClient Instance => _instance;

	public Action<Exception> OnServerError
	{
		get
		{
			if (BacktraceApi != null)
			{
				return BacktraceApi.OnServerError;
			}
			return null;
		}
		set
		{
			if (ValidClientConfiguration())
			{
				BacktraceApi.OnServerError = value;
			}
		}
	}

	public Func<string, BacktraceData, BacktraceResult> RequestHandler
	{
		get
		{
			if (BacktraceApi != null)
			{
				return BacktraceApi.RequestHandler;
			}
			return null;
		}
		set
		{
			if (ValidClientConfiguration())
			{
				BacktraceApi.RequestHandler = value;
			}
		}
	}

	public Action<BacktraceResult> OnServerResponse
	{
		get
		{
			if (BacktraceApi != null)
			{
				return BacktraceApi.OnServerResponse;
			}
			return null;
		}
		set
		{
			if (ValidClientConfiguration())
			{
				BacktraceApi.OnServerResponse = value;
			}
		}
	}

	public Action<BacktraceReport> OnClientReportLimitReached
	{
		get
		{
			return _onClientReportLimitReached;
		}
		set
		{
			if (ValidClientConfiguration())
			{
				_onClientReportLimitReached = value;
			}
		}
	}

	internal INativeClient NativeClient => _nativeClient;

	public bool EnablePerformanceStatistics => Configuration.PerformanceStatistics;

	public int GameObjectDepth
	{
		get
		{
			if (Configuration.GameObjectDepth != 0)
			{
				return Configuration.GameObjectDepth;
			}
			return 16;
		}
	}

	internal IBacktraceApi BacktraceApi
	{
		get
		{
			return _backtraceApi;
		}
		set
		{
			_backtraceApi = value;
			if (Database != null)
			{
				Database.SetApi(_backtraceApi);
			}
		}
	}

	internal ReportLimitWatcher ReportLimitWatcher
	{
		get
		{
			return _reportLimitWatcher;
		}
		set
		{
			_reportLimitWatcher = value;
			if (Database != null)
			{
				Database.SetReportWatcher(_reportLimitWatcher);
			}
		}
	}

	public void AddAttachment(string pathToAttachment)
	{
		_clientReportAttachments.Add(pathToAttachment);
	}

	public IEnumerable<string> GetAttachments()
	{
		return _clientReportAttachments;
	}

	public void SetAttributes(Dictionary<string, string> attributes)
	{
		if (attributes == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> attribute in attributes)
		{
			this[attribute.Key] = attribute.Value;
		}
	}

	public int GetAttributesCount()
	{
		return AttributeProvider.Count();
	}

	public static BacktraceClient Initialize(BacktraceConfiguration configuration, Dictionary<string, string> attributes = null, string gameObjectName = "BacktraceClient")
	{
		if (string.IsNullOrEmpty(gameObjectName))
		{
			throw new ArgumentException("Missing game object name");
		}
		if (configuration == null || string.IsNullOrEmpty(configuration.ServerUrl))
		{
			throw new ArgumentException("Missing valid configuration");
		}
		if (Instance != null)
		{
			return Instance;
		}
		GameObject gameObject = new GameObject(gameObjectName, typeof(BacktraceClient), typeof(BacktraceDatabase));
		BacktraceClient component = gameObject.GetComponent<BacktraceClient>();
		component.Configuration = configuration;
		if (configuration.Enabled)
		{
			gameObject.GetComponent<BacktraceDatabase>().Configuration = configuration;
		}
		gameObject.SetActive(value: true);
		component.Refresh();
		component.SetAttributes(attributes);
		return component;
	}

	public static BacktraceClient Initialize(string url, string databasePath, Dictionary<string, string> attributes = null, string gameObjectName = "BacktraceClient")
	{
		return Initialize(url, databasePath, attributes, null, gameObjectName);
	}

	public static BacktraceClient Initialize(string url, string databasePath, Dictionary<string, string> attributes = null, string[] attachments = null, string gameObjectName = "BacktraceClient")
	{
		BacktraceConfiguration backtraceConfiguration = ScriptableObject.CreateInstance<BacktraceConfiguration>();
		backtraceConfiguration.ServerUrl = url;
		backtraceConfiguration.AttachmentPaths = attachments;
		backtraceConfiguration.Enabled = true;
		backtraceConfiguration.DatabasePath = databasePath;
		backtraceConfiguration.CreateDatabase = true;
		return Initialize(backtraceConfiguration, attributes, gameObjectName);
	}

	public static BacktraceClient Initialize(string url, Dictionary<string, string> attributes = null, string gameObjectName = "BacktraceClient")
	{
		return Initialize(url, attributes, new string[0], gameObjectName);
	}

	public static BacktraceClient Initialize(string url, Dictionary<string, string> attributes = null, string[] attachments = null, string gameObjectName = "BacktraceClient")
	{
		BacktraceConfiguration backtraceConfiguration = ScriptableObject.CreateInstance<BacktraceConfiguration>();
		backtraceConfiguration.ServerUrl = url;
		backtraceConfiguration.AttachmentPaths = attachments;
		backtraceConfiguration.Enabled = false;
		return Initialize(backtraceConfiguration, attributes, gameObjectName);
	}

	public void OnDisable()
	{
		Enabled = false;
	}

	public void Refresh()
	{
		if (Configuration == null || !Configuration.IsValid() || Instance != null)
		{
			return;
		}
		Enabled = true;
		_current = Thread.CurrentThread;
		CaptureUnityMessages();
		_reportLimitWatcher = new ReportLimitWatcher(Convert.ToUInt32(Configuration.ReportPerMin));
		_clientReportAttachments = Configuration.GetAttachmentPaths();
		BacktraceApi = new BacktraceApi(new BacktraceCredentials(Configuration.GetValidServerUrl()), Configuration.IgnoreSslValidation);
		BacktraceApi.EnablePerformanceStatistics = Configuration.PerformanceStatistics;
		if (!Configuration.DestroyOnLoad)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			_instance = this;
		}
		EnableMetrics(enableIfConfigurationIsDisabled: false);
		string text = string.Empty;
		if (Configuration.Enabled)
		{
			Database = GetComponent<BacktraceDatabase>();
			if (Database != null)
			{
				Database.Reload();
				_breadcrumbs = (BacktraceBreadcrumbs)Database.Breadcrumbs;
				Database.SetApi(BacktraceApi);
				Database.SetReportWatcher(_reportLimitWatcher);
				if (_breadcrumbs != null)
				{
					text = _breadcrumbs.GetBreadcrumbLogPath();
				}
			}
		}
		if (Database != null)
		{
			IDictionary<string, string> attributes = AttributeProvider.GenerateAttributes(includeDynamic: false);
			IList<string> nativeAttachments = GetNativeAttachments();
			if (!string.IsNullOrEmpty(text))
			{
				nativeAttachments.Add(text);
			}
			_nativeClient = NativeClientFactory.CreateNativeClient(Configuration, base.name, _breadcrumbs, attributes, nativeAttachments);
			AttributeProvider.AddDynamicAttributeProvider(_nativeClient);
		}
	}

	public bool EnableBreadcrumbsSupport()
	{
		if (Database == null)
		{
			return false;
		}
		return Database.EnableBreadcrumbsSupport();
	}

	public bool EnableMetrics()
	{
		return EnableMetrics(enableIfConfigurationIsDisabled: true);
	}

	private bool EnableMetrics(bool enableIfConfigurationIsDisabled = true)
	{
		if (!Configuration.EnableMetricsSupport)
		{
			if (!enableIfConfigurationIsDisabled)
			{
				return false;
			}
			UnityEngine.Debug.LogWarning("Event aggregation configuration was disabled. Enabling it manually via API");
		}
		return EnableMetrics("guid");
	}

	public bool EnableMetrics(string uniqueAttributeName = "guid")
	{
		string universeName = Configuration.GetUniverseName();
		if (string.IsNullOrEmpty(universeName))
		{
			UnityEngine.Debug.LogWarning("Cannot initialize event aggregation - Unknown Backtrace URL.");
			return false;
		}
		string token = Configuration.GetToken();
		EnableMetrics(BacktraceMetrics.GetDefaultUniqueEventsUrl(universeName, token), BacktraceMetrics.GetDefaultSummedEventsUrl(universeName, token), Configuration.GetEventAggregationIntervalTimerInMs(), uniqueAttributeName);
		return true;
	}

	public bool EnableMetrics(string uniqueEventsSubmissionUrl, string summedEventsSubmissionUrl, uint timeIntervalInSec = 1800u, string uniqueAttributeName = "guid")
	{
		if (_metrics != null)
		{
			UnityEngine.Debug.LogWarning("Backtrace metrics support is already enabled. Please use BacktraceClient.Metrics.");
			return false;
		}
		_metrics = new BacktraceMetrics(AttributeProvider, timeIntervalInSec, uniqueEventsSubmissionUrl, summedEventsSubmissionUrl)
		{
			StartupUniqueAttributeName = uniqueAttributeName,
			IgnoreSslValidation = Configuration.IgnoreSslValidation
		};
		StartupMetrics();
		return true;
	}

	private void StartupMetrics()
	{
		AttributeProvider.AddScopedAttributeProvider(Metrics);
		_metrics.SendStartupEvent();
	}

	private void OnApplicationQuit()
	{
		if (_nativeClient != null)
		{
			_nativeClient.Disable();
		}
	}

	private void Awake()
	{
		if (_breadcrumbs != null)
		{
			_breadcrumbs.FromMonoBehavior("Application awake", LogType.Assert, null);
		}
		Refresh();
	}

	private void LateUpdate()
	{
		if (_nativeClient != null)
		{
			_nativeClient.Update(Time.unscaledTime);
		}
		if (_metrics != null)
		{
			_metrics.Tick(Time.unscaledTime);
		}
		if (BackgroundExceptions.Count != 0)
		{
			while (BackgroundExceptions.Count > 0)
			{
				SendReport(BackgroundExceptions.Pop());
			}
		}
	}

	private void OnDestroy()
	{
		Enabled = false;
		if (_breadcrumbs != null)
		{
			_breadcrumbs.FromMonoBehavior("Backtrace Client: OnDestroy", LogType.Warning, null);
			_breadcrumbs.UnregisterEvents();
		}
		_instance = null;
		Application.logMessageReceived -= HandleUnityMessage;
		Application.logMessageReceivedThreaded -= HandleUnityBackgroundException;
		if (_nativeClient != null)
		{
			_nativeClient.Disable();
		}
	}

	public void SetClientReportLimit(uint reportPerMin)
	{
		if (!Enabled)
		{
			UnityEngine.Debug.LogWarning("Please enable BacktraceClient first.");
		}
		else
		{
			_reportLimitWatcher.SetClientReportLimit(reportPerMin);
		}
	}

	public void Send(string message, List<string> attachmentPaths = null, Dictionary<string, string> attributes = null)
	{
		if (ShouldSendReport(message, attachmentPaths, attributes))
		{
			BacktraceReport report = new BacktraceReport(message, attributes, attachmentPaths);
			if (_breadcrumbs != null)
			{
				_breadcrumbs.FromBacktrace(report);
			}
			_backtraceLogManager.Enqueue(report);
			SendReport(report);
		}
	}

	public void Send(Exception exception, List<string> attachmentPaths = null, Dictionary<string, string> attributes = null)
	{
		if (ShouldSendReport(exception, attachmentPaths, attributes))
		{
			BacktraceReport report = new BacktraceReport(exception, attributes, attachmentPaths);
			if (_breadcrumbs != null)
			{
				_breadcrumbs.FromBacktrace(report);
			}
			_backtraceLogManager.Enqueue(report);
			SendReport(report);
		}
	}

	public void Send(BacktraceReport report, Action<BacktraceResult> sendCallback = null)
	{
		if (ShouldSendReport(report))
		{
			if (_breadcrumbs != null)
			{
				_breadcrumbs.FromBacktrace(report);
			}
			_backtraceLogManager.Enqueue(report);
			SendReport(report, sendCallback);
		}
	}

	private void SendReport(BacktraceReport report, Action<BacktraceResult> sendCallback = null)
	{
		if (BacktraceApi == null)
		{
			UnityEngine.Debug.LogWarning("Backtrace API doesn't exist. Please validate client token or server url!");
		}
		else if (Enabled)
		{
			StartCoroutine(CollectDataAndSend(report, sendCallback));
		}
	}

	private IEnumerator CollectDataAndSend(BacktraceReport report, Action<BacktraceResult> sendCallback)
	{
		Dictionary<string, string> queryAttributes = new Dictionary<string, string>();
		Stopwatch stopWatch = (EnablePerformanceStatistics ? Stopwatch.StartNew() : new Stopwatch());
		BacktraceData data = SetupBacktraceData(report);
		if (EnablePerformanceStatistics)
		{
			stopWatch.Stop();
			queryAttributes["performance.report"] = stopWatch.GetMicroseconds();
		}
		if (BeforeSend != null)
		{
			data = BeforeSend(data);
			if (data == null)
			{
				yield break;
			}
		}
		BacktraceDatabaseRecord record = null;
		if (Database != null && Database.Enabled())
		{
			yield return WaitForFrame.Wait();
			if (EnablePerformanceStatistics)
			{
				stopWatch.Restart();
			}
			record = Database.Add(data);
			if (record == null)
			{
				yield break;
			}
			data = record.BacktraceData;
			if (EnablePerformanceStatistics)
			{
				stopWatch.Stop();
				queryAttributes["performance.database"] = stopWatch.GetMicroseconds();
			}
			if (record.Duplicated)
			{
				record.Unlock();
				yield break;
			}
		}
		if (EnablePerformanceStatistics)
		{
			stopWatch.Restart();
		}
		string json = ((record != null) ? record.BacktraceDataJson() : data.ToJson());
		if (EnablePerformanceStatistics)
		{
			stopWatch.Stop();
			queryAttributes["performance.json"] = stopWatch.GetMicroseconds();
		}
		yield return WaitForFrame.Wait();
		if (string.IsNullOrEmpty(json))
		{
			yield break;
		}
		if (RequestHandler != null)
		{
			yield return RequestHandler(BacktraceApi.ServerUrl, data);
			yield break;
		}
		if (data.Deduplication != 0)
		{
			queryAttributes["_mod_duplicate"] = data.Deduplication.ToString(CultureInfo.InvariantCulture);
		}
		StartCoroutine(BacktraceApi.Send(json, data.Attachments, queryAttributes, delegate(BacktraceResult result)
		{
			if (record != null)
			{
				record.Unlock();
				if (Database != null && result.Status != BacktraceResultStatus.ServerError && result.Status != BacktraceResultStatus.NetworkError)
				{
					Database.Delete(record);
				}
			}
			HandleInnerException(report);
			if (sendCallback != null)
			{
				sendCallback(result);
			}
		}));
	}

	private BacktraceData SetupBacktraceData(BacktraceReport report)
	{
		string text = (_backtraceLogManager.Disabled ? new BacktraceUnityMessage(report).ToString() : _backtraceLogManager.ToSourceCode());
		report.AssignSourceCodeToReport(text);
		report.SetReportFingerprint(Configuration.UseNormalizedExceptionMessage);
		report.AttachmentPaths.AddRange(_clientReportAttachments);
		BacktraceData backtraceData = report.ToBacktraceData(null, GameObjectDepth);
		AttributeProvider.AddAttributes(backtraceData.Attributes.Attributes);
		return backtraceData;
	}

	private void CaptureUnityMessages()
	{
		_backtraceLogManager = new BacktraceLogManager(Configuration.NumberOfLogs);
		if (Configuration.HandleUnhandledExceptions)
		{
			Application.logMessageReceived += HandleUnityMessage;
			Application.logMessageReceivedThreaded += HandleUnityBackgroundException;
		}
	}

	internal void OnApplicationPause(bool pause)
	{
		if (_breadcrumbs != null)
		{
			_breadcrumbs.FromMonoBehavior("Application pause", LogType.Assert, new Dictionary<string, string> { 
			{
				"paused",
				pause.ToString(CultureInfo.InvariantCulture).ToLower()
			} });
		}
		if (_nativeClient != null)
		{
			_nativeClient.PauseAnrThread(pause);
		}
	}

	internal void HandleUnityBackgroundException(string message, string stackTrace, LogType type)
	{
		if (Thread.CurrentThread != _current)
		{
			HandleUnityMessage(message, stackTrace, type);
		}
	}

	internal void HandleUnityMessage(string message, string stackTrace, LogType type)
	{
		if (!Enabled)
		{
			return;
		}
		BacktraceUnityMessage unityMessage = new BacktraceUnityMessage(message, stackTrace, type);
		_backtraceLogManager.Enqueue(unityMessage);
		if (!Configuration.HandleUnhandledExceptions || string.IsNullOrEmpty(message) || (type != 0 && type != LogType.Exception))
		{
			return;
		}
		BacktraceUnhandledException ex = null;
		bool invokeSkipApi = true;
		if (type == LogType.Error)
		{
			if (Configuration.ReportFilterType.HasFlag(ReportFilterType.Error))
			{
				return;
			}
			if (SamplingShouldSkip())
			{
				if (SkipReport == null)
				{
					return;
				}
				ex = new BacktraceUnhandledException(message, stackTrace)
				{
					Type = type
				};
				if (ShouldSkipReport(ReportFilterType.Error, ex, string.Empty))
				{
					return;
				}
				invokeSkipApi = false;
			}
		}
		if (ex == null)
		{
			ex = new BacktraceUnhandledException(message, stackTrace)
			{
				Type = type
			};
		}
		BacktraceReport report = new BacktraceReport(ex);
		SendUnhandledExceptionReport(report, invokeSkipApi);
	}

	private bool SamplingShouldSkip()
	{
		if (!Configuration || Configuration.Sampling == 1.0)
		{
			return false;
		}
		return Random.NextDouble() > Configuration.Sampling;
	}

	private void SendUnhandledExceptionReport(BacktraceReport report, bool invokeSkipApi = true)
	{
		if (OnUnhandledApplicationException != null)
		{
			OnUnhandledApplicationException(report.Exception);
		}
		if (ShouldSendReport(report.Exception, null, null, invokeSkipApi))
		{
			SendReport(report);
		}
	}

	private bool ShouldSendReport(Exception exception, List<string> attachmentPaths, Dictionary<string, string> attributes, bool invokeSkipApi = true)
	{
		ReportFilterType type = ReportFilterType.Exception;
		if (exception is BacktraceUnhandledException)
		{
			BacktraceUnhandledException ex = exception as BacktraceUnhandledException;
			type = ((ex.Classifier == "ANRException") ? ReportFilterType.Hang : ((ex.Type == LogType.Exception) ? ReportFilterType.UnhandledException : ReportFilterType.Error));
		}
		if (invokeSkipApi && ShouldSkipReport(type, exception, string.Empty))
		{
			return false;
		}
		if (_reportLimitWatcher.WatchReport(DateTimeHelper.Timestamp()))
		{
			if (Thread.CurrentThread.ManagedThreadId != _current.ManagedThreadId)
			{
				BacktraceReport backtraceReport = new BacktraceReport(exception, attributes, attachmentPaths);
				backtraceReport.Attributes["exception.thread"] = Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
				BackgroundExceptions.Push(backtraceReport);
				return false;
			}
			return true;
		}
		if (OnClientReportLimitReached != null)
		{
			BacktraceReport obj = new BacktraceReport(exception, attributes, attachmentPaths);
			_onClientReportLimitReached(obj);
		}
		return false;
	}

	private bool ShouldSendReport(string message, List<string> attachmentPaths, Dictionary<string, string> attributes)
	{
		if (ShouldSkipReport(ReportFilterType.Message, null, message))
		{
			return false;
		}
		if (_reportLimitWatcher.WatchReport(DateTimeHelper.Timestamp()))
		{
			if (Thread.CurrentThread.ManagedThreadId != _current.ManagedThreadId)
			{
				BacktraceReport backtraceReport = new BacktraceReport(message, attributes, attachmentPaths);
				backtraceReport.Attributes["exception.thread"] = Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
				BackgroundExceptions.Push(backtraceReport);
				return false;
			}
			return true;
		}
		if (OnClientReportLimitReached != null)
		{
			BacktraceReport obj = new BacktraceReport(message, attributes, attachmentPaths);
			_onClientReportLimitReached(obj);
		}
		return false;
	}

	private bool ShouldSendReport(BacktraceReport report)
	{
		if (ShouldSkipReport((!report.ExceptionTypeReport) ? ReportFilterType.Message : ReportFilterType.Exception, report.Exception, report.Message))
		{
			return false;
		}
		if (_reportLimitWatcher.WatchReport(DateTimeHelper.Timestamp()))
		{
			if (Thread.CurrentThread.ManagedThreadId != _current.ManagedThreadId)
			{
				report.Attributes["exception.thread"] = Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture);
				BackgroundExceptions.Push(report);
				return false;
			}
			return true;
		}
		if (OnClientReportLimitReached != null)
		{
			_onClientReportLimitReached(report);
		}
		return false;
	}

	private void HandleInnerException(BacktraceReport report)
	{
		BacktraceReport backtraceReport = report.CreateInnerReport();
		if (backtraceReport != null && ShouldSendReport(backtraceReport))
		{
			SendReport(backtraceReport);
		}
	}

	private bool ValidClientConfiguration()
	{
		int num;
		if (BacktraceApi != null)
		{
			num = ((!Enabled) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0021;
			}
		}
		else
		{
			num = 1;
		}
		UnityEngine.Debug.LogWarning("Cannot set method if configuration contain invalid url to Backtrace server or client is disabled");
		goto IL_0021;
		IL_0021:
		return num == 0;
	}

	private bool ShouldSkipReport(ReportFilterType type, Exception exception, string message)
	{
		if (!Enabled)
		{
			return false;
		}
		if (!Configuration.ReportFilterType.HasFlag(type))
		{
			if (SkipReport != null)
			{
				return SkipReport(type, exception, message);
			}
			return false;
		}
		return true;
	}

	internal IList<string> GetNativeAttachments()
	{
		return _clientReportAttachments.Where((string n) => !string.IsNullOrEmpty(n)).OrderBy(Path.GetFileName, StringComparer.InvariantCultureIgnoreCase).ToList();
	}
}
