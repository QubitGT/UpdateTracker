using System;

namespace Fusion;

public class AssertException : Exception
{
	public AssertException()
	{
	}

	public AssertException(string msg)
		: base(msg)
	{
	}
}
