namespace Fusion;

public abstract class DecoratingPropertyAttribute : PropertyAttribute
{
	public const int DefaultOrder = -10000;

	protected DecoratingPropertyAttribute()
	{
		base.order = -10000;
	}

	protected DecoratingPropertyAttribute(int order)
	{
		base.order = order;
	}
}
