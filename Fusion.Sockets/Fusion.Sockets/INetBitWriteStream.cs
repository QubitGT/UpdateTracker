using System;

namespace Fusion.Sockets;

public interface INetBitWriteStream
{
	int OffsetBits { get; }

	void WriteInt32(int value, int bits = 32);

	void WriteInt32VarLength(int value);

	void WriteInt32VarLength(int value, int blockSize);

	void WriteUInt64VarLength(ulong value, int blockSize);

	bool WriteBoolean(bool b);

	unsafe void WriteBytesAligned(void* buffer, int length);

	void WriteBytesAligned(Span<byte> buffer);
}
