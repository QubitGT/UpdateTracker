using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class ValidateGooglePlayPurchaseRequest : PlayFabRequestCommon
{
	public string CatalogVersion;

	public string CurrencyCode;

	public uint? PurchasePrice;

	public string ReceiptJson;

	public string Signature;
}
