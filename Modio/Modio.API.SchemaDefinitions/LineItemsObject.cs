using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct LineItemsObject
{
	internal readonly long GameId;

	internal readonly long BuyerId;

	internal readonly string GameName;

	internal readonly string BuyerName;

	internal readonly string TokenName;

	internal readonly long TokenPackId;

	internal readonly string TokenPackName;

	[JsonConstructor]
	public LineItemsObject(long game_id, long buyer_id, string game_name, string buyer_name, string token_name, long token_pack_id, string token_pack_name)
	{
		GameId = game_id;
		BuyerId = buyer_id;
		GameName = game_name;
		BuyerName = buyer_name;
		TokenName = token_name;
		TokenPackId = token_pack_id;
		TokenPackName = token_pack_name;
	}
}
