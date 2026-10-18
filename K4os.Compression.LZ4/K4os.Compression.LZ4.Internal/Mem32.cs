using System.Runtime.CompilerServices;

namespace K4os.Compression.LZ4.Internal;

public class Mem32 : Mem
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static uint PeekW(void* p)
	{
		return Mem.Peek4(p);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void PokeW(void* p, uint v)
	{
		Mem.Poke4(p, v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Copy16(byte* target, byte* source)
	{
		Mem.Copy8(target, source);
		Mem.Copy8(target + 8, source + 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Copy18(byte* target, byte* source)
	{
		Mem.Copy8(target, source);
		Mem.Copy8(target + 8, source + 8);
		Mem.Copy2(target + 16, source + 16);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void WildCopy8(byte* target, byte* source, void* limit)
	{
		do
		{
			Mem.Copy8(target, source);
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
