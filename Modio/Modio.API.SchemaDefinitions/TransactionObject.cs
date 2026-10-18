using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct TransactionObject
{
	internal readonly long Id;

	internal readonly string GatewayUuid;

	internal readonly string GatewayName;

	internal readonly long AccountId;

	internal readonly long GrossAmount;

	internal readonly long NetAmount;

	internal readonly long PlatformFee;

	internal readonly long GatewayFee;

	internal readonly long Tax;

	internal readonly string TaxType;

	internal readonly string Currency;

	internal readonly long Tokens;

	internal readonly string TransactionType;

	internal readonly string MonetizationType;

	internal readonly string PurchaseDate;

	internal readonly string CreatedAt;

	internal readonly PaymentMethodObject[] PaymentMethod;

	internal readonly LineItemsObject[] LineItems;

	[JsonConstructor]
	public TransactionObject(long id, string gateway_uuid, string gateway_name, long account_id, long gross_amount, long net_amount, long platform_fee, long gateway_fee, long tax, string tax_type, string currency, long tokens, string transaction_type, string monetization_type, string purchase_date, string created_at, PaymentMethodObject[] payment_method, LineItemsObject[] line_items)
	{
		Id = id;
		GatewayUuid = gateway_uuid;
		GatewayName = gateway_name;
		AccountId = account_id;
		GrossAmount = gross_amount;
		NetAmount = net_amount;
		PlatformFee = platform_fee;
		GatewayFee = gateway_fee;
		Tax = tax;
		TaxType = tax_type;
		Currency = currency;
		Tokens = tokens;
		TransactionType = transaction_type;
		MonetizationType = monetization_type;
		PurchaseDate = purchase_date;
		CreatedAt = created_at;
		PaymentMethod = payment_method;
		LineItems = line_items;
	}
}
