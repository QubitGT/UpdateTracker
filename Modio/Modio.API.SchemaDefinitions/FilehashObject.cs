using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct FilehashObject
{
	internal readonly string Md5;

	[JsonConstructor]
	public FilehashObject(string md5)
	{
		Md5 = md5;
	}
}
