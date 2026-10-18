using System;

namespace Fusion.Protocol;

internal class Disconnect : Message
{
	public DisconnectReason DisconnectReason;

	public Disconnect()
	{
	}

	public Disconnect(DisconnectReason reason, ProtocolMessageVersion protocolVersion = ProtocolMessageVersion.V1_6_0, Version serializationVersion = null)
		: base(protocolVersion, serializationVersion)
	{
		DisconnectReason = reason;
	}

	protected override void SerializeProtected(BitStream stream)
	{
		string value = DisconnectReason.ToString();
		byte value2 = (byte)DisconnectReason;
		if ((int)ProtocolVersion >= 2)
		{
			stream.Serialize(ref value2);
		}
		else
		{
			stream.Serialize(ref value);
		}
		DisconnectReason = (DisconnectReason)value2;
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}={2}, {3}]", "Disconnect", "DisconnectReason", DisconnectReason, base.ToString());
	}
}
