namespace Fusion;

internal static class NetworkObjectRuntimeFlagsExtensions
{
	public static bool CheckFlag(this NetworkObjectRuntimeFlags flags, NetworkObjectRuntimeFlags flag)
	{
		return (flags & flag) == flag;
	}
}
