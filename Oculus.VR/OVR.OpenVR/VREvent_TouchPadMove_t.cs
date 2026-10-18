using System.Runtime.InteropServices;

namespace OVR.OpenVR;

public struct VREvent_TouchPadMove_t
{
	[MarshalAs(UnmanagedType.I1)]
	public bool bFingerDown;

	public float flSecondsFingerDown;

	public float fValueXFirst;

	public float fValueYFirst;

	public float fValueXRaw;

	public float fValueYRaw;
}
