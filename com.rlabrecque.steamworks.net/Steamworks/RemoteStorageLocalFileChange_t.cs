using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 1)]
[CallbackIdentity(1333)]
public struct RemoteStorageLocalFileChange_t
{
	public const int k_iCallback = 1333;
}
