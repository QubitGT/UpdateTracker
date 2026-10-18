using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct KeyValuePairObject
{
	internal readonly string Key;

	internal readonly string Value;

	[JsonConstructor]
	public KeyValuePairObject(string key, string value)
	{
		Key = key;
		Value = value;
	}
}
