using System;

namespace Modio.Customizations;

[Serializable]
internal struct WssDeviceLoginResponse
{
	public string code;

	public long date_expires;

	public string display_url;

	public string login_url;
}
