using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct WalletObject
{
	internal readonly string Type;

	internal readonly string PaymentMethodId;

	internal readonly string GameId;

	internal readonly string Currency;

	internal readonly long Balance;

	internal readonly long PendingBalance;

	internal readonly long Deficit;

	internal readonly long MonetizationStatus;

	[JsonConstructor]
	public WalletObject(string type, string payment_method_id, string game_id, string currency, long balance, long pending_balance, long deficit, long monetization_status)
	{
		Type = type;
		PaymentMethodId = payment_method_id;
		GameId = game_id;
		Currency = currency;
		Balance = balance;
		PendingBalance = pending_balance;
		Deficit = deficit;
		MonetizationStatus = monetization_status;
	}
}
