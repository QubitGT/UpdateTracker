namespace Modio.Errors;

public class HttpError : Error
{
	public new static readonly HttpError None = new HttpError(HttpErrorCode.NONE);

	public new HttpErrorCode Code => (HttpErrorCode)base.Code;

	public HttpError(HttpErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
