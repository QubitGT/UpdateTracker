namespace Meta.WitAi.Requests;

internal struct VRequestResponse<TValue>
{
	public readonly TValue Value;

	public int Code;

	public string Error;

	public VRequestResponse(TValue value)
		: this(value, 200, string.Empty)
	{
	}

	public VRequestResponse(int code, string error)
		: this(default(TValue), code, error)
	{
	}

	public VRequestResponse(TValue value, int code, string error)
	{
		Value = value;
		Code = code;
		Error = error;
	}
}
