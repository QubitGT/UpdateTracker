namespace Fusion.Sockets.Stun;

internal class StunResult
{
	public NATType NatType = NATType.Invalid;

	public static readonly StunResult Invalid = new StunResult(NetAddress.AnyIPv4Addr, NetAddress.AnyIPv4Addr);

	public bool IsValid => PublicEndPoint.IsValid && PrivateEndPoint.IsValid;

	public NetAddress PublicEndPoint { get; private set; } = default(NetAddress);


	public NetAddress PrivateEndPoint { get; private set; } = default(NetAddress);


	private StunResult(NetAddress publicEndPoint = default(NetAddress), NetAddress privateEndPoint = default(NetAddress))
	{
		PublicEndPoint = publicEndPoint;
		PrivateEndPoint = privateEndPoint;
	}

	public static StunResult BuildStunResult(NetAddress publicEndPoint1, NetAddress publicEndPoint2, NetAddress privateEndPoint)
	{
		StunResult stunResult = new StunResult(publicEndPoint1, privateEndPoint)
		{
			NatType = NATType.Invalid
		};
		if (publicEndPoint1.Equals(NetAddress.AnyIPv4Addr) && publicEndPoint2.Equals(NetAddress.AnyIPv4Addr))
		{
			stunResult.NatType = NATType.UdpBlocked;
		}
		else if (publicEndPoint1.Equals(privateEndPoint))
		{
			stunResult.NatType = NATType.OpenInternet;
		}
		else if (publicEndPoint1.Equals(publicEndPoint2))
		{
			stunResult.NatType = NATType.FullCone;
		}
		else
		{
			stunResult.NatType = NATType.Symmetric;
		}
		return stunResult;
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}={2}, {3}={4}, {5}={6}]", "StunResult", "PublicEndPoint", PublicEndPoint, "PrivateEndPoint", PrivateEndPoint, "NatType", NatType);
	}
}
