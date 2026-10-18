#define TRACE
#define DEBUG
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
public struct SimulationMessage : ILogDumpable
{
	[Flags]
	private enum BuiltInFlags
	{
		USER_MESSAGE = 1,
		REMOTE = 2,
		STATIC = 4,
		UNRELIABLE = 8,
		TARGET_PLAYER = 0x10,
		TARGET_SERVER = 0x20,
		INTERNAL = 0x40,
		NOT_TICK_ALIGNED = 0x80,
		DUMMY = 0x100
	}

	public const int SIZE = 28;

	public const int MAX_PAYLOAD_SIZE = 512;

	public const int FLAG_USER_MESSAGE = 1;

	public const int FLAG_REMOTE = 2;

	public const int FLAG_STATIC = 4;

	public const int FLAG_UNRELIABLE = 8;

	public const int FLAG_TARGET_PLAYER = 16;

	public const int FLAG_TARGET_SERVER = 32;

	public const int FLAG_INTERNAL = 64;

	public const int FLAG_NOT_TICK_ALIGNED = 128;

	public const int FLAG_DUMMY = 256;

	public const int FLAG_USER_FLAGS_START = 65536;

	public const int FLAGS_RESERVED = 65535;

	public const int FLAGS_RESERVED_BITS = 16;

	[FieldOffset(0)]
	public int Tick;

	[FieldOffset(4)]
	public PlayerRef Source;

	[FieldOffset(8)]
	public int Capacity;

	[FieldOffset(12)]
	public int Offset;

	[FieldOffset(16)]
	public int References;

	[FieldOffset(20)]
	public int Flags;

	[FieldOffset(24)]
	public PlayerRef Target;

	public bool IsUnreliable
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Flags & 8) == 8;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ReferenceCountAdd()
	{
		References++;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ReferenceCountSub()
	{
		References--;
		Assert.Check(References >= 0);
		return References == 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetTarget(PlayerRef target)
	{
		Target = target;
		Flags |= (target.IsNone ? 32 : 16);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetStatic()
	{
		Flags |= 4;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetUnreliable()
	{
		Flags |= 8;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetNotTickAligned()
	{
		Flags |= 128;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDummy()
	{
		Flags |= 256;
		Offset = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool GetFlag(int flag)
	{
		return (Flags & flag) == flag;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IsTargeted()
	{
		return (Flags & 0x30) != 0;
	}

	public unsafe static SimulationMessage* Clone(Simulation sim, SimulationMessage* message)
	{
		int num = Maths.BytesRequiredForBits(message->Capacity);
		SimulationMessage* ptr = Allocate(sim, num);
		Native.MemCpy(ptr, message, 28 + num);
		ptr->Tick = 0;
		ptr->References = 0;
		return ptr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static void Free(Simulation sim, ref SimulationMessage* message)
	{
		InternalLogStreams.LogTraceSimulationMessage?.Log(sim, $"Freeing {LogUtils.GetDump(message)}");
		Assert.Always(message->References == 0, "Message is still referenced");
		sim.TempFree(ref message);
	}

	[Obsolete("Use GetRawData instead")]
	public unsafe static byte* GetData(SimulationMessage* message)
	{
		return (byte*)message + 28;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static Span<byte> GetRawData(SimulationMessage* message)
	{
		Assert.Check(sizeof(SimulationMessage) == 28);
		return new Span<byte>((byte*)message + 28, Maths.BytesRequiredForBits(message->Capacity));
	}

	[return: NotNull]
	public unsafe static SimulationMessage* Allocate(Simulation sim, int capacityInBytes)
	{
		Assert.Check(sizeof(SimulationMessage) == 28);
		Assert.Always(capacityInBytes >= 0 && capacityInBytes < 512, "Invalid capacity: {0}", capacityInBytes);
		SimulationMessage* ptr = (SimulationMessage*)sim.TempAlloc(28 + capacityInBytes);
		ptr->Capacity = capacityInBytes * 8;
		InternalLogStreams.LogTraceSimulationMessage?.Log(sim, $"Allocated SimulationMessage: {ptr->Capacity} {LogUtils.GetDump(ptr)}");
		return ptr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool CanAllocateUserPayload(int capacityInBytes)
	{
		return capacityInBytes <= 512;
	}

	public override string ToString()
	{
		return ToString(useBrackets: true);
	}

	public string ToString(bool useBrackets)
	{
		return string.Format("{0}{1}={2}, {3}={4}, {5}={6}, {7}={8}, {9}={10}, {11}={12}, Flags={13}, UserFlags={14}{15}", useBrackets ? "[SimulationMessage: " : "", "Tick", Tick, "Source", Source, "Capacity", Capacity, "Offset", Offset, "References", References, "Target", Target, (BuiltInFlags)(Flags & 0xFFFF), Flags & -65536, useBrackets ? "]" : "");
	}

	internal unsafe static string DumpContents(SimulationMessage* message)
	{
		Span<byte> rawData = GetRawData(message);
		if (message->GetFlag(1))
		{
			return BinUtils.BytesToHex(rawData, Maths.BytesRequiredForBits(message->Capacity));
		}
		RpcHeader rpcHeader = rawData.Read<RpcHeader>();
		return $"{rpcHeader} {BinUtils.BytesToHex(rawData)}";
	}

	unsafe void ILogDumpable.Dump(StringBuilder builder)
	{
		builder.Append(ToString());
		builder.Append("\n");
		fixed (SimulationMessage* message = &this)
		{
			builder.Append(DumpContents(message));
		}
	}
}
