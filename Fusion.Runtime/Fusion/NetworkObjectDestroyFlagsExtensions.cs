namespace Fusion;

internal static class NetworkObjectDestroyFlagsExtensions
{
	public static bool Get(this NetworkObjectDestroyFlags flags, NetworkObjectDestroyFlags flag)
	{
		return (flags & flag) == flag;
	}
}
