using System.Runtime.InteropServices;

namespace Fusion.Sockets;

[StructLayout(LayoutKind.Explicit)]
internal struct NetNotifyHeader
{
	public const int SIZE_IN_BYTES = 14;

	public const int SIZE_IN_BITS = 112;

	[FieldOffset(0)]
	public NetPacketType PacketType;

	[FieldOffset(1)]
	public byte Fragment;

	[FieldOffset(2)]
	public ushort Sequence;

	[FieldOffset(4)]
	public ushort AckSequence;

	[FieldOffset(6)]
	public ulong AckMask;

	public unsafe override string ToString()
	{
		ulong ackMask = AckMask;
		return $"[Type: {PacketType} Frag:{Fragment} Seq:{Sequence}, AckSeq:{AckSequence}, AckMask:{Maths.PrintBits((byte*)(&ackMask), 8)}]";
	}

	public static NetNotifyHeader CreateData(ushort sequence, ushort ackSequence, ulong ackMask)
	{
		NetNotifyHeader result = default(NetNotifyHeader);
		result.PacketType = NetPacketType.NotifyData;
		result.Sequence = sequence;
		result.AckSequence = ackSequence;
		result.AckMask = ackMask;
		result.Fragment = 0;
		return result;
	}

	public static NetNotifyHeader CreateAcks(ushort ackSequence, ulong ackMask)
	{
		NetNotifyHeader result = default(NetNotifyHeader);
		result.PacketType = NetPacketType.NotifyAcks;
		result.Sequence = 0;
		result.AckSequence = ackSequence;
		result.AckMask = ackMask;
		result.Fragment = 0;
		return result;
	}
}
