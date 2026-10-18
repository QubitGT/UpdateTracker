using System.Collections.Generic;

namespace Unity.Cinemachine;

internal class Joiner
{
	public int idx;

	public OutPt op1;

	public OutPt? op2;

	public Joiner? next1;

	public Joiner? next2;

	public Joiner? nextH;

	public Joiner(List<Joiner?>? joinerList, OutPt op1, OutPt? op2, Joiner? nextH)
	{
		if (joinerList != null)
		{
			idx = joinerList.Count;
			joinerList.Add(this);
		}
		else
		{
			idx = -1;
		}
		this.nextH = nextH;
		this.op1 = op1;
		this.op2 = op2;
		next1 = op1.joiner;
		op1.joiner = this;
		if (op2 != null)
		{
			next2 = op2.joiner;
			op2.joiner = this;
		}
		else
		{
			next2 = null;
		}
	}
}
