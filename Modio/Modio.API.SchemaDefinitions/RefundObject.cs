using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct RefundObject
{
	internal readonly long TransactionId;

	internal readonly long GrossAmount;

	internal readonly long NetAmount;

	internal readonly long PlatformFee;

	internal readonly long GatewayFee;

	internal readonly long Tax;

	internal readonly string TaxType;

	internal readonly string TransactionType;

	internal readonly JObject Meta;

	internal readonly long PurchaseDate;

	[JsonConstructor]
	public RefundObject(long transaction_id, long gross_amount, long net_amount, long platform_fee, long gateway_fee, long tax, string tax_type, string transaction_type, JObject meta, long purchase_date)
	{
		TransactionId = transaction_id;
		GrossAmount = gross_amount;
		NetAmount = net_amount;
		PlatformFee = platform_fee;
		GatewayFee = gateway_fee;
		Tax = tax;
		TaxType = tax_type;
		TransactionType = transaction_type;
		Meta = meta;
		PurchaseDate = purchase_date;
	}
}
