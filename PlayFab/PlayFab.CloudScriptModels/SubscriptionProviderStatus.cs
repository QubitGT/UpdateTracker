namespace PlayFab.CloudScriptModels;

public enum SubscriptionProviderStatus
{
	NoError,
	Cancelled,
	UnknownError,
	BillingError,
	ProductUnavailable,
	CustomerDidNotAcceptPriceChange,
	FreeTrial,
	PaymentPending
}
