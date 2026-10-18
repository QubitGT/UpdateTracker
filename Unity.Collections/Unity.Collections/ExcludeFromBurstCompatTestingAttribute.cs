using System;

namespace Unity.Collections;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
public class ExcludeFromBurstCompatTestingAttribute : Attribute
{
	public string Reason { get; set; }

	public ExcludeFromBurstCompatTestingAttribute(string _reason)
	{
		Reason = _reason;
	}
}
