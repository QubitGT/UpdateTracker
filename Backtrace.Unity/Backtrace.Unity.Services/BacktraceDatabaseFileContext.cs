using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Backtrace.Unity.Common;
using Backtrace.Unity.Interfaces;
using Backtrace.Unity.Model;
using Backtrace.Unity.Model.Database;
using UnityEngine;

namespace Backtrace.Unity.Services;

internal class BacktraceDatabaseFileContext : IBacktraceDatabaseFileContext
{
	private string[] _possibleDatabaseExtension = new string[4] { ".dmp", ".json", ".jpg", ".log" };

	private readonly long _maxDatabaseSize;

	private readonly uint _maxRecordNumber;

	private readonly DirectoryInfo _databaseDirectoryInfo;

	private const string RecordFilterRegex = "*-record.json";

	internal readonly BacktraceDatabaseAttachmentManager _attachmentManager;

	private readonly string _path;

	public int ScreenshotQuality
	{
		get
		{
			return _attachmentManager.ScreenshotQuality;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentException("ScreenshotQuality has to be greater than 0");
			}
			if (value > 100)
			{
				throw new ArgumentException("ScreenshotQuality cannot be larger than 100");
			}
			_attachmentManager.ScreenshotQuality = value;
		}
	}

	public int ScreenshotMaxHeight
	{
		get
		{
			return _attachmentManager.ScreenshotMaxHeight;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentException("ScreenshotMaxHeight has to be greater than 0");
			}
			_attachmentManager.ScreenshotMaxHeight = value;
		}
	}

	public BacktraceDatabaseFileContext(BacktraceDatabaseSettings settings)
	{
		_attachmentManager = new BacktraceDatabaseAttachmentManager(settings);
		_maxDatabaseSize = settings.MaxDatabaseSize;
		_maxRecordNumber = settings.MaxRecordCount;
		_path = settings.DatabasePath;
		_databaseDirectoryInfo = new DirectoryInfo(_path);
	}

	public IEnumerable<FileInfo> GetAll()
	{
		return _databaseDirectoryInfo.GetFiles();
	}

	public IEnumerable<FileInfo> GetRecords()
	{
		return from n in _databaseDirectoryInfo.GetFiles("*-record.json", SearchOption.TopDirectoryOnly)
			orderby n.CreationTime
			select n;
	}

	public void RemoveOrphaned(IEnumerable<BacktraceDatabaseRecord> existingRecords)
	{
		IEnumerable<string> source = existingRecords.Select((BacktraceDatabaseRecord n) => n.Id.ToString());
		IEnumerable<FileInfo> all = GetAll();
		for (int i = 0; i < all.Count(); i++)
		{
			FileInfo file = all.ElementAt(i);
			try
			{
				if (file.Name.StartsWith("bt-breadcrumbs"))
				{
					continue;
				}
				if (!_possibleDatabaseExtension.Any((string n) => n == file.Extension))
				{
					file.Delete();
					continue;
				}
				int num = file.Name.LastIndexOf('-');
				if (num == -1)
				{
					file.Delete();
					continue;
				}
				string value = file.Name.Substring(0, num);
				if (!source.Contains(value))
				{
					file.Delete();
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Cannot remove file in path: {file.FullName}. Reason: {ex.Message}");
			}
		}
	}

	public bool ValidFileConsistency()
	{
		FileInfo[] files = _databaseDirectoryInfo.GetFiles();
		long num = 0L;
		long num2 = 0L;
		FileInfo[] array = files;
		foreach (FileInfo fileInfo in array)
		{
			if (Regex.Match(fileInfo.FullName, "*-record.json").Success)
			{
				num2++;
				if (_maxRecordNumber > num2)
				{
					return false;
				}
			}
			num += fileInfo.Length;
			if (num > _maxDatabaseSize)
			{
				return false;
			}
		}
		return true;
	}

	public void Clear()
	{
		FileInfo[] files = _databaseDirectoryInfo.GetFiles();
		for (int i = 0; i < files.Length; i++)
		{
			files[i].Delete();
		}
	}

	public void Delete(BacktraceDatabaseRecord record)
	{
		Delete(record.DiagnosticDataPath);
		Delete(record.RecordPath);
		if (record.Attachments == null || record.Attachments.Count == 0)
		{
			return;
		}
		foreach (string attachment in record.Attachments)
		{
			Delete(attachment);
		}
	}

	private bool IsDatabaseDependency(string path)
	{
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			return false;
		}
		if (ClientPathHelper.IsFileInDatabaseDirectory(_path, path))
		{
			return !path.EndsWith("bt-breadcrumbs-0");
		}
		return false;
	}

	private void Delete(string path)
	{
		try
		{
			if (IsDatabaseDependency(path))
			{
				File.Delete(path);
			}
		}
		catch (IOException ex)
		{
			Debug.Log($"File {path} is in use. Message: {ex.Message}");
		}
		catch (Exception ex2)
		{
			Debug.Log($"Cannot delete file: {path}. Message: {ex2.Message}");
		}
	}

	public IEnumerable<string> GenerateRecordAttachments(BacktraceData data)
	{
		return _attachmentManager.GetReportAttachments(data);
	}

	public bool Save(BacktraceDatabaseRecord record)
	{
		try
		{
			string uuidString = record.BacktraceData.UuidString;
			record.DiagnosticDataJson = record.BacktraceData.ToJson();
			record.DiagnosticDataPath = Path.Combine(_path, $"{uuidString}-attachment.json");
			record.Size += Save(record.DiagnosticDataJson, record.DiagnosticDataPath);
			if (record.Attachments != null && record.Attachments.Count != 0)
			{
				foreach (string attachment in record.Attachments)
				{
					if (IsDatabaseDependency(attachment))
					{
						record.Size += new FileInfo(attachment).Length;
					}
				}
			}
			record.RecordPath = Path.Combine(_path, $"{uuidString}-record.json");
			string text = record.ToJson();
			record.Size += Encoding.Unicode.GetByteCount(text);
			Save(text, record.RecordPath);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogWarning($"Backtrace: Cannot save record on the hard drive. Reason: {ex.Message}");
			Delete(record);
			return false;
		}
	}

	private int Save(string json, string destPath)
	{
		if (string.IsNullOrEmpty(json))
		{
			return 0;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(json);
		using (FileStream fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
		{
			fileStream.Write(bytes, 0, bytes.Length);
		}
		return bytes.Length;
	}

	public bool IsValidRecord(BacktraceDatabaseRecord record)
	{
		if (record == null)
		{
			return false;
		}
		return File.Exists(record.DiagnosticDataPath);
	}
}
