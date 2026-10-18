using System.Runtime.CompilerServices;

namespace Fusion;

public static class NetworkObjectFlagsExtensions
{
	private const NetworkObjectFlags CurrentVersion = NetworkObjectFlags.V1;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetVersion(this NetworkObjectFlags flags)
	{
		return (int)(flags & NetworkObjectFlags.MaskVersion);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsVersionCurrent(this NetworkObjectFlags flags)
	{
		return 1 == flags.GetVersion();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static NetworkObjectFlags SetCurrentVersion(this NetworkObjectFlags flags)
	{
		return SetWithMask(flags, NetworkObjectFlags.V1, NetworkObjectFlags.MaskVersion);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsIgnored(this NetworkObjectFlags flags)
	{
		return (flags & NetworkObjectFlags.Ignore) == NetworkObjectFlags.Ignore;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static NetworkObjectFlags SetIgnored(this NetworkObjectFlags flags, bool value)
	{
		if (value)
		{
			return flags | NetworkObjectFlags.Ignore;
		}
		return flags & ~NetworkObjectFlags.Ignore;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static NetworkObjectFlags SetWithMask(NetworkObjectFlags flags, NetworkObjectFlags value, NetworkObjectFlags mask)
	{
		flags &= ~mask;
		flags |= value;
		return flags;
	}
}
