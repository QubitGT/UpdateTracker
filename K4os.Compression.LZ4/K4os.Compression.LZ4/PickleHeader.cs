using System.Runtime.InteropServices;

namespace K4os.Compression.LZ4;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct PickleHeader
{
	public ushort DataOffset { get; }

	public ushort Flags { get; }

	public int ResultLength { get; }

	public bool IsCompressed => (Flags & 1) != 0;

	public PickleHeader(ushort dataOffset, int resultLength, bool compressed)
	{
		DataOffset = dataOffset;
		ResultLength = resultLength;
		Flags = (ushort)((compressed ? 1u : 0u) | 0u);
	}
}
