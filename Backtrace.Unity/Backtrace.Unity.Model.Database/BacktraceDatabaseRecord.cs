using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Backtrace.Unity.Model.Database;

public class BacktraceDatabaseRecord
{
	[Serializable]
	private struct BacktraceDatabaseRawRecord
	{
		public string Id;

		public string recordName;

		public string dataPath;

		public long size;

		public string hash;

		public List<string> attachments;
	}

	public Guid Id = Guid.NewGuid();

	internal bool Locked;

	public string Hash = string.Empty;

	private int _count = 1;

	internal string RecordPath { get; set; }

	internal string DiagnosticDataPath { get; set; }

	internal long Size { get; set; }

	internal BacktraceData Record { get; set; }

	public ICollection<string> Attachments { get; private set; }

	internal string DiagnosticDataJson { get; set; }

	public bool Duplicated => _count != 1;

	public int Count => _count;

	public BacktraceData BacktraceData
	{
		get
		{
			if (Record != null)
			{
				Record.Deduplication = Count;
				return Record;
			}
			return null;
		}
	}

	public string BacktraceDataJson()
	{
		if (!string.IsNullOrEmpty(DiagnosticDataJson))
		{
			return DiagnosticDataJson;
		}
		if (Record != null)
		{
			return Record.ToJson();
		}
		if (string.IsNullOrEmpty(DiagnosticDataPath) || !File.Exists(DiagnosticDataPath))
		{
			return null;
		}
		DiagnosticDataJson = File.ReadAllText(DiagnosticDataPath);
		return DiagnosticDataJson;
	}

	public string ToJson()
	{
		BacktraceDatabaseRawRecord backtraceDatabaseRawRecord = default(BacktraceDatabaseRawRecord);
		backtraceDatabaseRawRecord.Id = Id.ToString();
		backtraceDatabaseRawRecord.recordName = RecordPath;
		backtraceDatabaseRawRecord.dataPath = DiagnosticDataPath;
		backtraceDatabaseRawRecord.size = Size;
		backtraceDatabaseRawRecord.hash = Hash;
		backtraceDatabaseRawRecord.attachments = new List<string>(Attachments);
		return JsonUtility.ToJson(backtraceDatabaseRawRecord, prettyPrint: false);
	}

	public static BacktraceDatabaseRecord Deserialize(string json)
	{
		return new BacktraceDatabaseRecord(JsonUtility.FromJson<BacktraceDatabaseRawRecord>(json));
	}

	private BacktraceDatabaseRecord(BacktraceDatabaseRawRecord rawRecord)
	{
		Id = new Guid(rawRecord.Id);
		RecordPath = rawRecord.recordName;
		DiagnosticDataPath = rawRecord.dataPath;
		Size = rawRecord.size;
		Hash = rawRecord.hash;
		Attachments = rawRecord.attachments;
	}

	public BacktraceDatabaseRecord(BacktraceData data)
	{
		Id = data.Uuid;
		Record = data;
		Attachments = data.Attachments;
	}

	public virtual void Increment()
	{
		_count++;
	}

	internal static BacktraceDatabaseRecord ReadFromFile(FileInfo file)
	{
		if (!file.Exists)
		{
			return null;
		}
		using StreamReader streamReader = file.OpenText();
		string json = streamReader.ReadToEnd();
		try
		{
			return Deserialize(json);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public virtual void Unlock()
	{
		Locked = false;
		Record = null;
	}
}
