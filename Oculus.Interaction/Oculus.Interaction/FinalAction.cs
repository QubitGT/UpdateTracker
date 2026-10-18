using System;

namespace Oculus.Interaction;

public class FinalAction
{
	private readonly Action _action;

	private bool _cancelled;

	public FinalAction(Action action)
	{
		_action = action;
	}

	public void Cancel()
	{
		_cancelled = true;
	}

	~FinalAction()
	{
		if (!_cancelled)
		{
			Context.ExecuteOnMainThread(_action);
		}
	}
}
