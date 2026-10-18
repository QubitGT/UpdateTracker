namespace Liv.Lck;

public interface ILckResult
{
	bool Success { get; }

	string Message { get; }

	LckError? Error { get; }
}
