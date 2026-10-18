namespace Modio.Errors;

public class FilesystemError : Error
{
	public new static readonly FilesystemError None = new FilesystemError(FilesystemErrorCode.NONE);

	public new FilesystemErrorCode Code => (FilesystemErrorCode)base.Code;

	public FilesystemError(FilesystemErrorCode code)
		: base((ErrorCode)code)
	{
	}
}
