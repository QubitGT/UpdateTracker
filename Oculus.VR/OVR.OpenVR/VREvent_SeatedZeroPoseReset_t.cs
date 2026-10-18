using System.Runtime.InteropServices;

namespace OVR.OpenVR;

public struct VREvent_SeatedZeroPoseReset_t
{
	[MarshalAs(UnmanagedType.I1)]
	public bool bResetBySystemMenu;
}
