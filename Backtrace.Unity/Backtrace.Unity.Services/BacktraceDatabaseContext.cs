using System;
using System.Collections.Generic;
using System.Linq;
using Backtrace.Unity.Interfaces;
using Backtrace.Unity.Model;
using Backtrace.Unity.Model.Database;
using Backtrace.Unity.Types;

namespace Backtrace.Unity.Services;

internal class BacktraceDatabaseContext : IBacktraceDatabaseContext, IDisposable
{
	internal long TotalSize;

	internal int TotalRecords;

	private readonly int _retryNumber;

	internal IDictionary<int, List<BacktraceDatabaseRecord>> BatchRetry { get; private set; }

	internal RetryOrder RetryOrder { get; set; }

	public DeduplicationStrategy DeduplicationStrategy { get; set; }

	public BacktraceDatabaseContext(BacktraceDatabaseSettings settings)
	{
		_retryNumber = checked((int)settings.RetryLimit);
		RetryOrder = settings.RetryOrder;
		DeduplicationStrategy = settings.DeduplicationStrategy;
		SetupBatch();
	}

	private void SetupBatch()
	{
		BatchRetry = new Dictionary<int, List<BacktraceDatabaseRecord>>();
		if (_retryNumber == 0)
		{
			throw new ArgumentException(string.Format("{0} have to be greater than 0!", "_retryNumber"));
		}
		for (int i = 0; i < _retryNumber; i++)
		{
			BatchRetry[i] = new List<BacktraceDatabaseRecord>();
		}
	}

	public string GetHash(BacktraceData backtraceData)
	{
		string text = ((backtraceData == null) ? string.Empty : (backtraceData.Report.Fingerprint ?? string.Empty));
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (DeduplicationStrategy == DeduplicationStrategy.None)
		{
			return string.Empty;
		}
		return new DeduplicationModel(backtraceData, DeduplicationStrategy).GetSha();
	}

	public BacktraceDatabaseRecord GetRecordByHash(string hash)
	{
		for (int i = 0; i < BatchRetry.Count; i++)
		{
			for (int j = 0; j < BatchRetry[i].Count; j++)
			{
				if (BatchRetry[i][j].Hash == hash)
				{
					BacktraceDatabaseRecord backtraceDatabaseRecord = BatchRetry[i][j];
					backtraceDatabaseRecord.Locked = true;
					return backtraceDatabaseRecord;
				}
			}
		}
		return null;
	}

	public BacktraceDatabaseRecord Add(BacktraceDatabaseRecord backtraceRecord)
	{
		if (backtraceRecord == null)
		{
			throw new NullReferenceException("BacktraceDatabaseRecord");
		}
		backtraceRecord.Locked = true;
		TotalSize += backtraceRecord.Size;
		BatchRetry[0].Add(backtraceRecord);
		TotalRecords++;
		return backtraceRecord;
	}

	public bool Any(BacktraceDatabaseRecord record)
	{
		return BatchRetry.SelectMany((KeyValuePair<int, List<BacktraceDatabaseRecord>> n) => n.Value).Any((BacktraceDatabaseRecord n) => n.Id == record.Id);
	}

	public bool Any()
	{
		return TotalRecords != 0;
	}

	public void Delete(BacktraceDatabaseRecord record)
	{
		if (record == null)
		{
			return;
		}
		for (int i = 0; i < BatchRetry.Keys.Count; i++)
		{
			int key = BatchRetry.Keys.ElementAt(i);
			for (int j = 0; j < BatchRetry[key].Count; j++)
			{
				BacktraceDatabaseRecord backtraceDatabaseRecord = BatchRetry[key].ElementAt(j);
				if (backtraceDatabaseRecord.Id == record.Id)
				{
					BatchRetry[key].Remove(backtraceDatabaseRecord);
					if (backtraceDatabaseRecord.Count > 0)
					{
						TotalRecords -= backtraceDatabaseRecord.Count;
					}
					else
					{
						TotalRecords--;
					}
					TotalSize -= backtraceDatabaseRecord.Size;
					return;
				}
			}
		}
	}

	public void IncrementBatchRetry()
	{
		RemoveMaxRetries();
		IncrementBatches();
	}

	private void IncrementBatches()
	{
		for (int num = _retryNumber - 2; num >= 0; num--)
		{
			List<BacktraceDatabaseRecord> value = BatchRetry[num];
			BatchRetry[num] = new List<BacktraceDatabaseRecord>();
			BatchRetry[num + 1] = value;
		}
	}

	private void RemoveMaxRetries()
	{
		List<BacktraceDatabaseRecord> list = BatchRetry[_retryNumber - 1];
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			BacktraceDatabaseRecord backtraceDatabaseRecord = list[i];
			if (backtraceDatabaseRecord.Count > 0)
			{
				TotalRecords -= backtraceDatabaseRecord.Count;
			}
			else
			{
				TotalRecords--;
			}
			TotalSize -= backtraceDatabaseRecord.Size;
		}
	}

	public IEnumerable<BacktraceDatabaseRecord> Get()
	{
		return BatchRetry.SelectMany((KeyValuePair<int, List<BacktraceDatabaseRecord>> n) => n.Value);
	}

	public int Count()
	{
		int num = 0;
		for (int i = 0; i < BatchRetry.Count; i++)
		{
			for (int j = 0; j < BatchRetry[i].Count; j++)
			{
				num += BatchRetry[i][j].Count;
			}
		}
		return num;
	}

	public void Dispose()
	{
		TotalRecords = 0;
		BatchRetry.Clear();
	}

	public void Clear()
	{
		TotalRecords = 0;
		TotalSize = 0L;
		foreach (KeyValuePair<int, List<BacktraceDatabaseRecord>> item in BatchRetry)
		{
			item.Value.Clear();
		}
	}

	public BacktraceDatabaseRecord LastOrDefault()
	{
		if (RetryOrder != 0)
		{
			return GetFirstRecord();
		}
		return GetLastRecord();
	}

	public BacktraceDatabaseRecord FirstOrDefault()
	{
		if (RetryOrder != RetryOrder.Queue)
		{
			return GetLastRecord();
		}
		return GetFirstRecord();
	}

	public BacktraceDatabaseRecord FirstOrDefault(Func<BacktraceDatabaseRecord, bool> predicate)
	{
		return BatchRetry.SelectMany((KeyValuePair<int, List<BacktraceDatabaseRecord>> n) => n.Value).FirstOrDefault(predicate);
	}

	private BacktraceDatabaseRecord GetFirstRecord()
	{
		for (int i = 0; i < _retryNumber; i++)
		{
			if (BatchRetry.ContainsKey(i) && BatchRetry[i].Any((BacktraceDatabaseRecord n) => !n.Locked))
			{
				BacktraceDatabaseRecord backtraceDatabaseRecord = BatchRetry[i].FirstOrDefault((BacktraceDatabaseRecord n) => !n.Locked);
				if (backtraceDatabaseRecord == null)
				{
					return null;
				}
				backtraceDatabaseRecord.Locked = true;
				return backtraceDatabaseRecord;
			}
		}
		return null;
	}

	private BacktraceDatabaseRecord GetLastRecord()
	{
		for (int num = _retryNumber - 1; num >= 0; num--)
		{
			if (BatchRetry[num].Any((BacktraceDatabaseRecord n) => !n.Locked))
			{
				BacktraceDatabaseRecord backtraceDatabaseRecord = BatchRetry[num].Last((BacktraceDatabaseRecord n) => !n.Locked);
				backtraceDatabaseRecord.Locked = true;
				return backtraceDatabaseRecord;
			}
		}
		return null;
	}

	public long GetSize()
	{
		return TotalSize;
	}

	public int GetTotalNumberOfRecords()
	{
		return Count();
	}

	public IEnumerable<BacktraceDatabaseRecord> GetRecordsToDelete()
	{
		return BatchRetry[_retryNumber - 1];
	}

	public void AddDuplicate(BacktraceDatabaseRecord record)
	{
		record.Increment();
		TotalRecords++;
	}
}
