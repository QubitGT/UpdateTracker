using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct ModPlatformsObject
{
	internal readonly string Platform;

	internal readonly long ModfileLive;

	[JsonConstructor]
	public ModPlatformsObject(string platform, long modfile_live)
	{
		Platform = platform;
		ModfileLive = modfile_live;
	}
}
