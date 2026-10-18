namespace Modio.Errors;

public class MetricsError : Error
{
	public new static readonly MetricsError None = new MetricsError(MetricsErrorCode.NONE);

	public new MetricsErrorCode Code => (MetricsErrorCode)base.Code;

	public MetricsError(MetricsErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
