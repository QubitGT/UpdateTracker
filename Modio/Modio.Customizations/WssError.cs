using System;
using System.Collections.Generic;

namespace Modio.Customizations;

[Serializable]
public struct WssError
{
	public long code;

	public long error_ref;

	public string message;

	public Dictionary<string, string> errors;
}
