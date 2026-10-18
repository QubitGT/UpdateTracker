using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Backtrace.Unity.Model.Attributes;

internal sealed class MachineStateAttributeProvider : IDynamicAttributeProvider
{
	public void GetAttributes(IDictionary<string, string> attributes)
	{
		if (attributes != null)
		{
			attributes["battery.level"] = ((SystemInfo.batteryLevel == -1f) ? (-1f) : (SystemInfo.batteryLevel * 100f)).ToString(CultureInfo.InvariantCulture);
			attributes["battery.status"] = SystemInfo.batteryStatus.ToString();
		}
	}
}
