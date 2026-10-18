using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct ProxyTransformerObject
{
	internal readonly bool Success;

	[JsonConstructor]
	public ProxyTransformerObject(bool success)
	{
		Success = success;
	}
}
