using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
[CallbackIdentity(331)]
public struct GameOverlayActivated_t
{
	public const int k_iCallback = 331;

	public byte m_bActive;

	[MarshalAs(UnmanagedType.I1)]
	public bool m_bUserInitiated;

	public AppId_t m_nAppID;

	public uint m_dwOverlayPID;
}
