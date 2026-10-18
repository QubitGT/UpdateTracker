namespace Backtrace.Unity.Runtime.Native.Android;

internal enum UnwindingMode
{
	LOCAL,
	REMOTE,
	REMOTE_DUMPWITHOUTCRASH,
	LOCAL_DUMPWITHOUTCRASH,
	LOCAL_CONTEXT
}
