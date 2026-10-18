using System;
using System.Collections.Generic;
using Backtrace.Unity.Model;
using Backtrace.Unity.Model.Database;
using Backtrace.Unity.Types;

namespace Backtrace.Unity.Interfaces;

public interface IBacktraceDatabaseContext : IDisposable
{
	DeduplicationStrategy DeduplicationStrategy { get; set; }

	BacktraceDatabaseRecord Add(BacktraceDatabaseRecord backtraceDatabaseRecord);

	BacktraceDatabaseRecord FirstOrDefault();

	BacktraceDatabaseRecord FirstOrDefault(Func<BacktraceDatabaseRecord, bool> predicate);

	BacktraceDatabaseRecord LastOrDefault();

	IEnumerable<BacktraceDatabaseRecord> Get();

	void Delete(BacktraceDatabaseRecord record);

	bool Any(BacktraceDatabaseRecord n);

	bool Any();

	int Count();

	void Clear();

	void IncrementBatchRetry();

	long GetSize();

	[Obsolete("Please use Count method instead")]
	int GetTotalNumberOfRecords();

	IEnumerable<BacktraceDatabaseRecord> GetRecordsToDelete();

	string GetHash(BacktraceData backtraceData);

	BacktraceDatabaseRecord GetRecordByHash(string hash);

	void AddDuplicate(BacktraceDatabaseRecord record);
}
