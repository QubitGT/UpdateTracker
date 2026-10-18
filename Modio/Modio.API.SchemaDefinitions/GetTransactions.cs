using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct GetTransactions
{
	internal readonly TransactionObject[] Data;

	internal readonly PaginationObject Download;

	[JsonConstructor]
	public GetTransactions(TransactionObject[] data, PaginationObject download)
	{
		Data = data;
		Download = download;
	}
}
