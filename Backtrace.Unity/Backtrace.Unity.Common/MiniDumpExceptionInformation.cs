using System;
using System.Runtime.InteropServices;
using Backtrace.Unity.Types;

namespace Backtrace.Unity.Common;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct MiniDumpExceptionInformation
{
	internal uint ThreadId;

	internal IntPtr ExceptionPointers;

	[MarshalAs(UnmanagedType.Bool)]
	internal bool ClientPointers;

	internal static MiniDumpExceptionInformation GetInstance(MinidumpException exceptionInfo)
	{
		MiniDumpExceptionInformation result = default(MiniDumpExceptionInformation);
		result.ThreadId = SystemHelper.GetCurrentThreadId();
		result.ClientPointers = false;
		result.ExceptionPointers = IntPtr.Zero;
		return result;
	}
}
