namespace Liv.Lck;

public class LckResult<T> : ILckResult
{
	private readonly bool _success;

	private readonly string _message;

	private readonly LckError? _error;

	private readonly T _result;

	public bool Success => _success;

	public string Message => _message;

	public LckError? Error => _error;

	public T Result => _result;

	private LckResult(bool success, string message, LckError? error, T result)
	{
		_success = success;
		_message = message;
		_error = error;
		_result = result;
	}

	internal static LckResult<T> NewSuccess(T result)
	{
		return new LckResult<T>(success: true, null, null, result);
	}

	internal static LckResult<T> NewError(LckError error, string message)
	{
		return new LckResult<T>(success: false, message, error, default(T));
	}
}
public class LckResult : ILckResult
{
	private readonly bool _success;

	private readonly string _message;

	private readonly LckError? _error;

	public bool Success => _success;

	public string Message => _message;

	public LckError? Error => _error;

	private LckResult(bool success, string message, LckError? error)
	{
		_success = success;
		_message = message;
		_error = error;
	}

	internal static LckResult NewSuccess()
	{
		return new LckResult(success: true, null, null);
	}

	internal static LckResult NewError(LckError error, string message)
	{
		return new LckResult(success: false, message, error);
	}
}
