using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Backtrace.Unity.Interfaces;
using Backtrace.Unity.Model;
using Backtrace.Unity.Model.Attributes;
using Backtrace.Unity.Model.Breadcrumbs;
using Backtrace.Unity.Runtime.Native.Base;
using Backtrace.Unity.Types;
using UnityEngine;

namespace Backtrace.Unity.Runtime.Native.Windows;

internal sealed class NativeClient : NativeClientBase, INativeClient, IDynamicAttributeProvider, IStartupMinidumpSender
{
	[Serializable]
	private class ScopedAttributesContainer
	{
		public List<string> Keys = new List<string>();
	}

	private const string ScopedAttributeListKey = "backtrace-scoped-attributes";

	private const string ScopedAttributesPattern = "bt-{0}";

	internal const string VersionKey = "backtrace-app-version";

	internal const string MachineUuidKey = "backtrace-uuid";

	internal const string SessionKey = "backtrace-session-id";

	[DllImport("BacktraceCrashpadWindows")]
	private static extern bool Initialize(string submissionUrl, [MarshalAs(UnmanagedType.LPWStr)] string databasePath, [MarshalAs(UnmanagedType.LPWStr)] string handlerPath, string[] attachments, int attachmentSize);

	[DllImport("BacktraceCrashpadWindows", EntryPoint = "AddAttribute")]
	private static extern bool AddNativeAttribute(string key, string value);

	[DllImport("BacktraceCrashpadWindows", EntryPoint = "DumpWithoutCrash")]
	private static extern void NativeReport(string message, bool setMainThreadAsFaultingThread);

	public NativeClient(BacktraceConfiguration configuration, BacktraceBreadcrumbs breadcrumbs, IDictionary<string, string> clientAttributes, IEnumerable<string> attachments)
		: base(configuration, breadcrumbs)
	{
		CleanScopedAttributes();
		HandleNativeCrashes(clientAttributes, attachments);
		AddScopedAttributes(clientAttributes);
		if (!configuration.ReportFilterType.HasFlag(ReportFilterType.Hang))
		{
			HandleAnr();
		}
	}

	private void HandleNativeCrashes(IDictionary<string, string> clientAttributes, IEnumerable<string> attachments)
	{
		if (!_configuration.CaptureNativeCrashes || !_configuration.Enabled)
		{
			return;
		}
		string pluginDirectoryPath = GetPluginDirectoryPath();
		if (!Directory.Exists(pluginDirectoryPath))
		{
			Debug.LogWarning("Backtrace native lib directory doesn't exist");
		}
		else
		{
			if (Isx86Build(pluginDirectoryPath) || IntPtr.Size == 4)
			{
				return;
			}
			string defaultPathToCrashpadHandler = GetDefaultPathToCrashpadHandler(pluginDirectoryPath);
			if (string.IsNullOrEmpty(defaultPathToCrashpadHandler) || !File.Exists(defaultPathToCrashpadHandler))
			{
				Debug.LogWarning("Backtrace native integration status: Cannot find path to Crashpad handler.");
				return;
			}
			string crashpadDatabasePath = _configuration.CrashpadDatabasePath;
			if (string.IsNullOrEmpty(crashpadDatabasePath) || !Directory.Exists(_configuration.GetFullDatabasePath()))
			{
				Debug.LogWarning("Backtrace native integration status: database path doesn't exist");
				return;
			}
			string submissionUrl = new BacktraceCredentials(_configuration.GetValidServerUrl()).GetMinidumpSubmissionUrl().ToString();
			if (!Directory.Exists(crashpadDatabasePath))
			{
				Directory.CreateDirectory(crashpadDatabasePath);
			}
			CaptureNativeCrashes = Initialize(submissionUrl, crashpadDatabasePath, defaultPathToCrashpadHandler, attachments.ToArray(), attachments.Count());
			if (!CaptureNativeCrashes)
			{
				Debug.LogWarning("Backtrace native integration status: Cannot initialize Crashpad client");
				return;
			}
			foreach (KeyValuePair<string, string> clientAttribute in clientAttributes)
			{
				AddNativeAttribute(clientAttribute.Key, (clientAttribute.Value == null) ? string.Empty : clientAttribute.Value);
			}
			AddNativeAttribute("error.type", "Crash");
		}
	}

	private bool Isx86Build(string pluginDirectoryPath)
	{
		return File.Exists(Path.Combine(Path.Combine(pluginDirectoryPath, "x86"), "BacktraceCrashpadWindows.dll"));
	}

	public void GetAttributes(IDictionary<string, string> attributes)
	{
	}

	public void HandleAnr()
	{
		if (!CaptureNativeCrashes || !_configuration.HandleANR)
		{
			return;
		}
		bool reported = false;
		_ = Thread.CurrentThread.ManagedThreadId;
		AnrThread = new Thread((ThreadStart)delegate
		{
			float num = 0f;
			while (AnrThread.IsAlive && !StopAnr)
			{
				if (!PreventAnr)
				{
					if (num == 0f)
					{
						num = LastUpdateTime;
					}
					else if (num == LastUpdateTime)
					{
						if (!reported)
						{
							OnAnrDetection();
							reported = true;
							AddNativeAttribute("error.type", "Hang");
							NativeReport("ANRException: Blocked thread detected.", setMainThreadAsFaultingThread: true);
							AddNativeAttribute("error.type", "Hang");
						}
					}
					else
					{
						reported = false;
					}
					num = LastUpdateTime;
				}
				else if (num != 0f)
				{
					num = 0f;
				}
				Thread.Sleep(AnrWatchdogTimeout);
			}
		});
		AnrThread.IsBackground = true;
		AnrThread.Start();
	}

	public bool OnOOM()
	{
		return false;
	}

	public void SetAttribute(string key, string value)
	{
		if (!string.IsNullOrEmpty(key))
		{
			if (value == null)
			{
				value = string.Empty;
			}
			AddAttributes(key, value);
		}
	}

	public IEnumerator SendMinidumpOnStartup(ICollection<string> clientAttachments, IBacktraceApi backtraceApi)
	{
		string path = $"Temp/{Application.companyName}/{Application.productName}/crashes";
		string[] obj = new string[2]
		{
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path),
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path)
		};
		List<string> list = new List<string>();
		string[] array = obj;
		foreach (string text in array)
		{
			if (Directory.Exists(text))
			{
				list.Add(text);
			}
		}
		if (list.Count == 0)
		{
			yield break;
		}
		IDictionary<string, string> attributes = GetScopedAttributes();
		attributes["error.type"] = "Crash";
		foreach (string nativeCrashesDir in list)
		{
			List<string> attachments = ((clientAttachments == null) ? new List<string>() : new List<string>(clientAttachments));
			string[] directories = Directory.GetDirectories(nativeCrashesDir);
			string[] array2 = directories;
			foreach (string path2 in array2)
			{
				string crashDirFullPath = Path.Combine(nativeCrashesDir, path2);
				string[] files = Directory.GetFiles(crashDirFullPath);
				if (files.Any((string n) => n.EndsWith("backtrace.json")))
				{
					continue;
				}
				string minidumpPath = files.FirstOrDefault((string n) => n.EndsWith("crash.dmp"));
				if (string.IsNullOrEmpty(minidumpPath))
				{
					continue;
				}
				List<string> attachments2 = (from n in files.Concat(attachments)
					where n != minidumpPath
					select n).ToList();
				yield return backtraceApi.SendMinidump(minidumpPath, attachments2, attributes, delegate(BacktraceResult result)
				{
					if (result != null && result.Status == BacktraceResultStatus.Ok)
					{
						File.Create(Path.Combine(crashDirFullPath, "backtrace.json"));
					}
				});
			}
		}
	}

	private string GetPluginDirectoryPath()
	{
		return Path.Combine(Application.dataPath, "Plugins");
	}

	private string GetDefaultPathToCrashpadHandler(string pluginDirectoryPath)
	{
		return Path.Combine(Path.Combine(pluginDirectoryPath, "x86_64"), "crashpad_handler.dll");
	}

	internal static void CleanScopedAttributes()
	{
		string @string = PlayerPrefs.GetString("backtrace-scoped-attributes");
		if (!HasScopedAttributesEmpty(@string))
		{
			return;
		}
		foreach (string key in JsonUtility.FromJson<ScopedAttributesContainer>(@string).Keys)
		{
			PlayerPrefs.DeleteKey($"bt-{key}");
		}
		PlayerPrefs.DeleteKey("backtrace-scoped-attributes");
	}

	internal static IDictionary<string, string> GetScopedAttributes()
	{
		string @string = PlayerPrefs.GetString("backtrace-scoped-attributes");
		if (!HasScopedAttributesEmpty(@string))
		{
			return new Dictionary<string, string>();
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string key in JsonUtility.FromJson<ScopedAttributesContainer>(@string).Keys)
		{
			string string2 = PlayerPrefs.GetString($"bt-{key}", string.Empty);
			dictionary[key] = string2;
		}
		foreach (KeyValuePair<string, string> item in (IEnumerable<KeyValuePair<string, string>>)new Dictionary<string, string>
		{
			{ "backtrace-uuid", "guid" },
			{ "backtrace-app-version", "application.version" },
			{ "backtrace-session-id", "application.session" }
		})
		{
			string string3 = PlayerPrefs.GetString(item.Key, string.Empty);
			if (!string.IsNullOrEmpty(string3))
			{
				PlayerPrefs.DeleteKey(item.Key);
				dictionary[item.Value] = string3;
			}
		}
		return dictionary;
	}

	private void AddAttributes(string key, string value)
	{
		if (CaptureNativeCrashes)
		{
			AddNativeAttribute(key, value);
		}
		AddScopedAttribute(key, value);
	}

	internal void AddScopedAttributes(IDictionary<string, string> attributes)
	{
		if (!_configuration.SendUnhandledGameCrashesOnGameStartup)
		{
			return;
		}
		ScopedAttributesContainer scopedAttributesContainer = new ScopedAttributesContainer();
		foreach (KeyValuePair<string, string> attribute in attributes)
		{
			scopedAttributesContainer.Keys.Add(attribute.Key);
			PlayerPrefs.SetString($"bt-{attribute.Key}", attribute.Value);
		}
		PlayerPrefs.SetString("backtrace-scoped-attributes", JsonUtility.ToJson(scopedAttributesContainer));
	}

	private void AddScopedAttribute(string key, string value)
	{
		if (_configuration.SendUnhandledGameCrashesOnGameStartup)
		{
			string @string = PlayerPrefs.GetString("backtrace-scoped-attributes");
			ScopedAttributesContainer scopedAttributesContainer = (HasScopedAttributesEmpty(@string) ? JsonUtility.FromJson<ScopedAttributesContainer>(@string) : new ScopedAttributesContainer());
			scopedAttributesContainer.Keys.Add(key);
			PlayerPrefs.SetString("backtrace-scoped-attributes", JsonUtility.ToJson(scopedAttributesContainer));
			PlayerPrefs.SetString($"bt-{key}", value);
		}
	}

	private static bool HasScopedAttributesEmpty(string attributesJson)
	{
		if (!string.IsNullOrEmpty(attributesJson))
		{
			return !(attributesJson == "{}");
		}
		return false;
	}
}
