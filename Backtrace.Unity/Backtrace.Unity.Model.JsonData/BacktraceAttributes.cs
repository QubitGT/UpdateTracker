using System.Collections.Generic;
using Backtrace.Unity.Json;

namespace Backtrace.Unity.Model.JsonData;

public class BacktraceAttributes
{
	public readonly Dictionary<string, string> Attributes;

	public BacktraceAttributes(BacktraceReport report, Dictionary<string, string> clientAttributes)
	{
		Attributes = clientAttributes;
		if (report != null && report.Attributes != null)
		{
			if (Attributes == null)
			{
				Attributes = report.Attributes;
			}
			else
			{
				foreach (KeyValuePair<string, string> attribute in report.Attributes)
				{
					Attributes[attribute.Key] = attribute.Value;
				}
			}
		}
		if (Attributes == null)
		{
			Attributes = new Dictionary<string, string>();
		}
	}

	public BacktraceJObject ToJson()
	{
		return new BacktraceJObject(Attributes);
	}
}
