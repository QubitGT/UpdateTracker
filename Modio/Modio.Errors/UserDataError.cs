namespace Modio.Errors;

public class UserDataError : Error
{
	public new static readonly UserDataError None = new UserDataError(UserDataErrorCode.NONE);

	public new UserDataErrorCode Code => (UserDataErrorCode)base.Code;

	public UserDataError(UserDataErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
