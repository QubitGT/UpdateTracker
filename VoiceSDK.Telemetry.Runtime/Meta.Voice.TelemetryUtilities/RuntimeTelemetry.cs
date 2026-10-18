using System.Collections.Generic;

namespace Meta.Voice.TelemetryUtilities;

public class RuntimeTelemetry : ITelemetryWriter
{
	private readonly List<ITelemetryWriter> _writers = new List<ITelemetryWriter>();

	public static RuntimeTelemetry Instance { get; } = new RuntimeTelemetry();


	internal RuntimeTelemetry()
	{
	}

	public void RegisterWriter(ITelemetryWriter writer)
	{
		_writers.Add(writer);
	}

	public void StartEvent(OperationID operationId, RuntimeTelemetryEventType runtimeTelemetryEventType)
	{
		foreach (ITelemetryWriter writer in _writers)
		{
			writer.StartEvent(operationId, runtimeTelemetryEventType);
		}
	}

	public void LogEventTermination(OperationID operationId, TerminationReason reason = TerminationReason.Successful, string message = "")
	{
		foreach (ITelemetryWriter writer in _writers)
		{
			writer.LogEventTermination(operationId, reason, message);
		}
	}

	public void LogInstantaneousEvent(OperationID operationId, RuntimeTelemetryEventType runtimeTelemetryEventType, Dictionary<string, string> annotations = null)
	{
		foreach (ITelemetryWriter writer in _writers)
		{
			writer.LogInstantaneousEvent(operationId, runtimeTelemetryEventType, annotations);
		}
	}

	public void LogPoint(OperationID operationId, RuntimeTelemetryPoint point)
	{
		foreach (ITelemetryWriter writer in _writers)
		{
			writer.LogPoint(operationId, point);
		}
	}

	public void LogPoint(string operationId, RuntimeTelemetryPoint point)
	{
		LogPoint((OperationID)operationId, point);
	}

	public void AnnotateEvent(OperationID operationID, string annotationKey, string annotationValue)
	{
		foreach (ITelemetryWriter writer in _writers)
		{
			writer.AnnotateEvent(operationID, annotationKey, annotationValue);
		}
	}
}
