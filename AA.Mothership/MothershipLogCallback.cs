using System;

public class MothershipLogCallback : MothershipLogDelegateWrapper
{
	private readonly Action<MothershipLogLevel, string> _logFunction;

	public MothershipLogCallback(Action<MothershipLogLevel, string> logFunction)
	{
		swigCMemOwn = false;
		_logFunction = logFunction;
	}

	public override void OnLogCallback(MothershipLogLevel level, string message)
	{
		_logFunction?.Invoke(level, message);
	}
}
