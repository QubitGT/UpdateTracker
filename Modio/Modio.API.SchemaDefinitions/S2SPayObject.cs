using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct S2SPayObject
{
	internal readonly long TransactionId;

	internal readonly string GatewayUuid;

	internal readonly long GrossAmount;

	internal readonly long NetAmount;

	internal readonly long PlatformFee;

	internal readonly long GatewayFee;

	internal readonly string TransactionType;

	internal readonly JObject Meta;

	internal readonly long PurchaseDate;

	[JsonConstructor]
	public S2SPayObject(long transaction_id, string gateway_uuid, long gross_amount, long net_amount, long platform_fee, long gateway_fee, string transaction_type, JObject meta, long purchase_date)
	{
		TransactionId = transaction_id;
		GatewayUuid = gateway_uuid;
		GrossAmount = gross_amount;
		NetAmount = net_amount;
		PlatformFee = platform_fee;
		GatewayFee = gateway_fee;
		TransactionType = transaction_type;
		Meta = meta;
		PurchaseDate = purchase_date;
	}
}
