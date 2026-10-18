using System;

namespace Fusion.Protocol;

internal class NetworkConfigSync : Message
{
	public SyncType Type;

	public string NetworkConfig;

	public NetworkConfigSync()
	{
	}

	public NetworkConfigSync(SyncType type, string serializedNetworkConfig, ProtocolMessageVersion protocolVersion = ProtocolMessageVersion.V1_6_0, Version serializationVersion = null)
		: base(protocolVersion, serializationVersion)
	{
		Type = type;
		NetworkConfig = serializedNetworkConfig;
	}

	protected override void SerializeProtected(BitStream stream)
	{
		byte value = (byte)Type;
		stream.Serialize(ref value);
		stream.Serialize(ref NetworkConfig);
		Type = (SyncType)value;
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}={2}, {3}={4}, {5}]", "NetworkConfigSync", "Type", Type, "NetworkConfig", NetworkConfig, base.ToString());
	}
}
