using System.Runtime.CompilerServices;

namespace K4os.Compression.LZ4.Internal;

public class Mem64 : Mem
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new unsafe static ushort Peek2(void* p)
	{
		return *(ushort*)p;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new unsafe static void Poke2(void* p, ushort v)
	{
		*(ushort*)p = v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new unsafe static uint Peek4(void* p)
	{
		return *(uint*)p;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new unsafe static void Poke4(void* p, uint v)
	{
		*(uint*)p = v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new unsafe static void Copy1(byte* target, byte* source)
	{
		*target = *source;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new unsafe static void Copy2(byte* target, byte* source)
	{
		*(short*)target = *(short*)source;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new unsafe static void Copy4(byte* target, byte* source)
	{
		*(int*)target = *(int*)source;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new unsafe static ulong Peek8(void* p)
	{
		return *(ulong*)p;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new unsafe static void Poke8(void* p, ulong v)
	{
		*(ulong*)p = v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public new unsafe static void Copy8(byte* target, byte* source)
	{
		*(long*)target = *(long*)source;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ulong PeekW(void* p)
	{
		return Peek8(p);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void PokeW(void* p, ulong v)
	{
		Poke8(p, v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Copy16(byte* target, byte* source)
	{
		Copy8(target, source);
		Copy8(target + 8, source + 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Copy18(byte* target, byte* source)
	{
		Copy8(target, source);
		Copy8(target + 8, source + 8);
		Copy2(target + 16, source + 16);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void WildCopy8(byte* target, byte* source, void* limit)
	{
		do
		{
			Copy8(target, source);
			target += 8;
			source += 8;
		}
		while (target < limit);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void WildCopy32(byte* target, byte* source, void* limit)
	{
		do
		{
			Copy16(target, source);
			Copy16(target + 16, source + 16);
			target += 32;
			source += 32;
		}
		while (target < limit);
	}
}
