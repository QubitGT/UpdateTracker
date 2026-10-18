using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class TrialOfferList : DeserializableList<TrialOffer>
{
	public TrialOfferList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_TrialOfferArray_GetSize(a);
		_Data = new List<TrialOffer>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new TrialOffer(CAPI.ovr_TrialOfferArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
	}
}
