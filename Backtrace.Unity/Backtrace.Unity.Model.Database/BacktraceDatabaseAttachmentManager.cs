using System;
using System.Collections.Generic;
using System.IO;
using Backtrace.Unity.Common;
using Backtrace.Unity.Types;
using UnityEngine;

namespace Backtrace.Unity.Model.Database;

internal class BacktraceDatabaseAttachmentManager
{
	private readonly BacktraceDatabaseSettings _settings;

	private float _lastScreenTime;

	private string _lastScreenPath;

	private readonly object _lock = new object();

	internal int ScreenshotMaxHeight { get; set; }

	internal int ScreenshotQuality { get; set; }

	public BacktraceDatabaseAttachmentManager(BacktraceDatabaseSettings settings)
	{
		_settings = settings;
		ScreenshotMaxHeight = Screen.height;
		ScreenshotQuality = 25;
	}

	public IEnumerable<string> GetReportAttachments(BacktraceData data)
	{
		string uuidString = data.UuidString;
		List<string> list = new List<string>();
		try
		{
			AddIfPathIsNotEmpty(list, GetScreenshotPath(uuidString));
			AddIfPathIsNotEmpty(list, GetUnityPlayerLogFile(data, uuidString));
			AddIfPathIsNotEmpty(list, GetMinidumpPath(data, uuidString));
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"Cannot generate report attachments. Reason: {ex.Message}");
		}
		return list;
	}

	private void AddIfPathIsNotEmpty(List<string> source, string attachmentPath)
	{
		if (!string.IsNullOrEmpty(attachmentPath))
		{
			source.Add(attachmentPath);
		}
	}

	private string GetMinidumpPath(BacktraceData backtraceData, string dataPrefix)
	{
		if (_settings.MinidumpType == MiniDumpType.None)
		{
			return string.Empty;
		}
		string text = Path.Combine(_settings.DatabasePath, $"{dataPrefix}-dump.dmp");
		BacktraceReport report = backtraceData.Report;
		if (report == null)
		{
			return string.Empty;
		}
		MinidumpException exceptionType = (report.ExceptionTypeReport ? MinidumpException.Present : MinidumpException.None);
		if (!MinidumpHelper.Write(text, _settings.MinidumpType, exceptionType))
		{
			return string.Empty;
		}
		return text;
	}

	private string GetScreenshotPath(string dataPrefix)
	{
		if (!_settings.GenerateScreenshotOnException)
		{
			return string.Empty;
		}
		string text = Path.Combine(_settings.DatabasePath, $"{dataPrefix}-screen.jpg");
		lock (_lock)
		{
			if (BacktraceDatabase.LastFrameTime == _lastScreenTime)
			{
				if (File.Exists(_lastScreenPath))
				{
					File.Copy(_lastScreenPath, text);
					return text;
				}
				return _lastScreenPath;
			}
			float num = (float)Screen.width / (float)Screen.height;
			bool num2 = ScreenshotMaxHeight == Screen.height;
			int num3 = (num2 ? Screen.height : Mathf.Min(Screen.height, ScreenshotMaxHeight));
			int num4 = (num2 ? Screen.width : Mathf.RoundToInt((float)num3 * num));
			RenderTexture temporary = RenderTexture.GetTemporary(Screen.width, Screen.height);
			ScreenCapture.CaptureScreenshotIntoRenderTexture(temporary);
			RenderTexture temporary2 = RenderTexture.GetTemporary(num4, num3);
			if (SystemInfo.graphicsUVStartsAtTop)
			{
				Graphics.Blit(temporary, temporary2, new Vector2(1f, -1f), new Vector2(0f, 1f));
			}
			else
			{
				Graphics.Blit(temporary, temporary2);
			}
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = temporary2;
			Texture2D texture2D = new Texture2D(num4, num3, TextureFormat.RGB24, mipChain: false);
			texture2D.ReadPixels(new Rect(0f, 0f, num4, num3), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary2);
			RenderTexture.ReleaseTemporary(temporary);
			File.WriteAllBytes(text, texture2D.EncodeToJPG(ScreenshotQuality));
			UnityEngine.Object.Destroy(texture2D);
			_lastScreenTime = BacktraceDatabase.LastFrameTime;
			_lastScreenPath = text;
			return text;
		}
	}

	private string GetUnityPlayerLogFile(BacktraceData backtraceData, string dataPrefix)
	{
		if (!_settings.AddUnityLogToReport)
		{
			return string.Empty;
		}
		string text = Path.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).FullName, $"LocalLow/{Application.companyName}/{Application.productName}/Player.log");
		if (string.IsNullOrEmpty(text) || !File.Exists(text))
		{
			return string.Empty;
		}
		string text2 = Path.Combine(_settings.DatabasePath, $"{dataPrefix}-lg.log");
		File.Copy(text, text2);
		return text2;
	}
}
