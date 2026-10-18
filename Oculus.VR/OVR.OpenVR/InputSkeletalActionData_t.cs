using System.Runtime.InteropServices;

namespace OVR.OpenVR;

public struct InputSkeletalActionData_t
{
	[MarshalAs(UnmanagedType.I1)]
	public bool bActive;

	public ulong activeOrigin;

	public uint boneCount;
}
