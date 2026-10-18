using System;
using System.Runtime.CompilerServices;

namespace Meta.Voice.Logging;

internal class LazyLogger : Lazy<IVLogger>, IVLogger, ICoreLogger
{
	public CorrelationID CorrelationID
	{
		get
		{
			return base.Value.CorrelationID;
		}
		set
		{
			base.Value.CorrelationID = value;
		}
	}

	public LazyLogger(Func<IVLogger> initializer)
		: base(initializer)
	{
	}

	public void Verbose(string message, params object[] parameters)
	{
		base.Value.Verbose(message, parameters);
	}

	public void Verbose(CorrelationID correlationId, string message, params object[] parameters)
	{
		base.Value.Verbose(correlationId, message, parameters);
	}

	public void Verbose(string message, object p1 = null, object p2 = null, object p3 = null, object p4 = null, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
	{
		base.Value.Verbose(message, p1, p2, p3, p4, memberName, sourceFilePath, sourceLineNumber);
	}

	public void Info(string message, params object[] parameters)
	{
		base.Value.Info(message, parameters);
	}

	public void Info(CorrelationID correlationId, string message, params object[] parameters)
	{
		base.Value.Info(correlationId, message, parameters);
	}

	public void Info(string message, object p1 = null, object p2 = null, object p3 = null, object p4 = null, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
	{
		base.Value.Info(message, p1, p2, p3, p4, memberName, sourceFilePath, sourceLineNumber);
	}

	public void Debug(string message, params object[] parameters)
	{
		base.Value.Debug(message, parameters);
	}

	public void Debug(string message, object p1 = null, object p2 = null, object p3 = null, object p4 = null, string memberName = "", string sourceFilePath = "", int sourceLineNumber = 0)
	{
		base.Value.Debug(message, p1, p2, p3, p4, memberName, sourceFilePath, sourceLineNumber);
	}

	public void Debug(CorrelationID correlationId, string message, params object[] parameters)
	{
		base.Value.Debug(correlationId, message, parameters);
	}

	public void Warning(CorrelationID correlationId, string message, params object[] parameters)
	{
		base.Value.Warning(correlationId, message, parameters);
	}

	public void Warning(string message, params object[] parameters)
	{
		base.Value.Warning(message, parameters);
	}

	public void Error(CorrelationID correlationId, ErrorCode errorCode, string message, params object[] parameters)
	{
		base.Value.Error(correlationId, errorCode, message, parameters);
	}

	public void Error(ErrorCode errorCode, string message, params object[] parameters)
	{
		base.Value.Error(errorCode, message, parameters);
	}

	public void Error(CorrelationID correlationId, Exception exception, ErrorCode errorCode, string message, params object[] parameters)
	{
		base.Value.Error(correlationId, exception, errorCode, message, parameters);
	}

	public void Error(CorrelationID correlationId, string message, params object[] parameters)
	{
		base.Value.Error(correlationId, message, parameters);
	}

	public void Error(string message, params object[] parameters)
	{
		base.Value.Error(message, parameters);
	}

	public void Error(CorrelationID correlationId, Exception exception, string message = "", params object[] parameters)
	{
		base.Value.Error(correlationId, exception, message, parameters);
	}

	public void Error(Exception exception, ErrorCode errorCode, string message = "", params object[] parameters)
	{
		base.Value.Error(exception, errorCode, message, parameters);
	}

	public void Error(Exception exception, string message = "", params object[] parameters)
	{
		base.Value.Error(exception, message, parameters);
	}

	public ILogScope Scope(VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		return base.Value.Scope(verbosity, message, parameters);
	}

	public ILogScope Scope(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		return base.Value.Scope(correlationId, verbosity, message, parameters);
	}

	public int Start(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		return base.Value.Start(correlationId, verbosity, message, parameters);
	}

	public int Start(VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		return base.Value.Start(verbosity, message, parameters);
	}

	public void End(int sequenceId)
	{
		base.Value.End(sequenceId);
	}

	public void Correlate(CorrelationID newCorrelationId, CorrelationID rootCorrelationId)
	{
		base.Value.Correlate(newCorrelationId, rootCorrelationId);
	}

	public void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, string message, params object[] parameters)
	{
		base.Value.Log(correlationId, verbosity, message, parameters);
	}

	public void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, Exception exception, ErrorCode errorCode, string message = "", params object[] parameters)
	{
		base.Value.Log(correlationId, verbosity, exception, errorCode, message, parameters);
	}

	public void Log(CorrelationID correlationId, VLoggerVerbosity verbosity, ErrorCode errorCode, string message, params object[] parameters)
	{
		base.Value.Log(correlationId, verbosity, errorCode, message, parameters);
	}

	public void Flush(CorrelationID correlationId)
	{
		base.Value.Flush(correlationId);
	}

	public void Flush()
	{
		base.Value.Flush();
	}
}
