using System;
using Meta.Conduit;

namespace Meta.WitAi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ValidatePartialIntent : ConduitActionAttribute
{
	public ValidatePartialIntent(string intent, float minConfidence = 0.51f, float maxConfidence = 1f)
		: base(intent, minConfidence, maxConfidence, true)
	{
	}
}
