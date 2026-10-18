namespace Liv.Lck.ErrorHandling;

internal struct LckCaptureError
{
	public CaptureErrorType Type { get; set; }

	public string Message { get; set; }

	public LckCaptureError(CaptureErrorType type, string message)
	{
		Type = type;
		Message = message;
	}
}
