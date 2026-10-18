using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = true)]
public class DrawIfAttribute : DoIfAttributeBase
{
	private new const int DefaultOrder = -11000;

	public DrawIfMode Mode;

	public bool Hide
	{
		get
		{
			return Mode == DrawIfMode.Hide;
		}
		set
		{
			Mode = (value ? DrawIfMode.Hide : DrawIfMode.ReadOnly);
		}
	}

	public DrawIfAttribute(string conditionMember, double compareToValue, CompareOperator compare = CompareOperator.Equal, DrawIfMode mode = DrawIfMode.ReadOnly)
		: base(conditionMember, compareToValue, compare)
	{
		Mode = mode;
		base.order = -11000;
	}

	public DrawIfAttribute(string conditionMember, bool compareToValue, CompareOperator compare = CompareOperator.Equal, DrawIfMode mode = DrawIfMode.ReadOnly)
		: base(conditionMember, compareToValue, compare)
	{
		Mode = mode;
		base.order = -11000;
	}

	public DrawIfAttribute(string conditionMember, long compareToValue, CompareOperator compare = CompareOperator.Equal, DrawIfMode mode = DrawIfMode.ReadOnly)
		: base(conditionMember, compareToValue, compare)
	{
		Mode = mode;
		base.order = -11000;
	}

	public DrawIfAttribute(string conditionMember)
		: this(conditionMember, 0L, CompareOperator.NotEqual)
	{
	}
}
