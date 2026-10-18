using System.Runtime.InteropServices;

namespace OVR.OpenVR;

public struct RenderModel_ControllerMode_State_t
{
	[MarshalAs(UnmanagedType.I1)]
	public bool bScrollWheelVisible;
}
