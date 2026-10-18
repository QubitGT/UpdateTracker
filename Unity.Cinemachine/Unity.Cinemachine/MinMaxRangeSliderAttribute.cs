using UnityEngine;

namespace Unity.Cinemachine;

public sealed class MinMaxRangeSliderAttribute : PropertyAttribute
{
	public float Min;

	public float Max;

	public MinMaxRangeSliderAttribute(float min, float max)
	{
		Min = min;
		Max = max;
	}
}
