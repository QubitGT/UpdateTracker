using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class RangeExAttribute : DrawerPropertyAttribute
{
	public bool ClampMin = true;

	public bool ClampMax = true;

	public bool UseSlider = true;

	public double Max { get; }

	public double Min { get; }

	public RangeExAttribute(double min, double max)
	{
		Max = max;
		Min = min;
	}
}
