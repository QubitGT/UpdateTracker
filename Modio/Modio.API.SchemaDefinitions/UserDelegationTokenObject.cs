using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct UserDelegationTokenObject
{
	internal readonly string Entity;

	internal readonly string Token;

	[JsonConstructor]
	public UserDelegationTokenObject(string entity, string token)
	{
		Entity = entity;
		Token = token;
	}
}
