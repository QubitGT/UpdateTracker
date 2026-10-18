namespace Modio.Errors;

public class ZlibError : Error
{
	public new static readonly ZlibError None = new ZlibError(ZlibErrorCode.NONE);

	public new ZlibErrorCode Code => (ZlibErrorCode)base.Code;

	public ZlibError(ZlibErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
