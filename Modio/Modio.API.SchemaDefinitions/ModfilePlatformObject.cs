using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct ModfilePlatformObject
{
	internal readonly string Platform;

	internal readonly long Status;

	[JsonConstructor]
	public ModfilePlatformObject(string platform, long status)
	{
		Platform = platform;
		Status = status;
	}
}
