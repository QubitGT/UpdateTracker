namespace Meta.Voice.UnityOpus;

public enum ErrorCode
{
	OK = 0,
	BadArg = -1,
	BufferTooSmall = -2,
	InternalError = -3,
	InvalidPacket = -4,
	Unimplemented = -5,
	InvalidState = -6,
	AllocFail = -7
}
