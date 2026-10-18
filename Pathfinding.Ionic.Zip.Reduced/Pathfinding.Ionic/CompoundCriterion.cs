using System;
using System.Text;
using Pathfinding.Ionic.Zip;

namespace Pathfinding.Ionic;

internal class CompoundCriterion : SelectionCriterion
{
	internal LogicalConjunction Conjunction;

	internal SelectionCriterion Left;

	private SelectionCriterion _Right;

	internal SelectionCriterion Right
	{
		get
		{
			return _Right;
		}
		set
		{
			_Right = value;
			if (value == null)
			{
				Conjunction = LogicalConjunction.NONE;
			}
			else if (Conjunction == LogicalConjunction.NONE)
			{
				Conjunction = LogicalConjunction.AND;
			}
		}
	}

	internal override bool Evaluate(string filename)
	{
		bool flag = Left.Evaluate(filename);
		switch (Conjunction)
		{
		case LogicalConjunction.AND:
			if (flag)
			{
				flag = Right.Evaluate(filename);
			}
			break;
		case LogicalConjunction.OR:
			if (!flag)
			{
				flag = Right.Evaluate(filename);
			}
			break;
		case LogicalConjunction.XOR:
			flag ^= Right.Evaluate(filename);
			break;
		default:
			throw new ArgumentException("Conjunction");
		}
		return flag;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(").Append((Left == null) ? "null" : Left.ToString()).Append(" ")
			.Append(Conjunction.ToString())
			.Append(" ")
			.Append((Right == null) ? "null" : Right.ToString())
			.Append(")");
		return stringBuilder.ToString();
	}

	internal override bool Evaluate(ZipEntry entry)
	{
		bool flag = Left.Evaluate(entry);
		switch (Conjunction)
		{
		case LogicalConjunction.AND:
			if (flag)
			{
				flag = Right.Evaluate(entry);
			}
			break;
		case LogicalConjunction.OR:
			if (!flag)
			{
				flag = Right.Evaluate(entry);
			}
			break;
		case LogicalConjunction.XOR:
			flag ^= Right.Evaluate(entry);
			break;
		}
		return flag;
	}
}
