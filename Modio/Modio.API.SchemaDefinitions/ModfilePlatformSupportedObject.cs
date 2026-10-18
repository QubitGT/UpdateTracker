using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct ModfilePlatformSupportedObject
{
	internal readonly string[] Targetted;

	internal readonly string[] Approved;

	internal readonly string[] Denied;

	internal readonly string[] Live;

	internal readonly string[] Pending;

	[JsonConstructor]
	public ModfilePlatformSupportedObject(string[] targetted, string[] approved, string[] denied, string[] live, string[] pending)
	{
		Targetted = targetted;
		Approved = approved;
		Denied = denied;
		Live = live;
		Pending = pending;
	}
}
