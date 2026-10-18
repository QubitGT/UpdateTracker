using System;

namespace g3;

public class gException : Exception
{
	public gException(string sMessage)
		: base(sMessage)
	{
	}

	public gException(string text, object arg0)
		: base(string.Format(text, arg0))
	{
	}

	public gException(string text, object arg0, object arg1)
		: base(string.Format(text, arg0, arg1))
	{
	}

	public gException(string text, params object[] args)
		: base(string.Format(text, args))
	{
	}
}
