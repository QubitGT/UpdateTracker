using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field)]
public class HideArrayElementLabelAttribute : DecoratingPropertyAttribute
{
	private new const int DefaultOrder = -11000;

	public HideArrayElementLabelAttribute()
		: base(-11000)
	{
	}
}
