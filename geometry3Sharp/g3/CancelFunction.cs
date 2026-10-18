using System;

namespace g3;

public class CancelFunction : ICancelSource
{
	public Func<bool> CancelF;

	public CancelFunction(Func<bool> cancelF)
	{
		CancelF = cancelF;
	}

	public bool Cancelled()
	{
		return CancelF();
	}
}
