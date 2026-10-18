namespace Modio.Errors;

public class MonetizationError : Error
{
	public new static readonly MonetizationError None = new MonetizationError(MonetizationErrorCode.NONE);

	public new MonetizationErrorCode Code => (MonetizationErrorCode)base.Code;

	public MonetizationError(MonetizationErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
