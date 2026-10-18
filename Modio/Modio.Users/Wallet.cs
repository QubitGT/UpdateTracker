using Modio.API.SchemaDefinitions;

namespace Modio.Users;

public class Wallet
{
	public string Type { get; private set; }

	public string Currency { get; private set; }

	public long Balance { get; private set; }

	internal Wallet()
	{
	}

	internal void ApplyDetailsFromWalletObject(WalletObject walletObject)
	{
		Type = walletObject.Type;
		Currency = walletObject.Currency;
		Balance = walletObject.Balance;
	}

	internal void UpdateBalance(long newBalance)
	{
		Balance = newBalance;
	}
}
