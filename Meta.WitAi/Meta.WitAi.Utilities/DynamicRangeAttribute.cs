using UnityEngine;

namespace Meta.WitAi.Utilities;

public class DynamicRangeAttribute : PropertyAttribute
{
	public string RangeProperty { get; private set; }

	public float DefaultMin { get; private set; }

	public float DefaultMax { get; private set; }

	public DynamicRangeAttribute(string rangeProperty, float defaultMin = float.MinValue, float defaultMax = float.MaxValue)
	{
		DefaultMin = defaultMin;
		DefaultMax = defaultMax;
		RangeProperty = rangeProperty;
	}
}
