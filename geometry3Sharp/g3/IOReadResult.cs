namespace g3;

public struct IOReadResult
{
	public static readonly IOReadResult Ok = new IOReadResult(IOCode.Ok, "");

	public IOCode code { get; set; }

	public string message { get; set; }

	public IOReadResult(IOCode r, string s)
	{
		this = default(IOReadResult);
		code = r;
		message = s;
		if (message == "")
		{
			message = "(no message)";
		}
	}
}
