namespace Modio.Errors;

public class ModManagementError : Error
{
	public new static readonly ModManagementError None = new ModManagementError(ModManagementErrorCode.NONE);

	public new ModManagementErrorCode Code => (ModManagementErrorCode)base.Code;

	public ModManagementError(ModManagementErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
