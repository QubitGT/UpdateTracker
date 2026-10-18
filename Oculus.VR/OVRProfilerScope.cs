using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct OVRProfilerScope : IDisposable
{
	public OVRProfilerScope(string name)
	{
	}

	void IDisposable.Dispose()
	{
	}
}
