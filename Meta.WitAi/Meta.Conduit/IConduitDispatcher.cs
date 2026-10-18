using System;
using System.Threading.Tasks;

namespace Meta.Conduit;

internal interface IConduitDispatcher
{
	Manifest Manifest { get; }

	Task Initialize(string manifestFilePath);

	bool InvokeAction(IParameterProvider parameterProvider, string actionId, bool relaxed, float confidence = 1f, bool partial = false);

	bool InvokeError(string actionId, Exception exception = null);
}
