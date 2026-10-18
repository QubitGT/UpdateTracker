using System;
using System.Runtime.InteropServices;

namespace Steamworks;

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void SteamInputActionEventCallbackPointer(IntPtr SteamInputActionEvent);
