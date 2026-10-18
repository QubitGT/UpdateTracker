using System;

namespace g3;

public class ProgressCancel
{
	public ICancelSource Source;

	private bool WasCancelled;

	public ProgressCancel(ICancelSource source)
	{
		Source = source;
	}

	public ProgressCancel(Func<bool> cancelF)
	{
		Source = new CancelFunction(cancelF);
	}

	public bool Cancelled()
	{
		if (WasCancelled)
		{
			return true;
		}
		WasCancelled = Source.Cancelled();
		return WasCancelled;
	}
}
