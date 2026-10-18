using System;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class BillingPlanList : DeserializableList<BillingPlan>
{
	public BillingPlanList(IntPtr a)
	{
		int num = (int)(uint)CAPI.ovr_BillingPlanArray_GetSize(a);
		_Data = new List<BillingPlan>(num);
		for (int i = 0; i < num; i++)
		{
			_Data.Add(new BillingPlan(CAPI.ovr_BillingPlanArray_GetElement(a, (UIntPtr)(ulong)i)));
		}
	}
}
