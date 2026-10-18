using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Backtrace.Unity.Types;
using UnityEngine;

namespace Backtrace.Unity.Common;

internal static class MinidumpHelper
{
	private static readonly string[] Libraries = new string[2] { "kernel32.dll", "dbghelp.dll" };

	private static bool IsMemoryDumpAvailable()
	{
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
		{
			return SystemHelper.IsLibraryAvailable(Libraries);
		}
		return false;
	}

	[DllImport("dbghelp.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
	internal static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, ref MiniDumpExceptionInformation expParam, IntPtr userStreamParam, IntPtr callbackParam);

	[DllImport("dbghelp.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
	internal static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);

	internal static bool Write(string filePath, MiniDumpType options = MiniDumpType.WithFullMemory, MinidumpException exceptionType = MinidumpException.None)
	{
		if (!IsMemoryDumpAvailable())
		{
			return false;
		}
		Process currentProcess = Process.GetCurrentProcess();
		IntPtr handle = currentProcess.Handle;
		uint id = (uint)currentProcess.Id;
		MiniDumpExceptionInformation expParam = MiniDumpExceptionInformation.GetInstance(exceptionType);
		using FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Write);
		return (expParam.ExceptionPointers == IntPtr.Zero) ? MiniDumpWriteDump(handle, id, fileStream.SafeFileHandle, (uint)options, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) : MiniDumpWriteDump(handle, id, fileStream.SafeFileHandle, (uint)options, ref expParam, IntPtr.Zero, IntPtr.Zero);
	}
}
