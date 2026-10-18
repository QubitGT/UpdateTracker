using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
[CallbackIdentity(2803)]
public struct SteamInputConfigurationLoaded_t
{
	public const int k_iCallback = 2803;

	public AppId_t m_unAppID;

	public InputHandle_t m_ulDeviceHandle;

	public CSteamID m_ulMappingCreator;

	public uint m_unMajorRevision;

	public uint m_unMinorRevision;

	[MarshalAs(UnmanagedType.I1)]
	public bool m_bUsesSteamInputAPI;

	[MarshalAs(UnmanagedType.I1)]
	public bool m_bUsesGamepadAPI;
}
