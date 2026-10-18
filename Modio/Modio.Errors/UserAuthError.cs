namespace Modio.Errors;

public class UserAuthError : Error
{
	public new static readonly UserAuthError None = new UserAuthError(UserAuthErrorCode.NONE);

	public new UserAuthErrorCode Code => (UserAuthErrorCode)base.Code;

	public UserAuthError(UserAuthErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
