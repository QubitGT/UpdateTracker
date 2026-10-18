using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct PayObject
{
	internal readonly long TransactionId;

	internal readonly string GatewayUuid;

	internal readonly long GrossAmount;

	internal readonly long NetAmount;

	internal readonly long PlatformFee;

	internal readonly long GatewayFee;

	internal readonly string TransactionType;

	internal readonly JArray Meta;

	internal readonly long PurchaseDate;

	internal readonly string WalletType;

	internal readonly long Balance;

	internal readonly long Deficit;

	internal readonly string PaymentMethodId;

	internal readonly ModObject Mod;

	[JsonConstructor]
	public PayObject(long transaction_id, string gateway_uuid, long gross_amount, long net_amount, long platform_fee, long gateway_fee, string transaction_type, JArray meta, long purchase_date, string wallet_type, long balance, long deficit, string payment_method_id, ModObject mod)
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
		WalletType = wallet_type;
		Balance = balance;
		Deficit = deficit;
		PaymentMethodId = payment_method_id;
		Mod = mod;
	}
}
