using System;

namespace Oculus.Platform.Models;

public class Price
{
	public readonly uint AmountInHundredths;

	public readonly string Currency;

	public readonly string Formatted;

	public Price(IntPtr o)
	{
		AmountInHundredths = CAPI.ovr_Price_GetAmountInHundredths(o);
		Currency = CAPI.ovr_Price_GetCurrency(o);
		Formatted = CAPI.ovr_Price_GetFormatted(o);
	}
}
