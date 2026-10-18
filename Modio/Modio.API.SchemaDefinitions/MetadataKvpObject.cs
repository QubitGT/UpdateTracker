using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject(MemberSerialization.Fields)]
internal readonly struct MetadataKvpObject
{
	internal readonly string Metakey;

	internal readonly string Metavalue;

	[JsonConstructor]
	public MetadataKvpObject(string metakey, string metavalue)
	{
		Metakey = metakey;
		Metavalue = metavalue;
	}
}
