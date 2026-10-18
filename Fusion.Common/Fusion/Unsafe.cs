using System.Runtime.CompilerServices;

namespace Fusion;

public static class Unsafe
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref TTo As<TFrom, TTo>(ref TFrom source)
	{
		return ref System.Runtime.CompilerServices.Unsafe.As<TFrom, TTo>(ref source);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void* AsPointer<T>(ref T value)
	{
		return System.Runtime.CompilerServices.Unsafe.AsPointer(ref value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static ref T AsRef<T>(void* source)
	{
		return ref *(T*)source;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref T AsRef<T>(in T source)
	{
		return ref source;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T Read<T>(void* source)
	{
		return System.Runtime.CompilerServices.Unsafe.Read<T>(source);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static T ReadUnaligned<T>(void* source)
	{
		return System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(source);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T ReadUnaligned<T>(ref byte source)
	{
		return System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ref source);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void Write<T>(void* destination, T value)
	{
		System.Runtime.CompilerServices.Unsafe.Write(destination, value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void WriteUnaligned<T>(void* destination, T value)
	{
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned(destination, value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void WriteUnaligned<T>(ref byte destination, T value)
	{
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ref destination, value);
	}
}
