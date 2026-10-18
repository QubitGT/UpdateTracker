namespace Modio.Errors;

public class ApiError : Error
{
	public new static readonly ApiError None = new ApiError(ApiErrorCode.NONE);

	public new ApiErrorCode Code => (ApiErrorCode)base.Code;

	public ApiError(ApiErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
