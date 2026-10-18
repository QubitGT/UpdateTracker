using System;

namespace Fusion.Protocol;

internal class HostMigration : Message
{
	public PeerMode PeerMode;

	public HostMigration()
	{
	}

	public HostMigration(PeerMode peerMode, ProtocolMessageVersion protocolVersion = ProtocolMessageVersion.V1_6_0, Version serializationVersion = null)
		: base(protocolVersion, serializationVersion)
	{
		PeerMode = peerMode;
	}

	protected override void SerializeProtected(BitStream stream)
	{
		byte value = (byte)PeerMode;
		stream.Serialize(ref value);
		PeerMode = (PeerMode)value;
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}={2}, {3}]", "HostMigration", "PeerMode", PeerMode, base.ToString());
	}
}
