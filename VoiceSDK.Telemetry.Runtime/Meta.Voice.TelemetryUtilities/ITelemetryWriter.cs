using System.Collections.Generic;

namespace Meta.Voice.TelemetryUtilities;

public interface ITelemetryWriter
{
	void StartEvent(OperationID operationId, RuntimeTelemetryEventType runtimeTelemetryEventType);

	void LogEventTermination(OperationID operationId, TerminationReason reason = TerminationReason.Successful, string message = "");

	void LogInstantaneousEvent(OperationID operationId, RuntimeTelemetryEventType runtimeTelemetryEventType, Dictionary<string, string> annotations = null);

	void LogPoint(OperationID operationId, RuntimeTelemetryPoint point);

	void AnnotateEvent(OperationID operationID, string annotationKey, string annotationValue);
}
