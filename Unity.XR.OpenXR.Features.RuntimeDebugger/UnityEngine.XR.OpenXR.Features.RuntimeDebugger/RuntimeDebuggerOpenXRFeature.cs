using System;
using System.Runtime.InteropServices;
using UnityEngine.Networking.PlayerConnection;

namespace UnityEngine.XR.OpenXR.Features.RuntimeDebugger;

public class RuntimeDebuggerOpenXRFeature : OpenXRFeature
{
	internal static readonly Guid kEditorToPlayerRequestDebuggerOutput = new Guid("B3E6DED1-C6C7-411C-BE58-86031A0877E7");

	internal static readonly Guid kPlayerToEditorSendDebuggerOutput = new Guid("B3E6DED1-C6C7-411C-BE58-86031A0877E8");

	public uint cacheSize = 1048576u;

	public uint perThreadCacheSize = 51200u;

	private uint lutOffset;

	private const string Library = "openxr_runtime_debugger";

	protected override IntPtr HookGetInstanceProcAddr(IntPtr func)
	{
		PlayerConnection.instance.Register(kEditorToPlayerRequestDebuggerOutput, RecvMsg);
		Native_StartDataAccess();
		Native_EndDataAccess();
		lutOffset = 0u;
		return Native_HookGetInstanceProcAddr(func, cacheSize, perThreadCacheSize);
	}

	internal void RecvMsg(MessageEventArgs args)
	{
		Native_StartDataAccess();
		Native_GetLUTData(out var ptr, out var size, lutOffset);
		byte[] array = new byte[size];
		if (size != 0)
		{
			lutOffset = size;
			Marshal.Copy(ptr, array, 0, (int)size);
		}
		Native_GetDataForRead(out var ptr2, out var size2);
		Native_GetDataForRead(out var ptr3, out var size3);
		byte[] array2 = new byte[size2 + size3];
		if (size2 != 0)
		{
			Marshal.Copy(ptr2, array2, 0, (int)size2);
		}
		if (size3 != 0)
		{
			Marshal.Copy(ptr3, array2, (int)size2, (int)size3);
		}
		Native_EndDataAccess();
		PlayerConnection.instance.Send(kPlayerToEditorSendDebuggerOutput, array);
		PlayerConnection.instance.Send(kPlayerToEditorSendDebuggerOutput, array2);
	}

	[DllImport("openxr_runtime_debugger", EntryPoint = "HookXrInstanceProcAddr")]
	private static extern IntPtr Native_HookGetInstanceProcAddr(IntPtr func, uint cacheSize, uint perThreadCacheSize);

	[DllImport("openxr_runtime_debugger", EntryPoint = "GetDataForRead")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Native_GetDataForRead(out IntPtr ptr, out uint size);

	[DllImport("openxr_runtime_debugger", EntryPoint = "GetLUTData")]
	private static extern void Native_GetLUTData(out IntPtr ptr, out uint size, uint offset);

	[DllImport("openxr_runtime_debugger", EntryPoint = "StartDataAccess")]
	private static extern void Native_StartDataAccess();

	[DllImport("openxr_runtime_debugger", EntryPoint = "EndDataAccess")]
	private static extern void Native_EndDataAccess();
}
