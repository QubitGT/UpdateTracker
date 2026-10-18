using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Modio.API.SchemaDefinitions;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public readonly struct Pagination<T>
{
	public readonly T Data;

	public readonly long ResultCount;

	public readonly long ResultOffset;

	public readonly long ResultLimit;

	public readonly long ResultTotal;

	[JsonConstructor]
	internal Pagination(T data, long resultCount, long resultOffset, long resultLimit, long resultTotal)
	{
		Data = data;
		ResultCount = resultCount;
		ResultOffset = resultOffset;
		ResultLimit = resultLimit;
		ResultTotal = resultTotal;
	}
}
