using System;

namespace Fusion.Protocol;

internal class Start : Message
{
	public int RemoteServerID;

	public StartRequests StartRequests;

	public Start()
	{
	}

	public Start(int remoteServerId, StartRequests startRequests, ProtocolMessageVersion protocolVersion = ProtocolMessageVersion.V1_6_0, Version serializationVersion = null)
		: base(protocolVersion, serializationVersion)
	{
		RemoteServerID = remoteServerId;
		StartRequests = startRequests;
	}

	protected override void SerializeProtected(BitStream stream)
	{
		uint value = (uint)StartRequests;
		stream.Serialize(ref RemoteServerID);
		stream.Serialize(ref value);
		StartRequests = (StartRequests)value;
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}={2}, {3}={4}, {5}]", "Start", "RemoteServerID", RemoteServerID, "StartRequests", StartRequests, base.ToString());
	}
}
