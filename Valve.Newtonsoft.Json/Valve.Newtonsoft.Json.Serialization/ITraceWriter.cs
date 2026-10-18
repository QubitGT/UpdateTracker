using System;
using System.Diagnostics;

namespace Valve.Newtonsoft.Json.Serialization;

public interface ITraceWriter
{
	TraceLevel LevelFilter { get; }

	void Trace(TraceLevel level, string message, Exception ex);
}
