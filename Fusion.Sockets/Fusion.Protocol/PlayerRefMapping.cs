using System;

namespace Fusion.Protocol;

internal class PlayerRefMapping : Message
{
	public int ActorId;

	public int PlayerRef;

	public byte[] UniqueId;

	protected override void SerializeProtected(BitStream stream)
	{
		stream.Serialize(ref ActorId);
		stream.Serialize(ref PlayerRef);
		stream.Serialize(ref UniqueId);
	}

	public override string ToString()
	{
		long num = ((UniqueId != null && UniqueId.Length == 8) ? BitConverter.ToInt64(UniqueId, 0) : 0);
		return string.Format("[{0}: {1}={2}, {3}={4}, {5}={6}, {7}]", "PlayerRefMapping", "ActorId", ActorId, "PlayerRef", PlayerRef, "UniqueId", num, base.ToString());
	}
}
