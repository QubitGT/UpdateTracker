namespace Modio.Errors;

public enum ArchiveErrorCode : long
{
	NONE = 0L,
	UNKNOWN = -2147483648L,
	INVALID_HEADER = -2147483600L,
	UNSUPPORTED_COMPRESSION = -2147483599L
}
