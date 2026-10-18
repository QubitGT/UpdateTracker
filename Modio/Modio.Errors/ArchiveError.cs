namespace Modio.Errors;

public class ArchiveError : Error
{
	public new static readonly ArchiveError None = new ArchiveError(ArchiveErrorCode.NONE);

	public new ArchiveErrorCode Code => (ArchiveErrorCode)base.Code;

	public ArchiveError(ArchiveErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
