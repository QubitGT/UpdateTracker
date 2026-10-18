using System;

namespace UnityEngine.Animations.Rigging;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public sealed class WeightRangeAttribute : PropertyAttribute
{
	public readonly float min;

	public readonly float max = 1f;

	public WeightRangeAttribute(float min, float max)
	{
		this.min = min;
		this.max = max;
	}
}
