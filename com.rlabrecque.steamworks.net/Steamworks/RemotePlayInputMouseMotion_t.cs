using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct RemotePlayInputMouseMotion_t
{
	[MarshalAs(UnmanagedType.I1)]
	public bool m_bAbsolute;

	public float m_flNormalizedX;

	public float m_flNormalizedY;

	public int m_nDeltaX;

	public int m_nDeltaY;
}
