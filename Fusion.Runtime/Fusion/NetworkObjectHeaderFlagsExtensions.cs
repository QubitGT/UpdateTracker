using System.Runtime.CompilerServices;

namespace Fusion;

internal static class NetworkObjectHeaderFlagsExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool CheckFlag(this NetworkObjectHeaderFlags flag, NetworkObjectHeaderFlags value)
	{
		return (flag & value) == value;
	}
}
