using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Backtrace.Unity.Common;
using Backtrace.Unity.Interfaces;
using Backtrace.Unity.Model;
using Backtrace.Unity.Model.Breadcrumbs;
using Backtrace.Unity.Model.Breadcrumbs.Storage;
using Backtrace.Unity.Model.Database;
using Backtrace.Unity.Runtime.Native;
using Backtrace.Unity.Services;
using Backtrace.Unity.Types;
using UnityEngine;

namespace Backtrace.Unity;

[RequireComponent(typeof(BacktraceClient))]
public class BacktraceDatabase : MonoBehaviour, IBacktraceDatabase
{
	private bool _timerBackgroundWork;

	public BacktraceConfiguration Configuration;

	private BacktraceBreadcrumbs _breadcrumbs;

	private BacktraceClient _client;

	internal static float LastFrameTime;

	private static BacktraceDatabase _instance;

	private float _lastConnection;

	private ReportLimitWatcher _reportLimitWatcher;

	public IBacktraceBreadcrumbs Breadcrumbs
	{
		get
		{
			if (_breadcrumbs == null && Enable && Configuration.EnableBreadcrumbsSupport && BacktraceBreadcrumbs.CanStoreBreadcrumbs(Configuration.LogLevel, Configuration.BacktraceBreadcrumbsLevel))
			{
				_breadcrumbs = new BacktraceBreadcrumbs(new BacktraceStorageLogManager(Configuration.GetFullDatabasePath()), Configuration.BacktraceBreadcrumbsLevel, Configuration.LogLevel);
			}
			return _breadcrumbs;
		}
	}

	public string DatabasePath { get; protected set; }

	public int ScreenshotQuality
	{
		get
		{
			return BacktraceDatabaseFileContext.ScreenshotQuality;
		}
		set
		{
			BacktraceDatabaseFileContext.ScreenshotQuality = value;
		}
	}

	public int ScreenshotMaxHeight
	{
		get
		{
			return BacktraceDatabaseFileContext.ScreenshotMaxHeight;
		}
		set
		{
			BacktraceDatabaseFileContext.ScreenshotMaxHeight = value;
		}
	}

	public static BacktraceDatabase Instance => _instance;

	public DeduplicationStrategy DeduplicationStrategy
	{
		get
		{
			if (BacktraceDatabaseContext == null)
			{
				return DeduplicationStrategy.None;
			}
			return BacktraceDatabaseContext.DeduplicationStrategy;
		}
		set
		{
			if (!Enable)
			{
				throw new InvalidOperationException("Backtrace Database is disabled");
			}
			BacktraceDatabaseContext.DeduplicationStrategy = value;
		}
	}

	protected BacktraceDatabaseSettings DatabaseSettings { get; set; }

	public IBacktraceApi BacktraceApi { get; set; }

	protected virtual IBacktraceDatabaseContext BacktraceDatabaseContext { get; set; }

	internal IBacktraceDatabaseFileContext BacktraceDatabaseFileContext { get; set; }

	public bool Enable { get; private set; }

	public void Reload()
	{
		if (Configuration == null)
		{
			_client = GetComponent<BacktraceClient>();
			Configuration = _client.Configuration;
		}
		if (Instance != null)
		{
			return;
		}
		if (Configuration == null || !Configuration.IsValid())
		{
			Enable = false;
			return;
		}
		Enable = Configuration.Enabled && InitializeDatabasePaths();
		if (!Enable)
		{
			if (Configuration.Enabled)
			{
				UnityEngine.Debug.LogWarning("Cannot initialize database - invalid path to database. Database is disabled");
			}
			return;
		}
		DatabaseSettings = new BacktraceDatabaseSettings(DatabasePath, Configuration);
		SetupMultisceneSupport();
		_lastConnection = Time.unscaledTime;
		LastFrameTime = Time.unscaledTime;
		BacktraceDatabaseContext = new BacktraceDatabaseContext(DatabaseSettings);
		BacktraceDatabaseFileContext = new BacktraceDatabaseFileContext(DatabaseSettings);
		BacktraceApi = new BacktraceApi(Configuration.ToCredentials());
		_reportLimitWatcher = new ReportLimitWatcher(Convert.ToUInt32(Configuration.ReportPerMin));
	}

	public void OnDisable()
	{
		Enable = false;
	}

	private void Awake()
	{
		Reload();
	}

	internal void Update()
	{
		if (!Enable)
		{
			return;
		}
		if (_breadcrumbs != null)
		{
			_breadcrumbs.Update();
		}
		LastFrameTime = Time.unscaledTime;
		if (DatabaseSettings.AutoSendMode && Time.unscaledTime - _lastConnection > (float)DatabaseSettings.RetryInterval)
		{
			_lastConnection = Time.unscaledTime;
			if (!_timerBackgroundWork && BacktraceDatabaseContext.Any())
			{
				_timerBackgroundWork = true;
				SendData(BacktraceDatabaseContext.FirstOrDefault());
				_timerBackgroundWork = false;
			}
		}
	}

	private void Start()
	{
		if (!Enable)
		{
			return;
		}
		string breadcrumbPath = string.Empty;
		string text = string.Empty;
		if (Breadcrumbs != null)
		{
			breadcrumbPath = Breadcrumbs.GetBreadcrumbLogPath();
			text = Breadcrumbs.Archive();
		}
		LoadReports(breadcrumbPath, text);
		RemoveOrphaned();
		bool num = Enable && Configuration.SendUnhandledGameCrashesOnGameStartup && base.isActiveAndEnabled;
		bool flag = (bool)_client && _client.NativeClient != null && _client.NativeClient is IStartupMinidumpSender;
		if (num && flag)
		{
			IStartupMinidumpSender startupMinidumpSender = _client.NativeClient as IStartupMinidumpSender;
			IList<string> nativeAttachments = _client.GetNativeAttachments();
			if (!string.IsNullOrEmpty(text))
			{
				nativeAttachments.Add(text);
			}
			StartCoroutine(startupMinidumpSender.SendMinidumpOnStartup(nativeAttachments, BacktraceApi));
		}
		EnableBreadcrumbsSupport();
		if (DatabaseSettings.AutoSendMode)
		{
			_lastConnection = Time.unscaledTime;
			SendData(BacktraceDatabaseContext.FirstOrDefault());
		}
	}

	public void SetApi(IBacktraceApi backtraceApi)
	{
		BacktraceApi = backtraceApi;
	}

	public bool Enabled()
	{
		return Enable;
	}

	public BacktraceDatabaseSettings GetSettings()
	{
		return DatabaseSettings;
	}

	public void Clear()
	{
		if (BacktraceDatabaseContext != null)
		{
			BacktraceDatabaseContext.Clear();
		}
		if (BacktraceDatabaseContext != null)
		{
			BacktraceDatabaseFileContext.Clear();
		}
	}

	public BacktraceDatabaseRecord Add(BacktraceData data, bool @lock = true)
	{
		if (data == null || !Enable)
		{
			return null;
		}
		if (!ValidateDatabaseSize())
		{
			return null;
		}
		string hash = BacktraceDatabaseContext.GetHash(data);
		if (!string.IsNullOrEmpty(hash))
		{
			BacktraceDatabaseRecord recordByHash = BacktraceDatabaseContext.GetRecordByHash(hash);
			if (recordByHash != null)
			{
				BacktraceDatabaseContext.AddDuplicate(recordByHash);
				return recordByHash;
			}
		}
		IEnumerable<string> source = BacktraceDatabaseFileContext.GenerateRecordAttachments(data);
		for (int i = 0; i < source.Count(); i++)
		{
			if (!string.IsNullOrEmpty(source.ElementAt(i)))
			{
				data.Attachments.Add(source.ElementAt(i));
			}
		}
		if (Breadcrumbs != null)
		{
			data.Attachments.Add(Breadcrumbs.GetBreadcrumbLogPath());
			data.Attributes.Attributes["breadcrumbs.lastId"] = Breadcrumbs.BreadcrumbId().ToString("F0", CultureInfo.InvariantCulture);
		}
		BacktraceDatabaseRecord backtraceDatabaseRecord = new BacktraceDatabaseRecord(data)
		{
			Hash = hash
		};
		if (!BacktraceDatabaseFileContext.Save(backtraceDatabaseRecord))
		{
			BacktraceDatabaseFileContext.Delete(backtraceDatabaseRecord);
			return null;
		}
		BacktraceDatabaseContext.Add(backtraceDatabaseRecord);
		if (!@lock)
		{
			backtraceDatabaseRecord.Unlock();
		}
		return backtraceDatabaseRecord;
	}

	[Obsolete("Please use Add method with Backtrace data parameter instead")]
	public BacktraceDatabaseRecord Add(BacktraceReport backtraceReport, Dictionary<string, string> attributes, MiniDumpType miniDumpType = MiniDumpType.None)
	{
		if (!Enable || backtraceReport == null)
		{
			return null;
		}
		BacktraceData data = backtraceReport.ToBacktraceData(attributes, Configuration.GameObjectDepth);
		return Add(data);
	}

	public IEnumerable<BacktraceDatabaseRecord> Get()
	{
		if (BacktraceDatabaseContext != null)
		{
			return BacktraceDatabaseContext.Get();
		}
		return new List<BacktraceDatabaseRecord>();
	}

	public void Delete(BacktraceDatabaseRecord record)
	{
		if (BacktraceDatabaseContext != null)
		{
			BacktraceDatabaseContext.Delete(record);
		}
		if (BacktraceDatabaseFileContext != null)
		{
			BacktraceDatabaseFileContext.Delete(record);
		}
	}

	public void Flush()
	{
		if (Enable && BacktraceDatabaseContext.Any())
		{
			FlushRecord(BacktraceDatabaseContext.FirstOrDefault());
		}
	}

	public void Send()
	{
		if (Enable && BacktraceDatabaseContext.Any())
		{
			SendData(BacktraceDatabaseContext.FirstOrDefault());
		}
	}

	private void FlushRecord(BacktraceDatabaseRecord record)
	{
		if (record == null)
		{
			return;
		}
		Stopwatch stopwatch = (Configuration.PerformanceStatistics ? Stopwatch.StartNew() : null);
		string text = record.BacktraceDataJson();
		Delete(record);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (Configuration.PerformanceStatistics)
		{
			stopwatch.Stop();
			dictionary["performance.database.flush"] = stopwatch.GetMicroseconds();
		}
		if (text != null)
		{
			dictionary["_mod_duplicate"] = record.Count.ToString(CultureInfo.InvariantCulture);
			StartCoroutine(BacktraceApi.Send(text, record.Attachments, dictionary, delegate
			{
				record = BacktraceDatabaseContext.FirstOrDefault();
				FlushRecord(record);
			}));
		}
	}

	private void SendData(BacktraceDatabaseRecord record)
	{
		if (record == null)
		{
			return;
		}
		Stopwatch stopwatch = (Configuration.PerformanceStatistics ? Stopwatch.StartNew() : null);
		string text = ((record != null) ? record.BacktraceDataJson() : null);
		if (string.IsNullOrEmpty(text))
		{
			Delete(record);
			return;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (Configuration.PerformanceStatistics)
		{
			stopwatch.Stop();
			dictionary["performance.database.send"] = stopwatch.GetMicroseconds();
		}
		dictionary["_mod_duplicate"] = record.Count.ToString(CultureInfo.InvariantCulture);
		StartCoroutine(BacktraceApi.Send(text, record.Attachments, dictionary, delegate(BacktraceResult sendResult)
		{
			record.Unlock();
			if (sendResult.Status != BacktraceResultStatus.ServerError && sendResult.Status != BacktraceResultStatus.NetworkError)
			{
				Delete(record);
				if (_reportLimitWatcher.WatchReport(DateTimeHelper.Timestamp()))
				{
					record = BacktraceDatabaseContext.FirstOrDefault();
					SendData(record);
				}
			}
			else
			{
				IncrementBatchRetry();
			}
		}));
	}

	public int Count()
	{
		return BacktraceDatabaseContext.Count();
	}

	protected virtual void RemoveOrphaned()
	{
		IEnumerable<BacktraceDatabaseRecord> existingRecords = BacktraceDatabaseContext.Get();
		BacktraceDatabaseFileContext.RemoveOrphaned(existingRecords);
	}

	protected virtual void SetupMultisceneSupport()
	{
		if (!Configuration.DestroyOnLoad)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			_instance = this;
		}
	}

	protected virtual bool InitializeDatabasePaths()
	{
		if (!Configuration.Enabled)
		{
			return false;
		}
		DatabasePath = Configuration.GetFullDatabasePath();
		if (string.IsNullOrEmpty(DatabasePath))
		{
			UnityEngine.Debug.LogWarning("Backtrace database path is empty or unavailable.");
			return false;
		}
		bool flag = Directory.Exists(DatabasePath);
		if (!flag && Configuration.CreateDatabase)
		{
			try
			{
				flag = Directory.CreateDirectory(DatabasePath).Exists;
			}
			catch (Exception)
			{
				return false;
			}
		}
		if (!flag)
		{
			UnityEngine.Debug.LogWarning($"Backtrace database path doesn't exist. Database path: {DatabasePath}");
		}
		return flag;
	}

	protected virtual void LoadReports(string breadcrumbPath, string breadcrumbArchive)
	{
		if (!Enable)
		{
			return;
		}
		FileInfo[] array = BacktraceDatabaseFileContext.GetRecords().ToArray();
		if (array.Length == 0)
		{
			return;
		}
		bool flag = !string.IsNullOrEmpty(breadcrumbArchive);
		FileInfo[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			BacktraceDatabaseRecord backtraceDatabaseRecord = BacktraceDatabaseRecord.ReadFromFile(array2[i]);
			if (backtraceDatabaseRecord == null)
			{
				continue;
			}
			if (!BacktraceDatabaseFileContext.IsValidRecord(backtraceDatabaseRecord))
			{
				BacktraceDatabaseFileContext.Delete(backtraceDatabaseRecord);
				continue;
			}
			if (flag && backtraceDatabaseRecord.Attachments.Remove(breadcrumbPath))
			{
				backtraceDatabaseRecord.Attachments.Add(breadcrumbArchive);
			}
			BacktraceDatabaseContext.Add(backtraceDatabaseRecord);
			ValidateDatabaseSize();
			backtraceDatabaseRecord.Unlock();
		}
	}

	private bool ValidateDatabaseSize()
	{
		bool num = ReachedMaximumNumberOfRecords();
		bool flag = ReachedDiskSpaceLimit();
		if (num || flag)
		{
			int num2 = 5;
			while (ReachedDiskSpaceLimit() || ReachedMaximumNumberOfRecords())
			{
				BacktraceDatabaseRecord backtraceDatabaseRecord = BacktraceDatabaseContext.LastOrDefault();
				if (backtraceDatabaseRecord != null)
				{
					BacktraceDatabaseContext.Delete(backtraceDatabaseRecord);
					BacktraceDatabaseFileContext.Delete(backtraceDatabaseRecord);
				}
				num2--;
				if (num2 == 0)
				{
					break;
				}
			}
			return num2 != 0;
		}
		return true;
	}

	private bool ReachedDiskSpaceLimit()
	{
		if (DatabaseSettings.MaxDatabaseSize != 0L)
		{
			return BacktraceDatabaseContext.GetSize() > DatabaseSettings.MaxDatabaseSize;
		}
		return false;
	}

	private bool ReachedMaximumNumberOfRecords()
	{
		if (BacktraceDatabaseContext.Count() + 1 > DatabaseSettings.MaxRecordCount)
		{
			return DatabaseSettings.MaxRecordCount != 0;
		}
		return false;
	}

	public bool ValidConsistency()
	{
		return BacktraceDatabaseFileContext.ValidFileConsistency();
	}

	public long GetDatabaseSize()
	{
		return BacktraceDatabaseContext.GetSize();
	}

	public void SetReportWatcher(ReportLimitWatcher reportLimitWatcher)
	{
		_reportLimitWatcher = reportLimitWatcher;
	}

	private void IncrementBatchRetry()
	{
		IEnumerable<BacktraceDatabaseRecord> recordsToDelete = BacktraceDatabaseContext.GetRecordsToDelete();
		BacktraceDatabaseContext.IncrementBatchRetry();
		if (recordsToDelete == null || recordsToDelete.Count() == 0)
		{
			return;
		}
		foreach (BacktraceDatabaseRecord item in recordsToDelete)
		{
			BacktraceDatabaseFileContext.Delete(item);
		}
	}

	internal string GetBreadcrumbsPath()
	{
		if (_breadcrumbs == null)
		{
			return string.Empty;
		}
		return _breadcrumbs.GetBreadcrumbLogPath();
	}

	public bool EnableBreadcrumbsSupport()
	{
		if (Breadcrumbs == null)
		{
			return false;
		}
		return _breadcrumbs.EnableBreadcrumbs();
	}
}
