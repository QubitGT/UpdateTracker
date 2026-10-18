namespace g3;

public struct IOWriteResult
{
	public static readonly IOWriteResult Ok = new IOWriteResult(IOCode.Ok, "");

	public IOCode code { get; set; }

	public string message { get; set; }

	public IOWriteResult(IOCode r, string s)
	{
		this = default(IOWriteResult);
		code = r;
		message = s;
		if (message == "")
		{
			message = "(no message)";
		}
	}
}
