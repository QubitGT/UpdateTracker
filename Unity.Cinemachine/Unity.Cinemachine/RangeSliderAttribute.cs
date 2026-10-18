using System;
using UnityEngine;

namespace Unity.Cinemachine;

[Obsolete("Use RangeAttribute instead")]
public sealed class RangeSliderAttribute : PropertyAttribute
{
	public float Min;

	public float Max;

	public RangeSliderAttribute(float min, float max)
	{
		Min = min;
		Max = max;
	}
}
