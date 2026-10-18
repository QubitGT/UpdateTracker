namespace Modio.Errors;

public class TempModsError : Error
{
	public new static readonly TempModsError None = new TempModsError(TempModsErrorCode.NONE);

	public new TempModsErrorCode Code => (TempModsErrorCode)base.Code;

	public TempModsError(TempModsErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
