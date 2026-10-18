using System;
using Backtrace.Unity.Types;

namespace Backtrace.Unity.Model.Database;

public class BacktraceDatabaseSettings
{
	private readonly BacktraceConfiguration _configuration;

	private readonly uint _retryInterval;

	public string DatabasePath { get; private set; }

	public uint MaxRecordCount => Convert.ToUInt32(_configuration.MaxRecordCount);

	public long MaxDatabaseSize => _configuration.MaxDatabaseSize * 1000 * 1000;

	public bool AutoSendMode => _configuration.AutoSendMode;

	public uint RetryInterval => _retryInterval;

	public uint RetryLimit => Convert.ToUInt32(_configuration.RetryLimit);

	public DeduplicationStrategy DeduplicationStrategy => _configuration.DeduplicationStrategy;

	public bool GenerateScreenshotOnException => _configuration.GenerateScreenshotOnException;

	public bool AddUnityLogToReport => _configuration.AddUnityLogToReport;

	public RetryOrder RetryOrder => _configuration.RetryOrder;

	public MiniDumpType MinidumpType => _configuration.MinidumpType;

	public BacktraceDatabaseSettings(string databasePath, BacktraceConfiguration configuration)
	{
		if (!(configuration == null) && !string.IsNullOrEmpty(databasePath))
		{
			DatabasePath = databasePath;
			_configuration = configuration;
			_retryInterval = ((configuration.RetryInterval > 0) ? ((uint)_configuration.RetryInterval) : 60u);
		}
	}
}
