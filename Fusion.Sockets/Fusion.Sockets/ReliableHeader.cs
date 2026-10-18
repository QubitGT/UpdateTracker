#define DEBUG
using System.Runtime.InteropServices;

namespace Fusion.Sockets;

[StructLayout(LayoutKind.Explicit)]
public struct ReliableHeader
{
	public const int SIZE = 64;

	[FieldOffset(0)]
	public unsafe ReliableHeader* Next;

	[FieldOffset(8)]
	public unsafe ReliableHeader* Prev;

	[FieldOffset(16)]
	public ReliableId Id;

	public unsafe static byte* GetData(ReliableHeader* header)
	{
		Assert.Check(sizeof(ReliableHeader) == 64);
		return (byte*)header + 64;
	}
}
