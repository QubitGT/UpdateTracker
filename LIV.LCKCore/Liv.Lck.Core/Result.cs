namespace Liv.Lck.Core;

public class Result<T>
{
	private readonly bool _success;

	private readonly string _message;

	private readonly CoreError? _error;

	private readonly T _result;

	public bool IsOk => _success;

	public string Message => _message;

	public CoreError? Err => _error;

	public T Ok => _result;

	private Result(bool success, string message, CoreError? error, T result)
	{
		_success = success;
		_message = message;
		_error = error;
		_result = result;
	}

	public static Result<T> NewSuccess(T result)
	{
		return new Result<T>(success: true, null, null, result);
	}

	public static Result<T> NewError(CoreError error, string message)
	{
		return new Result<T>(success: false, message, error, default(T));
	}
}
