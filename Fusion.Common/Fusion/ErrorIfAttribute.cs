using System;

namespace Fusion;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class ErrorIfAttribute : DoIfAttributeBase
{
	public string Message;

	public bool AsBox;

	public ErrorIfAttribute(string conditionMember, double compareToValue, string message, CompareOperator compare = CompareOperator.Equal)
		: base(conditionMember, compareToValue, compare)
	{
		base.order = -10000;
		Message = message;
	}

	public ErrorIfAttribute(string conditionMember, bool compareToValue, string message, CompareOperator compare = CompareOperator.Equal)
		: base(conditionMember, compareToValue, compare)
	{
		base.order = -10000;
		Message = message;
	}

	public ErrorIfAttribute(string conditionMember, long compareToValue, string message, CompareOperator compare = CompareOperator.Equal)
		: base(conditionMember, compareToValue, compare)
	{
		base.order = -10000;
		Message = message;
	}
}
