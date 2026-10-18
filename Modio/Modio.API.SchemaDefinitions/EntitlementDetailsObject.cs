using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
public readonly struct EntitlementDetailsObject
{
	internal readonly long TokensAllocated;

	[JsonConstructor]
	public EntitlementDetailsObject(long tokens_allocated)
	{
		TokensAllocated = tokens_allocated;
	}
}
