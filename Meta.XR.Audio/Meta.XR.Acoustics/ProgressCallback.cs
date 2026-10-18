using System;
using System.Runtime.InteropServices;

namespace Meta.XR.Acoustics;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate bool ProgressCallback(IntPtr userData, string description, float progress);
