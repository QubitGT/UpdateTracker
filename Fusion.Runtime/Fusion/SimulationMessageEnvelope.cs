#define TRACE
#define DEBUG
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Fusion.Sockets;

namespace Fusion;

internal struct SimulationMessageEnvelope : ILogDumpable
{
	public ulong Sequence;

	public unsafe SimulationMessage* Message;

	public unsafe SimulationMessageEnvelope* Prev;

	public unsafe SimulationMessageEnvelope* Next;

	private const int OffsetBlockSize = 10;

	private const int TickBlockSize = 16;

	private const int SequenceBlockSize = 16;

	private unsafe static int WriteInternal<T>(SimulationMessageEnvelope* envelope, T* buffer) where T : unmanaged, INetBitWriteStream
	{
		buffer->WriteInt32VarLength(envelope->Message->Offset, 10);
		buffer->WriteInt32VarLength(envelope->Message->Tick, 16);
		Assert.Check(!envelope->Message->Source.IsRealPlayer || !envelope->Message->Target.IsRealPlayer);
		PlayerRef.Write(buffer, envelope->Message->Source);
		PlayerRef.Write(buffer, envelope->Message->Target);
		int num = envelope->Message->Flags & -3;
		int num2 = num & 0xFFFF;
		int num3 = num >> 16;
		if (buffer->WriteBoolean(num2 != 0))
		{
			buffer->WriteInt32(num2, 16);
			if ((num2 & 1) == 1 && buffer->WriteBoolean(num3 != 0))
			{
				InternalLogStreams.LogError?.Log("Trying to write user flags");
				buffer->WriteInt32(num3, -12);
			}
		}
		else
		{
			Assert.Check(num3 == 0, "User flags should only be used if FLAG_USER_MESSAGE is set");
		}
		if ((envelope->Message->Flags & 8) != 8)
		{
			buffer->WriteUInt64VarLength(envelope->Sequence, 16);
		}
		int offsetBits = buffer->OffsetBits;
		if (envelope->Message->Offset > 0)
		{
			int length = Maths.BytesRequiredForBits(envelope->Message->Offset);
			buffer->WriteBytesAligned(SimulationMessage.GetRawData(envelope->Message).Slice(0, length));
		}
		return offsetBits;
	}

	public unsafe static void Write(SimulationMessageEnvelope* envelope, NetBitBuffer* buffer)
	{
		int offsetBits = buffer->OffsetBits;
		int num = WriteInternal(envelope, buffer);
		InternalLogStreams.LogTraceSimulationMessage?.Log($"Wrote (header={num - offsetBits}, payload={buffer->OffsetBits - num}) {LogUtils.GetDump(envelope)}");
	}

	public unsafe static int GetBitCount(SimulationMessageEnvelope* envelope, NetBitBuffer* buffer)
	{
		int offsetBits = buffer->OffsetBits;
		NetBitBufferNull netBitBufferNull = default(NetBitBufferNull);
		netBitBufferNull.OffsetBits = offsetBits;
		NetBitBufferNull netBitBufferNull2 = netBitBufferNull;
		WriteInternal(envelope, &netBitBufferNull2);
		return netBitBufferNull2.OffsetBits - offsetBits;
	}

	[return: NotNull]
	public unsafe static SimulationMessageEnvelope* Read(Simulation sim, NetBitBuffer* buffer)
	{
		int num = buffer->ReadInt32VarLength(10);
		int tick = buffer->ReadInt32VarLength(16);
		PlayerRef source = PlayerRef.Read(buffer);
		PlayerRef target = PlayerRef.Read(buffer);
		int num2 = (buffer->ReadBoolean() ? buffer->ReadInt32(16) : 0);
		if ((num2 & 1) == 1)
		{
			bool flag = buffer->ReadBoolean();
			if (flag)
			{
				InternalLogStreams.LogError?.Log("Trying to read user flags");
			}
			int num3 = (flag ? buffer->ReadInt32(-12) : 0);
			num2 |= num3 << 16;
		}
		ulong sequence = (((num2 & 8) != 8) ? buffer->ReadUInt64VarLength(16) : 0);
		num2 |= 2;
		int capacityInBytes = Maths.BytesRequiredForBits(num);
		SimulationMessageEnvelope* ptr = Allocate(sim, SimulationMessage.Allocate(sim, capacityInBytes), sequence);
		ptr->Message->Capacity = num;
		ptr->Message->Offset = 0;
		ptr->Message->Tick = tick;
		ptr->Message->Source = source;
		ptr->Message->Target = target;
		ptr->Message->Flags = num2;
		if (ptr->Message->Capacity > 0)
		{
			buffer->ReadBytesAligned(SimulationMessage.GetRawData(ptr->Message));
		}
		InternalLogStreams.LogTraceSimulationMessage?.Log(sim, $"Read {LogUtils.GetDump(ptr)}");
		return ptr;
	}

	[return: NotNull]
	public unsafe static SimulationMessageEnvelope* Allocate(Simulation sim, SimulationMessage* message, ulong sequence)
	{
		SimulationMessageEnvelope* ptr = sim.TempAlloc<SimulationMessageEnvelope>();
		ptr->Message = message;
		ptr->Sequence = sequence;
		ptr->Next = null;
		ptr->Prev = null;
		message->ReferenceCountAdd();
		return ptr;
	}

	public unsafe static void Free(Simulation sim, ref SimulationMessageEnvelope* envelope)
	{
		if (envelope != null)
		{
			Assert.Check(envelope->Prev == null);
			Assert.Check(envelope->Next == null);
			if (envelope->Message != null && envelope->Message->ReferenceCountSub())
			{
				SimulationMessage.Free(sim, ref envelope->Message);
			}
			sim.TempFree(ref envelope);
		}
	}

	public unsafe override string ToString()
	{
		return string.Format("[SimulationMessageEnvelope: {0}={1}, {2}={3}]", "Sequence", Sequence, "Message", (Message != null) ? Message->ToString(useBrackets: false) : "null");
	}

	unsafe void ILogDumpable.Dump(StringBuilder builder)
	{
		builder.Append(ToString());
		if (Message != null)
		{
			builder.Append("\n");
			builder.Append(SimulationMessage.DumpContents(Message));
		}
	}
}
