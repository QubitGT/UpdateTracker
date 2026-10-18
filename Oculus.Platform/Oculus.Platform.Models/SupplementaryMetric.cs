using System;

namespace Oculus.Platform.Models;

public class SupplementaryMetric
{
	public readonly ulong ID;

	public readonly long Metric;

	public SupplementaryMetric(IntPtr o)
	{
		ID = CAPI.ovr_SupplementaryMetric_GetID(o);
		Metric = CAPI.ovr_SupplementaryMetric_GetMetric(o);
	}
}
