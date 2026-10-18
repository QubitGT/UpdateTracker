using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
public readonly struct AccessTokenObject
{
	public readonly long Code;

	public readonly string AccessToken;

	public readonly long DateExpires;

	[JsonConstructor]
	public AccessTokenObject(long code, string access_token, long date_expires)
	{
		Code = code;
		AccessToken = access_token;
		DateExpires = date_expires;
	}
}
