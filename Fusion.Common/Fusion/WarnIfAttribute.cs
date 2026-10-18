using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class WarnIfAttribute : DoIfAttributeBase
{
	public string Message;

	public bool AsBox;

	public WarnIfAttribute(string conditionMember, double compareToValue, string message, CompareOperator compare = CompareOperator.Equal)
		: base(conditionMember, compareToValue, compare)
	{
		base.order = -10000;
		Message = message;
	}

	public WarnIfAttribute(string conditionMember, bool compareToValue, string message, CompareOperator compare = CompareOperator.Equal)
		: base(conditionMember, compareToValue, compare)
	{
		base.order = -10000;
		Message = message;
	}

	public WarnIfAttribute(string conditionMember, long compareToValue, string message, CompareOperator compare = CompareOperator.Equal)
		: base(conditionMember, compareToValue, compare)
	{
		base.order = -10000;
		Message = message;
	}

	public WarnIfAttribute(string conditionMember, string message)
		: this(conditionMember, 0L, message, CompareOperator.NotEqual)
	{
	}
}
