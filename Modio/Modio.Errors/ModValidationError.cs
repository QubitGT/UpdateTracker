namespace Modio.Errors;

public class ModValidationError : Error
{
	public new static readonly ModValidationError None = new ModValidationError(ModValidationErrorCode.NONE);

	public new ModValidationErrorCode Code => (ModValidationErrorCode)base.Code;

	public ModValidationError(ModValidationErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
