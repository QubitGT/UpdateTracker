using System;
using Meta.Conduit;

namespace Meta.WitAi;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MatchIntent : ConduitActionAttribute
{
	public MatchIntent(string intent, float minConfidence = 0.51f, float maxConfidence = 1f)
		: base(intent, minConfidence, maxConfidence, false)
	{
	}
}
