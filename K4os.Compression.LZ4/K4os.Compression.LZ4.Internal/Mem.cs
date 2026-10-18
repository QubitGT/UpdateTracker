using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace K4os.Compression.LZ4.Internal;

public class Mem
{
	public const int K1 = 1024;

	public const int K2 = 2048;

	public const int K4 = 4096;

	public const int K8 = 8192;

	public const int K16 = 16384;

	public const int K32 = 32768;

	public const int K64 = 65536;

	public const int K128 = 131072;

	public const int K256 = 262144;

	public const int K512 = 524288;

	public const int M1 = 1048576;

	public const int M4 = 4194304;

	public static readonly byte[] Empty = Array.Empty<byte>();

	public unsafe static bool System32
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return sizeof(void*) < 8;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int RoundUp(int value, int step)
	{
		return (value + step - 1) / step * step;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static void CpBlk(void* target, void* source, uint length)
	{
		Unsafe.CopyBlockUnaligned(target, source, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static void ZBlk(void* target, byte value, uint length)
	{
		Unsafe.InitBlockUnaligned(target, value, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Copy(byte* target, byte* source, int length)
	{
		if (length > 0)
		{
			CpBlk(target, source, (uint)length);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Move(byte* target, byte* source, int length)
	{
		Buffer.MemoryCopy(source, target, length, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void* Alloc(int size)
	{
		return Marshal.AllocHGlobal(size).ToPointer();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static byte* Zero(byte* target, int length)
	{
		return Fill(target, 0, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static byte* Fill(byte* target, byte value, int length)
	{
		if (length > 0)
		{
			ZBlk(target, value, (uint)length);
		}
		return target;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void* AllocZero(int size)
	{
		return Zero((byte*)Alloc(size), size);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Free(void* ptr)
	{
		Marshal.FreeHGlobal(new IntPtr(ptr));
	}

	public unsafe static T* CloneArray<T>(T[] array) where T : unmanaged
	{
		int num = Unsafe.SizeOf<T>() * array.Length;
		void* intPtr = Alloc(num);
		fixed (byte* ptr = &Unsafe.As<T, byte>(ref array[0]))
		{
			void* source = ptr;
			Copy((byte*)intPtr, (byte*)source, num);
		}
		return (T*)intPtr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static byte Peek1(void* p)
	{
		return *(byte*)p;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Poke1(void* p, byte v)
	{
		*(byte*)p = v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ushort Peek2(void* p)
	{
		ushort result = default(ushort);
		CpBlk(&result, p, 2u);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Poke2(void* p, ushort v)
	{
		CpBlk(p, &v, 2u);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static uint Peek4(void* p)
	{
		uint result = default(uint);
		CpBlk(&result, p, 4u);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Poke4(void* p, uint v)
	{
		CpBlk(p, &v, 4u);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ulong Peek8(void* p)
	{
		ulong result = default(ulong);
		CpBlk(&result, p, 8u);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Poke8(void* p, ulong v)
	{
		CpBlk(p, &v, 8u);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Copy1(byte* target, byte* source)
	{
		*target = *source;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Copy2(byte* target, byte* source)
	{
		CpBlk(target, source, 2u);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Copy4(byte* target, byte* source)
	{
		CpBlk(target, source, 4u);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Copy8(byte* target, byte* source)
	{
		CpBlk(target, source, 8u);
	}
}
