using System;

namespace Oculus.Platform.Models;

public class Product
{
	public readonly BillingPlanList BillingPlansOptional;

	[Obsolete("Deprecated in favor of BillingPlansOptional")]
	public readonly BillingPlanList BillingPlans;

	public readonly ContentRating ContentRatingOptional;

	[Obsolete("Deprecated in favor of ContentRatingOptional")]
	public readonly ContentRating ContentRating;

	public readonly string CoverUrl;

	public readonly string Description;

	public readonly string FormattedPrice;

	public readonly string IconUrl;

	public readonly string Name;

	public readonly Price Price;

	public readonly string ShortDescription;

	public readonly string Sku;

	public readonly ProductType Type;

	public Product(IntPtr o)
	{
		IntPtr intPtr = CAPI.ovr_Product_GetBillingPlans(o);
		BillingPlans = new BillingPlanList(intPtr);
		if (intPtr == IntPtr.Zero)
		{
			BillingPlansOptional = null;
		}
		else
		{
			BillingPlansOptional = BillingPlans;
		}
		IntPtr intPtr2 = CAPI.ovr_Product_GetContentRating(o);
		ContentRating = new ContentRating(intPtr2);
		if (intPtr2 == IntPtr.Zero)
		{
			ContentRatingOptional = null;
		}
		else
		{
			ContentRatingOptional = ContentRating;
		}
		CoverUrl = CAPI.ovr_Product_GetCoverUrl(o);
		Description = CAPI.ovr_Product_GetDescription(o);
		FormattedPrice = CAPI.ovr_Product_GetFormattedPrice(o);
		IconUrl = CAPI.ovr_Product_GetIconUrl(o);
		Name = CAPI.ovr_Product_GetName(o);
		Price = new Price(CAPI.ovr_Product_GetPrice(o));
		ShortDescription = CAPI.ovr_Product_GetShortDescription(o);
		Sku = CAPI.ovr_Product_GetSKU(o);
		Type = CAPI.ovr_Product_GetType(o);
	}
}
