namespace Fusion;

public abstract class DoIfAttributeBase : DecoratingPropertyAttribute
{
	public double _doubleValue;

	public bool _isDouble;

	public long _longValue;

	public CompareOperator Compare;

	public string ConditionMember;

	public bool ErrorOnConditionMemberNotFound = true;

	protected DoIfAttributeBase(string conditionMember, double compareToValue, CompareOperator compare)
	{
		ConditionMember = conditionMember;
		Compare = compare;
		_doubleValue = compareToValue;
		_isDouble = true;
	}

	protected DoIfAttributeBase(string conditionMember, long compareToValue, CompareOperator compare)
	{
		ConditionMember = conditionMember;
		Compare = compare;
		_longValue = compareToValue;
		_isDouble = false;
	}

	protected DoIfAttributeBase(string conditionMember, bool compareToValue, CompareOperator compare)
	{
		ConditionMember = conditionMember;
		Compare = compare;
		_longValue = (compareToValue ? 1 : 0);
		_isDouble = false;
	}
}
