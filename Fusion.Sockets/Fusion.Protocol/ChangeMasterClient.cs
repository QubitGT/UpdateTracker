using System;

namespace Fusion.Protocol;

internal class ChangeMasterClient : Message
{
	public int NewMasterClientCandidate;

	public ChangeMasterClient()
	{
	}

	public ChangeMasterClient(int newMasterClientCandidate, ProtocolMessageVersion protocolVersion = ProtocolMessageVersion.V1_6_0, Version serializationVersion = null)
		: base(protocolVersion, serializationVersion)
	{
		NewMasterClientCandidate = newMasterClientCandidate;
	}

	protected override void SerializeProtected(BitStream stream)
	{
		stream.Serialize(ref NewMasterClientCandidate);
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}={2}, {3}]", "ChangeMasterClient", "NewMasterClientCandidate", NewMasterClientCandidate, base.ToString());
	}
}
