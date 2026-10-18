using System;

namespace Oculus.Platform.Models;

public class PaidOffer
{
	public readonly Price Price;

	public readonly OfferTerm SubscriptionTerm;

	public PaidOffer(IntPtr o)
	{
		Price = new Price(CAPI.ovr_PaidOffer_GetPrice(o));
		SubscriptionTerm = CAPI.ovr_PaidOffer_GetSubscriptionTerm(o);
	}
}
