using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Backtrace.Unity.Common;
using Backtrace.Unity.Json;

namespace Backtrace.Unity.Model.Breadcrumbs.Storage;

internal sealed class BacktraceStorageLogManager : IBacktraceLogManager, IArchiveableBreadcrumbManager
{
	public const int MinimumBreadcrumbsFileSize = 10000;

	private long _breadcrumbsSize = 64000L;

	internal const string BreadcrumbLogFilePrefix = "bt-breadcrumbs";

	internal const string BreadcrumbLogFileName = "bt-breadcrumbs-0";

	internal static byte[] NewRow = Encoding.UTF8.GetBytes(",\n");

	internal static byte[] EndOfDocument = Encoding.UTF8.GetBytes("\n]");

	internal static byte[] StartOfDocument = Encoding.UTF8.GetBytes("[\n");

	private bool _emptyFile = true;

	private double _breadcrumbId = DateTimeHelper.TimestampMs();

	private object _lockObject = new object();

	private long currentSize;

	private readonly Queue<long> _logSize = new Queue<long>();

	private readonly string _storagePath;

	public string BreadcrumbsFilePath { get; private set; }

	public long BreadcrumbsSize
	{
		get
		{
			return _breadcrumbsSize;
		}
		set
		{
			if (value < 10000)
			{
				throw new ArgumentException("Breadcrumbs size must be greater or equal to 10kB");
			}
			_breadcrumbsSize = value;
		}
	}

	internal IBreadcrumbFile BreadcrumbFile { get; set; }

	public BacktraceStorageLogManager(string storagePath)
	{
		if (string.IsNullOrEmpty(storagePath))
		{
			throw new ArgumentException("Breadcrumbs storage path is null or empty");
		}
		_storagePath = storagePath;
		BreadcrumbsFilePath = Path.Combine(_storagePath, "bt-breadcrumbs-0");
		BreadcrumbFile = new BreadcrumbFile(BreadcrumbsFilePath);
	}

	public bool Enable()
	{
		if (currentSize != 0L)
		{
			return true;
		}
		try
		{
			if (BreadcrumbFile.Exists())
			{
				BreadcrumbFile.Delete();
			}
			using (Stream stream = BreadcrumbFile.GetCreateStream())
			{
				stream.Write(StartOfDocument, 0, StartOfDocument.Length);
				stream.Write(EndOfDocument, 0, EndOfDocument.Length);
			}
			_emptyFile = true;
			currentSize = StartOfDocument.Length + EndOfDocument.Length;
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public bool Add(string message, BreadcrumbLevel level, UnityEngineLogLevel type, IDictionary<string, string> attributes)
	{
		if (currentSize == 0L)
		{
			return false;
		}
		byte[] bytes;
		lock (_lockObject)
		{
			BacktraceJObject backtraceJObject = CreateBreadcrumbJson(_breadcrumbId++, message, level, type, attributes);
			bytes = Encoding.UTF8.GetBytes(backtraceJObject.ToJson());
			if (currentSize + bytes.Length > BreadcrumbsSize)
			{
				try
				{
					ClearOldLogs();
				}
				catch (Exception)
				{
					return false;
				}
			}
		}
		try
		{
			return AppendBreadcrumb(bytes);
		}
		catch (Exception)
		{
			return false;
		}
	}

	private BacktraceJObject CreateBreadcrumbJson(double id, string message, BreadcrumbLevel level, UnityEngineLogLevel type, IDictionary<string, string> attributes)
	{
		BacktraceJObject backtraceJObject = new BacktraceJObject();
		backtraceJObject.Add("timestamp", DateTimeHelper.TimestampMs(), "F0");
		backtraceJObject.Add("id", id, "F0");
		backtraceJObject.Add("type", Enum.GetName(typeof(BreadcrumbLevel), level).ToLower());
		backtraceJObject.Add("level", Enum.GetName(typeof(UnityEngineLogLevel), type).ToLower());
		backtraceJObject.Add("message", message);
		if (attributes != null && attributes.Count > 0)
		{
			backtraceJObject.Add("attributes", new BacktraceJObject(attributes));
		}
		return backtraceJObject;
	}

	private bool AppendBreadcrumb(byte[] bytes)
	{
		long num = EndOfDocument.Length * -1;
		using (Stream stream = BreadcrumbFile.GetWriteStream())
		{
			stream.Position = stream.Length - EndOfDocument.Length;
			if (!_emptyFile)
			{
				stream.Write(NewRow, 0, NewRow.Length);
				num += NewRow.Length;
			}
			else
			{
				_emptyFile = false;
			}
			stream.Write(bytes, 0, bytes.Length);
			stream.Write(EndOfDocument, 0, EndOfDocument.Length);
			num += bytes.Length + EndOfDocument.Length;
		}
		currentSize += num;
		_logSize.Enqueue(bytes.Length);
		return true;
	}

	private void ClearOldLogs()
	{
		long nextStartPosition = GetNextStartPosition();
		using (Stream stream = BreadcrumbFile.GetIOStream())
		{
			using MemoryStream memoryStream = new MemoryStream();
			long num = stream.Length - nextStartPosition;
			stream.Seek(num * -1, SeekOrigin.End);
			stream.CopyTo(memoryStream);
			stream.SetLength(num + StartOfDocument.Length);
			memoryStream.Position = 0L;
			stream.Position = 0L;
			stream.Write(StartOfDocument, 0, StartOfDocument.Length);
			memoryStream.CopyTo(stream);
		}
		currentSize -= nextStartPosition;
		currentSize += StartOfDocument.Length;
	}

	private long GetNextStartPosition()
	{
		double num = (double)BreadcrumbsSize - (double)BreadcrumbsSize * 0.7;
		long num2 = StartOfDocument.Length;
		for (int num3 = NewRow.Length; (double)num2 < num; num2 += _logSize.Dequeue() + num3)
		{
			if (_logSize.Count == 0)
			{
				return num2;
			}
		}
		return num2;
	}

	public bool Clear()
	{
		try
		{
			currentSize = 0L;
			_logSize.Clear();
			BreadcrumbFile.Delete();
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public int Length()
	{
		return _logSize.Count;
	}

	public double BreadcrumbId()
	{
		return _breadcrumbId;
	}

	public string Archive()
	{
		if (!File.Exists(BreadcrumbsFilePath))
		{
			return string.Empty;
		}
		string text = Path.Combine(_storagePath, string.Format("{0}-1", "bt-breadcrumbs"));
		File.Copy(BreadcrumbsFilePath, text, overwrite: true);
		return text;
	}
}
