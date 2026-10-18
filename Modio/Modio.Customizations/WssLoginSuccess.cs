using System;

namespace Modio.Customizations;

[Serializable]
public struct WssLoginSuccess
{
	public long code;

	public string access_token;

	public long date_expires;
}
