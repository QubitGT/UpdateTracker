using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct RemotePlayInputMouseWheel_t
{
	public ERemotePlayMouseWheelDirection m_eDirection;

	public float m_flAmount;
}
