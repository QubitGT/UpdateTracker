using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
[CallbackIdentity(2802)]
public struct SteamInputDeviceDisconnected_t
{
	public const int k_iCallback = 2802;

	public InputHandle_t m_ulDisconnectedDeviceHandle;
}
