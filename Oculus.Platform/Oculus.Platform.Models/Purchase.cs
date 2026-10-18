using System;

namespace Oculus.Platform.Models;

public class Purchase
{
	public readonly string DeveloperPayload;

	public readonly DateTime ExpirationTime;

	public readonly DateTime GrantTime;

	public readonly string ID;

	public readonly string ReportingId;

	public readonly string Sku;

	public readonly ProductType Type;

	public Purchase(IntPtr o)
	{
		DeveloperPayload = CAPI.ovr_Purchase_GetDeveloperPayload(o);
		ExpirationTime = CAPI.ovr_Purchase_GetExpirationTime(o);
		GrantTime = CAPI.ovr_Purchase_GetGrantTime(o);
		ID = CAPI.ovr_Purchase_GetPurchaseStrID(o);
		ReportingId = CAPI.ovr_Purchase_GetReportingId(o);
		Sku = CAPI.ovr_Purchase_GetSKU(o);
		Type = CAPI.ovr_Purchase_GetType(o);
	}
}
