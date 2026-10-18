using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion.Sockets;

[StructLayout(LayoutKind.Explicit)]
public struct ReliableKey
{
	public const int SIZE = 16;

	[FieldOffset(0)]
	public unsafe fixed byte Data[16];

	public unsafe void GetInts(out int key0, out int key1, out int key2, out int key3)
	{
		ReliableKey reliableKey = this;
		key0 = *(int*)reliableKey.Data;
		key1 = *(int*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref reliableKey.Data[4]);
		key2 = *(int*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref reliableKey.Data[8]);
		key3 = *(int*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref reliableKey.Data[12]);
	}

	public unsafe void GetUlongs(out ulong key0, out ulong key1)
	{
		ReliableKey reliableKey = this;
		key0 = *(ulong*)reliableKey.Data;
		key1 = *(ulong*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref reliableKey.Data[8]);
	}

	public unsafe static ReliableKey FromInts(int key0 = 0, int key1 = 0, int key2 = 0, int key3 = 0)
	{
		ReliableKey result = default(ReliableKey);
		*(int*)result.Data = key0;
		*(int*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref result.Data[4]) = key1;
		*(int*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref result.Data[8]) = key2;
		*(int*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref result.Data[12]) = key3;
		return result;
	}

	public unsafe static ReliableKey FromULongs(ulong key0 = 0uL, ulong key1 = 0uL)
	{
		ReliableKey result = default(ReliableKey);
		*(ulong*)result.Data = key0;
		*(ulong*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref result.Data[8]) = key1;
		return result;
	}
}
