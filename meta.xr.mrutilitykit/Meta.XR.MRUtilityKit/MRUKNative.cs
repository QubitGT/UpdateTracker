using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

internal static class MRUKNative
{
	private static IntPtr _nativeLibraryPtr;

	[DllImport("kernel32")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool FreeLibrary(IntPtr hModule);

	[DllImport("kernel32", CharSet = CharSet.Unicode)]
	private static extern IntPtr LoadLibrary(string lpFileName);

	[DllImport("kernel32")]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

	private static IntPtr GetDllHandle(string path)
	{
		return LoadLibrary(path);
	}

	private static IntPtr GetDllExport(IntPtr dllHandle, string name)
	{
		return GetProcAddress(dllHandle, name);
	}

	private static bool FreeDllHandle(IntPtr dllHandle)
	{
		return FreeLibrary(dllHandle);
	}

	internal static void LoadMRUKSharedLibrary()
	{
		if (!(_nativeLibraryPtr != IntPtr.Zero))
		{
			string empty = string.Empty;
			empty = Path.Join(Application.dataPath, "Plugins/x86_64/mrutilitykitshared.dll");
			_nativeLibraryPtr = GetDllHandle(empty);
			if (_nativeLibraryPtr == IntPtr.Zero)
			{
				Debug.LogError("Failed to load mr utility kit shared library from '" + empty + "'");
			}
			else
			{
				MRUKNativeFuncs.LoadNativeFunctions();
			}
		}
	}

	internal static void FreeMRUKSharedLibrary()
	{
		MRUKNativeFuncs.UnloadNativeFunctions();
		if (!(_nativeLibraryPtr == IntPtr.Zero))
		{
			if (!FreeDllHandle(_nativeLibraryPtr))
			{
				Debug.LogError("Failed to free mr utility kit shared library");
			}
			_nativeLibraryPtr = IntPtr.Zero;
		}
	}

	internal static T LoadFunction<T>(string name)
	{
		if (_nativeLibraryPtr == IntPtr.Zero)
		{
			Debug.LogWarning("Failed to load " + name + " because mr utility kit shared library is not loaded");
			return default(T);
		}
		IntPtr dllExport = GetDllExport(_nativeLibraryPtr, name);
		if (dllExport == IntPtr.Zero)
		{
			Debug.LogWarning("Could not find " + name + " in mr utility kit shared library");
			return default(T);
		}
		return Marshal.GetDelegateForFunctionPointer<T>(dllExport);
	}
}
