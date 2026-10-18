using System;
using Fusion.Sockets;
using Fusion.Sockets.Stun;

namespace Fusion.Protocol;

internal class ReflexiveInfo : Message
{
	public int ActorNr;

	public NetAddress PublicAddr;

	public NetAddress PrivateAddr;

	public NATType NatType;

	public byte[] UniqueId;

	public override bool IsValid => base.IsValid && PublicAddr.IsValid && PrivateAddr.IsValid;

	public ReflexiveInfo()
	{
	}

	public ReflexiveInfo(int actorNr, NetAddress publicAddr, NetAddress privateAddr, NATType stunNatType, byte[] uniqueID = null, ProtocolMessageVersion protocolVersion = ProtocolMessageVersion.V1_6_0, Version serializationVersion = null)
		: base(protocolVersion, serializationVersion)
	{
		ActorNr = actorNr;
		PublicAddr = publicAddr;
		PrivateAddr = privateAddr;
		NatType = stunNatType;
		UniqueId = uniqueID;
	}

	protected override void SerializeProtected(BitStream stream)
	{
		stream.Serialize(ref ActorNr);
		PublicAddr.Serialize(stream);
		PrivateAddr.Serialize(stream);
		byte value = (byte)NatType;
		if ((int)ProtocolVersion >= 4)
		{
			stream.Serialize(ref value);
		}
		else
		{
			value = 4;
		}
		NatType = (NATType)value;
		if ((int)ProtocolVersion >= 6)
		{
			stream.Serialize(ref UniqueId);
		}
	}

	public override string ToString()
	{
		long num = ((UniqueId != null && UniqueId.Length == 8) ? BitConverter.ToInt64(UniqueId, 0) : 0);
		return string.Format("[{0}: {1}={2}, {3}={4}, {5}={6}, {7}={8}, {9}={10}, {11}]", "ReflexiveInfo", "ActorNr", ActorNr, "PublicAddr", PublicAddr, "PrivateAddr", PrivateAddr, "NatType", NatType, "UniqueId", num, base.ToString());
	}
}
