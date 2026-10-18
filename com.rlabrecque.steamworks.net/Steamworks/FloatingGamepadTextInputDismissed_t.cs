using System.Runtime.InteropServices;

namespace Steamworks;

[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 1)]
[CallbackIdentity(738)]
public struct FloatingGamepadTextInputDismissed_t
{
	public const int k_iCallback = 738;
}
