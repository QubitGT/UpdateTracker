using System;

namespace Oculus.Platform.Models;

public class BillingPlan
{
	public readonly PaidOffer PaidOffer;

	public readonly TrialOfferList TrialOffersOptional;

	[Obsolete("Deprecated in favor of TrialOffersOptional")]
	public readonly TrialOfferList TrialOffers;

	public BillingPlan(IntPtr o)
	{
		PaidOffer = new PaidOffer(CAPI.ovr_BillingPlan_GetPaidOffer(o));
		IntPtr intPtr = CAPI.ovr_BillingPlan_GetTrialOffers(o);
		TrialOffers = new TrialOfferList(intPtr);
		if (intPtr == IntPtr.Zero)
		{
			TrialOffersOptional = null;
		}
		else
		{
			TrialOffersOptional = TrialOffers;
		}
	}
}
