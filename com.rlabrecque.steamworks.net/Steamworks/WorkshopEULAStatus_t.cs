using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
[CallbackIdentity(3420)]
public struct WorkshopEULAStatus_t
{
	public const int k_iCallback = 3420;

	public EResult m_eResult;

	public AppId_t m_nAppID;

	public uint m_unVersion;

	public RTime32 m_rtAction;

	[MarshalAs(UnmanagedType.I1)]
	public bool m_bAccepted;

	[MarshalAs(UnmanagedType.I1)]
	public bool m_bNeedsAction;
}
