using System.Collections.Generic;

namespace Liv.Lck.Core;

internal interface ILckTelemetryContextProvider
{
	void SetTelemetryContext(LckTelemetryContextType contextType, Dictionary<string, object> context);

	void ClearTelemetryContext(LckTelemetryContextType contextType);
}
