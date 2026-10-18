using System;
using System.Collections.Generic;
using System.IO;
using Backtrace.Unity.Common;
using Backtrace.Unity.Model.Breadcrumbs;
using Backtrace.Unity.Types;
using UnityEngine;

namespace Backtrace.Unity.Model;

[Serializable]
[CreateAssetMenu(fileName = "Backtrace Configuration", menuName = "Backtrace/Configuration", order = 0)]
public class BacktraceConfiguration : ScriptableObject
{
	private const BacktraceBreadcrumbType AllBreadcrumbsTypes = BacktraceBreadcrumbType.Manual | BacktraceBreadcrumbType.Log | BacktraceBreadcrumbType.Navigation | BacktraceBreadcrumbType.Http | BacktraceBreadcrumbType.System | BacktraceBreadcrumbType.User | BacktraceBreadcrumbType.Configuration;

	private const UnityEngineLogLevel AllLogTypes = UnityEngineLogLevel.Debug | UnityEngineLogLevel.Warning | UnityEngineLogLevel.Info | UnityEngineLogLevel.Fatal | UnityEngineLogLevel.Error;

	public const int DefaultAnrWatchdogTimeout = 5000;

	public const int DefaultRetryLimit = 3;

	public const int DefaultReportPerMin = 50;

	public const int DefaultGameObjectDepth = -1;

	public const int DefaultNumberOfLogs = 10;

	public const int DefaultMaxRecordCount = 8;

	public const int DefaultMaxDatabaseSize = 0;

	public const int DefaultRetryInterval = 60;

	[Header("Backtrace client configuration")]
	[Tooltip("This field is required to submit exceptions from your Unity project to your Backtrace instance.\n \nMore information about how to retrieve this value for your instance is our docs at What is a submission URL and What is a submission token?\n\nNOTE: the backtrace-unity plugin will expect full URL with token to your Backtrace instance.")]
	public string ServerUrl;

	public string Token;

	[Tooltip("Reports per minute: Limits the number of reports the client will send per minutes. If set to 0, there is no limit. If set to a higher value and the value is reached, the client will not send any reports until the next minute. Default: 50")]
	public int ReportPerMin = 50;

	[Tooltip("Disable error reporting integration in editor mode.")]
	public bool DisableInEditor;

	[Tooltip("Toggle this on or off to set the library to handle unhandled exceptions that are not captured by try-catch blocks.")]
	public bool HandleUnhandledExceptions = true;

	[Tooltip("Unity by default will validate ssl certificates. By using this option you can avoid ssl certificates validation. However, if you don't need to ignore ssl validation, please set this option to false.")]
	public bool IgnoreSslValidation;

	[Tooltip("Backtrace-client by default will be available on each scene. Once you initialize Backtrace integration, you can fetch Backtrace game object from every scene. In case if you don't want to have Backtrace-unity integration available by default in each scene, please set this value to true.")]
	public bool DestroyOnLoad;

	[Tooltip("Log random sampling rate - Enables a random sampling mechanism for Unity.Error logs - by default sampling is equal to 0.01 - which means only 1% of randomply sampling reports will be send to Backtrace. \n* 1 - means 100% of error reports will be reported by library,\n* 0.1 - means 10% of error reports will be reported by library,\n* 0 - means library is going to drop all errors.")]
	[Range(0f, 1f)]
	public double Sampling = 0.01;

	[Tooltip("Report filter allows to filter specific type of reports. Possible options:\n* Disable - Disable report filtering - send every type of report.\n* Message - Prevent message reports.\n* Exception - Prevent exception reports.\n* Unhandled exception- Prevent unhandled exception reports.\n* Hang - Prevent sending reports when game hang.\n* Error log - Prevent sending error logs.")]
	public ReportFilterType ReportFilterType;

	[Tooltip("Allows developer to filter number of game object childrens in Backtrace report.")]
	public int GameObjectDepth = -1;

	[Tooltip("Number of logs collected by Backtrace-Unity")]
	public uint NumberOfLogs = 10u;

	[Tooltip("Enable performance statistics")]
	public bool PerformanceStatistics;

	[Tooltip("Try to find game native crashes and send them on Game startup")]
	public bool SendUnhandledGameCrashesOnGameStartup = true;

	[Tooltip("Capture native Crashes")]
	public bool CaptureNativeCrashes = true;

	[Tooltip("Capture ANR events - Application not responding")]
	public bool HandleANR = true;

	[Tooltip("ANR watchdog timeout")]
	public int AnrWatchdogTimeout = 5000;

	[Obsolete("Not supported")]
	public bool OomReports;

	[Obsolete("Not supported")]
	public bool ClientSideUnwinding;

	[Obsolete("Not supported")]
	public string SymbolsUploadToken = string.Empty;

	[Tooltip("Client-side deduplication allows the backtrace-unity library to group multiple error reports into a single one based on various factors. Factors include:\n\n* Disable - Client-side deduplication rules are disabled.\n* Everything - Use all the options as a factor in client-side deduplication.\n* Faulting callstack - Use the faulting callstack as a factor in client-side deduplication.\n* Exception type - Use the exception type as a factor in client-side deduplication.\n* Exception message - Use the exception message as a factor in client-side deduplication.")]
	public DeduplicationStrategy DeduplicationStrategy;

	[Tooltip("Enable breadcurmbs integration that will include game breadcrumbs in each report (native + managed).")]
	public bool EnableBreadcrumbsSupport;

	[Tooltip("Breadcrumbs support breadcrumbs level- Backtrace breadcrumbs log level controls what type of information will be available in the breadcrumb file")]
	public BacktraceBreadcrumbType BacktraceBreadcrumbsLevel = BacktraceBreadcrumbType.Manual | BacktraceBreadcrumbType.Log | BacktraceBreadcrumbType.Navigation | BacktraceBreadcrumbType.Http | BacktraceBreadcrumbType.System | BacktraceBreadcrumbType.User | BacktraceBreadcrumbType.Configuration;

	[Tooltip("Breadcrumbs log level")]
	public UnityEngineLogLevel LogLevel = UnityEngineLogLevel.Debug | UnityEngineLogLevel.Warning | UnityEngineLogLevel.Info | UnityEngineLogLevel.Fatal | UnityEngineLogLevel.Error;

	[Tooltip("If exception does not have a stack trace, use a normalized exception message to generate fingerprint.")]
	public bool UseNormalizedExceptionMessage;

	[Tooltip("Type of minidump that will be attached to Backtrace report in the report generated on Windows machine.")]
	public MiniDumpType MinidumpType = MiniDumpType.None;

	[Tooltip("Generate and attach screenshot of frame as exception occurs")]
	public bool GenerateScreenshotOnException;

	[Tooltip("List of path to attachments that Backtrace client will include in the native and managed reports.")]
	public string[] AttachmentPaths;

	[Tooltip("This is the path to directory where the Backtrace database will store reports on your game. NOTE: Backtrace database will remove all existing files on database start.")]
	public string DatabasePath = "${Application.persistentDataPath}/backtrace";

	[Tooltip("This toggles the periodic (default: every 30 minutes) transmission of session information to the Backtrace endpoints. This will enable metrics such as crash free users and crash free sessions.")]
	public bool EnableMetricsSupport = true;

	[Range(0f, 60f)]
	[Tooltip("How often events should be sent to the Backtrace endpoints, in minutes. Zero (0) disables auto send and will require manual periodic sending using the API. For more information, see the README.")]
	public uint TimeIntervalInMin = 30u;

	[Header("Backtrace database configuration")]
	[Tooltip("When this setting is toggled, the backtrace-unity plugin will configure an offline database that will store reports if they can't be submitted do to being offline or not finding a network. When toggled on, there are a number of Database settings to configure.")]
	public bool Enabled;

	[Tooltip("Add Unity player log file to Backtrace report")]
	public bool AddUnityLogToReport;

	[Tooltip("When toggled on, the database will send automatically reports to Backtrace server based on the Retry Settings below. When toggled off, the developer will need to use the Flush method to attempt to send and clear. Recommend that this is toggled on.")]
	public bool AutoSendMode = true;

	[Tooltip("If toggled, the library will create the offline database directory if the provided path doesn't exists.")]
	public bool CreateDatabase = true;

	[Tooltip("This is one of two limits you can impose for controlling the growth of the offline store. This setting is the maximum number of stored reports in database. If value is equal to zero, then limit not exists, When the limit is reached, the database will remove the oldest entries.")]
	public int MaxRecordCount = 8;

	[Tooltip("This is the second limit you can impose for controlling the growth of the offline store. This setting is the maximum database size in MB. If value is equal to zero, then size is unlimited, When the limit is reached, the database will remove the oldest entries.")]
	public long MaxDatabaseSize;

	[Tooltip("If the database is unable to send its record, this setting specifies how many seconds the library should wait between retries.")]
	public int RetryInterval = 60;

	[Tooltip("If the database is unable to send its record, this setting specifies the maximum number of retries before the system gives up.")]
	public int RetryLimit = 3;

	[Tooltip("This specifies in which order records are sent to the Backtrace server.")]
	public RetryOrder RetryOrder;

	public string CrashpadDatabasePath
	{
		get
		{
			if (!Enabled)
			{
				return string.Empty;
			}
			return Path.Combine(GetFullDatabasePath(), "crashpad");
		}
	}

	public HashSet<string> GetAttachmentPaths()
	{
		HashSet<string> hashSet = new HashSet<string>();
		if (AttachmentPaths == null || AttachmentPaths.Length == 0)
		{
			return hashSet;
		}
		string[] attachmentPaths = AttachmentPaths;
		foreach (string text in attachmentPaths)
		{
			if (!string.IsNullOrEmpty(text))
			{
				hashSet.Add(ClientPathHelper.GetFullPath(text));
			}
		}
		return hashSet;
	}

	public string GetUniverseName()
	{
		string validServerUrl = GetValidServerUrl();
		if (validServerUrl.StartsWith("https://submit.backtrace.io/"))
		{
			int length = "https://submit.backtrace.io/".Length;
			int num = validServerUrl.IndexOf('/', length);
			if (num == -1)
			{
				throw new ArgumentException("Invalid Backtrace URL");
			}
			return validServerUrl.Substring(length, num - length);
		}
		if (validServerUrl.IndexOf("backtrace.io") == -1)
		{
			return null;
		}
		UriBuilder uriBuilder = new UriBuilder(validServerUrl);
		return uriBuilder.Host.Substring(0, uriBuilder.Host.IndexOf("."));
	}

	public string GetToken()
	{
		string validServerUrl = GetValidServerUrl();
		if (!validServerUrl.Contains("submit.backtrace.io"))
		{
			return validServerUrl.Substring(validServerUrl.IndexOf("token=") + "token=".Length, 64);
		}
		return validServerUrl.Substring(validServerUrl.LastIndexOf("/") - 64, 64);
	}

	public string GetFullDatabasePath()
	{
		return ClientPathHelper.GetFullPath(DatabasePath);
	}

	public string GetValidServerUrl()
	{
		return UpdateServerUrl(ServerUrl);
	}

	public static string UpdateServerUrl(string value)
	{
		string result = value;
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (!value.StartsWith("http"))
		{
			value = $"https://{value}";
		}
		string scheme = (value.StartsWith("https://") ? Uri.UriSchemeHttps : Uri.UriSchemeHttp);
		if (!Uri.IsWellFormedUriString(value, UriKind.Absolute))
		{
			return result;
		}
		return new UriBuilder(value)
		{
			Scheme = scheme
		}.Uri.ToString();
	}

	public static bool ValidateServerUrl(string value)
	{
		return Uri.IsWellFormedUriString(UpdateServerUrl(value), UriKind.Absolute);
	}

	public bool IsValid()
	{
		return ValidateServerUrl(ServerUrl);
	}

	public uint GetEventAggregationIntervalTimerInMs()
	{
		return TimeIntervalInMin * 60;
	}

	public BacktraceCredentials ToCredentials()
	{
		return new BacktraceCredentials(ServerUrl);
	}
}
