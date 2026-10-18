using System;
using System.Runtime.InteropServices;

namespace Meta.XR.Acoustics;

public struct SceneIRCallbacks
{
	public IntPtr userData;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	public ProgressCallback progress;
}
