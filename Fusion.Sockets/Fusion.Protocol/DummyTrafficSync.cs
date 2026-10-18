using System;

namespace Fusion.Protocol;

internal class DummyTrafficSync : Message
{
	private const int DummySendIntervalMax = 5000;

	private const int DummySendIntervalMin = 100;

	private const int DummySizeMax = 128;

	private const int DummySizeMin = 2;

	public int SendInterval { get; private set; } = 5000;


	public int Size { get; private set; } = 2;


	public override bool IsValid => base.IsValid && SendInterval >= 100 && SendInterval <= 5000 && Size >= 2 && Size <= 128;

	public DummyTrafficSync()
	{
	}

	public DummyTrafficSync(int sendInterval, int size, ProtocolMessageVersion protocolVersion = ProtocolMessageVersion.V1_6_0, Version serializationVersion = null)
		: base(protocolVersion, serializationVersion)
	{
		SendInterval = Math.Max(Math.Min(sendInterval, 5000), 100);
		Size = Math.Max(Math.Min(size, 128), 2);
	}

	protected override void SerializeProtected(BitStream stream)
	{
		int value = SendInterval;
		int value2 = Size;
		stream.Serialize(ref value);
		stream.Serialize(ref value2);
		SendInterval = Math.Max(Math.Min(value, 5000), 100);
		Size = Math.Max(Math.Min(value2, 128), 2);
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}={2}, {3}={4}, {5}]", "DummyTrafficSync", "SendInterval", SendInterval, "Size", Size, base.ToString());
	}
}
