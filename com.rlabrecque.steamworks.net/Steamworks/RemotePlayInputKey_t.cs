using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct RemotePlayInputKey_t
{
	public int m_eScancode;

	public uint m_unModifiers;

	public uint m_unKeycode;
}
