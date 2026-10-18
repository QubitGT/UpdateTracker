using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
public readonly struct EntitlementFulfillmentObject
{
	internal readonly string TransactionId;

	internal readonly long TransactionState;

	internal readonly string SkuId;

	internal readonly bool EntitlementConsumed;

	internal readonly long EntitlementType;

	internal readonly EntitlementDetailsObject Details;

	[JsonConstructor]
	public EntitlementFulfillmentObject(string transaction_id, long transaction_state, string sku_id, bool entitlement_consumed, long entitlement_type, EntitlementDetailsObject details)
	{
		TransactionId = transaction_id;
		TransactionState = transaction_state;
		SkuId = sku_id;
		EntitlementConsumed = entitlement_consumed;
		EntitlementType = entitlement_type;
		Details = details;
	}
}
