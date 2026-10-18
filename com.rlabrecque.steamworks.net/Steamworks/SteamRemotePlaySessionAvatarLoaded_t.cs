using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
[CallbackIdentity(5704)]
public struct SteamRemotePlaySessionAvatarLoaded_t
{
	public const int k_iCallback = 5704;

	public RemotePlaySessionID_t m_unSessionID;

	public int m_iImage;

	public int m_iWide;

	public int m_iTall;
}
