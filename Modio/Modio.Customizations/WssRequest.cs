using Newtonsoft.Json.Linq;

namespace Modio.Customizations;

internal static class WssRequest
{
	public static WssMessage DeviceLogin()
	{
		WssMessage result = default(WssMessage);
		result.operation = "device_login";
		result.context = JToken.FromObject(default(WssDeviceLoginRequest));
		return result;
	}
}
