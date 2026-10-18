using Newtonsoft.Json;

namespace Modio.API.SchemaDefinitions;

[JsonObject]
internal readonly struct WalletBalanceObject
{
	internal readonly long Balance;

	[JsonConstructor]
	public WalletBalanceObject(long balance)
	{
		Balance = balance;
	}
}
