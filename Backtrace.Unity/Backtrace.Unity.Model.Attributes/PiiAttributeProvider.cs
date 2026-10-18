using System.Collections.Generic;
using UnityEngine;

namespace Backtrace.Unity.Model.Attributes;

internal sealed class PiiAttributeProvider : IScopeAttributeProvider
{
	public void GetAttributes(IDictionary<string, string> attributes)
	{
		if (attributes != null && SystemInfo.deviceModel != "n/a")
		{
			attributes["device.model"] = SystemInfo.deviceModel;
			attributes["device.machine"] = SystemInfo.deviceModel;
			attributes["device.type"] = SystemInfo.deviceType.ToString();
		}
	}
}
