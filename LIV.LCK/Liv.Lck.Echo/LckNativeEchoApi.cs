using System;
using System.Runtime.InteropServices;
using Liv.Lck.Recorder;

namespace Liv.Lck.Echo;

internal static class LckNativeEchoApi
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void EchoCompletionCallback(uint status, [MarshalAs(UnmanagedType.LPStr)] string outputPath);

	private const string EncodingLib = "lck_rs";

	[DllImport("lck_rs")]
	internal static extern IntPtr CreateEchoMemoryBuffer();

	[DllImport("lck_rs")]
	internal static extern IntPtr CreateEchoDiskBuffer([MarshalAs(UnmanagedType.LPStr)] string storageDir);

	[DllImport("lck_rs")]
	internal static extern void DestroyEchoBuffer(IntPtr echoBufferContext);

	[DllImport("lck_rs")]
	internal static extern void SetEchoBufferEnabled(IntPtr echoBufferContext, bool enabled);

	[DllImport("lck_rs")]
	internal static extern bool IsEchoBufferEnabled(IntPtr echoBufferContext);

	[DllImport("lck_rs")]
	internal static extern void SetEchoMuxerConfig(IntPtr echoBufferContext, ref MuxerConfig config);

	[DllImport("lck_rs")]
	internal static extern bool TriggerEchoSave(IntPtr echoBufferContext, [MarshalAs(UnmanagedType.LPStr)] string outputPath);

	[DllImport("lck_rs")]
	internal static extern IntPtr GetEchoCallbackFunction();

	[DllImport("lck_rs")]
	internal static extern void SetEchoCompletionCallback(IntPtr echoBufferContext, EchoCompletionCallback callback);

	[DllImport("lck_rs")]
	internal static extern ulong GetEchoBufferDurationUs(IntPtr echoBufferContext);

	[DllImport("lck_rs")]
	internal static extern ulong GetEchoBufferDataSizeBytes(IntPtr echoBufferContext);

	[DllImport("lck_rs")]
	internal static extern void ClearEchoBuffer(IntPtr echoBufferContext);

	[DllImport("lck_rs")]
	internal static extern ulong GetEchoBufferMaxDuration(IntPtr echoBufferContext);
}
