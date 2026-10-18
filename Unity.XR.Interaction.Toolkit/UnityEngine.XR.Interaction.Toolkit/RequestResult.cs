using System;

namespace UnityEngine.XR.Interaction.Toolkit;

[Obsolete("RequestResult is deprecated in XRI 3.0.0 and will be removed in a future release. Exclusive access behavior is no longer supported.", false)]
public enum RequestResult
{
	Success,
	Busy,
	Error
}
