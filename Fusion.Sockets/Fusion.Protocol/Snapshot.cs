using System;

namespace Fusion.Protocol;

internal class Snapshot : Message
{
	public int Tick { get; private set; }

	public uint NetworkID { get; private set; }

	public SnapshotType SnapshotType { get; private set; }

	public int TotalSize { get; private set; }

	public override bool IsValid => base.IsValid && CRC == ComputeCRC(Data, TotalSize);

	public byte[] Data { get; private set; }

	private ulong CRC { get; set; }

	public Snapshot()
	{
	}

	public Snapshot(int tick, uint networkID, SnapshotType snapshotType, int snapshotSize, byte[] data, ProtocolMessageVersion protocolVersion = ProtocolMessageVersion.V1_6_0, Version serializationVersion = null)
		: base(protocolVersion, serializationVersion)
	{
		Tick = tick;
		NetworkID = networkID;
		SnapshotType = snapshotType;
		TotalSize = snapshotSize;
		Data = data;
		CRC = ComputeCRC(Data, TotalSize);
	}

	protected override void SerializeProtected(BitStream stream)
	{
		int value = Tick;
		uint value2 = NetworkID;
		byte value3 = (byte)SnapshotType;
		ulong value4 = CRC;
		int value5 = TotalSize;
		byte[] array = Data;
		stream.Serialize(ref value);
		stream.Serialize(ref value2);
		stream.Serialize(ref value3);
		int value6 = 0;
		stream.Serialize(ref value6);
		stream.Serialize(ref value4);
		stream.Serialize(ref value5);
		stream.Serialize(ref array, ref value5);
		Tick = value;
		NetworkID = value2;
		SnapshotType = (SnapshotType)value3;
		CRC = value4;
		TotalSize = value5;
		Data = array;
	}

	private unsafe static ulong ComputeCRC(byte[] data, int length)
	{
		if (data == null)
		{
			return 0uL;
		}
		fixed (byte* data2 = data)
		{
			return CRC64.Compute(data2, length);
		}
	}

	public override Message Clone()
	{
		return new Snapshot();
	}

	public override string ToString()
	{
		return string.Format("[{0}: {1}={2}, {3}={4}, {5}={6}, {7}={8}, CRC={9}, {10}={11}, {12}]", "Snapshot", "Tick", Tick, "NetworkID", NetworkID, "SnapshotType", SnapshotType, "TotalSize", TotalSize, CRC, "IsValid", IsValid, base.ToString());
	}
}
