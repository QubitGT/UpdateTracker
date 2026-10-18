using System;

public class MothershipAuthRefreshRequiredCallback : AuthRefreshRequiredDelegateWrapper
{
	private Action<string> _authRefreshFunction;

	public MothershipAuthRefreshRequiredCallback(Action<string> authRefreshFunction)
	{
		swigCMemOwn = false;
		_authRefreshFunction = authRefreshFunction;
	}

	public override void AuthRefreshRequired(string arg0)
	{
		if (_authRefreshFunction != null)
		{
			_authRefreshFunction(arg0);
		}
	}
}
