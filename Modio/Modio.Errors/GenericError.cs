namespace Modio.Errors;

public class GenericError : Error
{
	public new static readonly GenericError None = new GenericError(GenericErrorCode.NONE);

	public new GenericErrorCode Code => (GenericErrorCode)base.Code;

	public GenericError(GenericErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
