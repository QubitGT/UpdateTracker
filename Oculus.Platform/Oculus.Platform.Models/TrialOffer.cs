using System;

namespace Oculus.Platform.Models;

public class TrialOffer
{
	public readonly int MaxTermCount;

	public readonly Price Price;

	public readonly OfferTerm TrialTerm;

	public readonly OfferType TrialType;

	public TrialOffer(IntPtr o)
	{
		MaxTermCount = CAPI.ovr_TrialOffer_GetMaxTermCount(o);
		Price = new Price(CAPI.ovr_TrialOffer_GetPrice(o));
		TrialTerm = CAPI.ovr_TrialOffer_GetTrialTerm(o);
		TrialType = CAPI.ovr_TrialOffer_GetTrialType(o);
	}
}
