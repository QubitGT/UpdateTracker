namespace Fusion.Sockets.Stun;

internal static class StunNatTypeExtensions
{
	public static bool IsValid(this NATType natType)
	{
		switch (natType)
		{
		case NATType.Invalid:
		case NATType.UdpBlocked:
			return false;
		case NATType.OpenInternet:
		case NATType.FullCone:
		case NATType.Symmetric:
			return true;
		default:
			return false;
		}
	}
}
