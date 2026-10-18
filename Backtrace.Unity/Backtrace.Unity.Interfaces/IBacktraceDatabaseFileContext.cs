using System.Collections.Generic;
using System.IO;
using Backtrace.Unity.Model;
using Backtrace.Unity.Model.Database;

namespace Backtrace.Unity.Interfaces;

internal interface IBacktraceDatabaseFileContext
{
	int ScreenshotQuality { get; set; }

	int ScreenshotMaxHeight { get; set; }

	IEnumerable<FileInfo> GetRecords();

	IEnumerable<FileInfo> GetAll();

	bool ValidFileConsistency();

	void RemoveOrphaned(IEnumerable<BacktraceDatabaseRecord> existingRecords);

	void Clear();

	void Delete(BacktraceDatabaseRecord record);

	IEnumerable<string> GenerateRecordAttachments(BacktraceData data);

	bool Save(BacktraceDatabaseRecord record);

	bool IsValidRecord(BacktraceDatabaseRecord record);
}
