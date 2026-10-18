using System.Runtime.InteropServices;
using System.Text;

namespace OVR.OpenVR;

public struct IVRDriverManager
{
	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate uint _GetDriverCount();

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate uint _GetDriverName(uint nDriver, StringBuilder pchValue, uint unBufferSize);

	[UnmanagedFunctionPointer(CallingConvention.StdCall)]
	internal delegate ulong _GetDriverHandle(string pchDriverName);

	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetDriverCount GetDriverCount;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetDriverName GetDriverName;

	[MarshalAs(UnmanagedType.FunctionPtr)]
	internal _GetDriverHandle GetDriverHandle;
}
