namespace Modio.Errors;

public class SystemError : Error
{
	public new static readonly SystemError None = new SystemError(SystemErrorCode.NONE);

	public new SystemErrorCode Code => (SystemErrorCode)base.Code;

	public SystemError(SystemErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
