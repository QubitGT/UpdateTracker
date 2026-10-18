using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
[CallbackIdentity(1223)]
public struct SteamNetworkingFakeIPResult_t
{
	public const int k_iCallback = 1223;

	public EResult m_eResult;

	public SteamNetworkingIdentity m_identity;

	public uint m_unIP;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
	public ushort[] m_unPorts;
}
