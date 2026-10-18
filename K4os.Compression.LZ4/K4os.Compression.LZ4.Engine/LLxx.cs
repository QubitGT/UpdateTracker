using System;
using System.Runtime.CompilerServices;

namespace K4os.Compression.LZ4.Engine;

internal static class LLxx
{
	private static NotImplementedException AlgorithmNotImplemented(string action)
	{
		return new NotImplementedException($"Algorithm {LL.Algorithm} not implemented for {action}");
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe static int LZ4_decompress_safe(byte* source, byte* target, int sourceLength, int targetLength)
	{
		return LL.Algorithm switch
		{
			Algorithm.X64 => LL64.LZ4_decompress_safe(source, target, sourceLength, targetLength), 
			Algorithm.X32 => LL32.LZ4_decompress_safe(source, target, sourceLength, targetLength), 
			_ => throw AlgorithmNotImplemented("LZ4_decompress_safe"), 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe static int LZ4_decompress_safe_partial(byte* source, byte* target, int sourceLength, int targetLength)
	{
		return LL.Algorithm switch
		{
			Algorithm.X64 => LL64.LZ4_decompress_safe_partial(source, target, sourceLength, targetLength, targetLength), 
			Algorithm.X32 => LL32.LZ4_decompress_safe_partial(source, target, sourceLength, targetLength, targetLength), 
			_ => throw AlgorithmNotImplemented("LZ4_decompress_safe_partial"), 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe static int LZ4_decompress_safe_usingDict(byte* source, byte* target, int sourceLength, int targetLength, byte* dictionary, int dictionaryLength)
	{
		return LL.Algorithm switch
		{
			Algorithm.X64 => LL64.LZ4_decompress_safe_usingDict(source, target, sourceLength, targetLength, dictionary, dictionaryLength), 
			Algorithm.X32 => LL32.LZ4_decompress_safe_usingDict(source, target, sourceLength, targetLength, dictionary, dictionaryLength), 
			_ => throw AlgorithmNotImplemented("LZ4_decompress_safe_usingDict"), 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe static int LZ4_decompress_safe_continue(LL.LZ4_streamDecode_t* context, byte* source, byte* target, int sourceLength, int targetLength)
	{
		return LL.Algorithm switch
		{
			Algorithm.X64 => LL64.LZ4_decompress_safe_continue(context, source, target, sourceLength, targetLength), 
			Algorithm.X32 => LL32.LZ4_decompress_safe_continue(context, source, target, sourceLength, targetLength), 
			_ => throw AlgorithmNotImplemented("LZ4_decompress_safe_continue"), 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe static int LZ4_compress_fast(byte* source, byte* target, int sourceLength, int targetLength, int acceleration)
	{
		return LL.Algorithm switch
		{
			Algorithm.X64 => LL64.LZ4_compress_fast(source, target, sourceLength, targetLength, acceleration), 
			Algorithm.X32 => LL32.LZ4_compress_fast(source, target, sourceLength, targetLength, acceleration), 
			_ => throw AlgorithmNotImplemented("LZ4_compress_fast"), 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe static int LZ4_compress_fast_continue(LL.LZ4_stream_t* context, byte* source, byte* target, int sourceLength, int targetLength, int acceleration)
	{
		return LL.Algorithm switch
		{
			Algorithm.X64 => LL64.LZ4_compress_fast_continue(context, source, target, sourceLength, targetLength, acceleration), 
			Algorithm.X32 => LL32.LZ4_compress_fast_continue(context, source, target, sourceLength, targetLength, acceleration), 
			_ => throw AlgorithmNotImplemented("LZ4_compress_fast_continue"), 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe static int LZ4_compress_HC(byte* source, byte* target, int sourceLength, int targetLength, int level)
	{
		return LL.Algorithm switch
		{
			Algorithm.X64 => LL64.LZ4_compress_HC(source, target, sourceLength, targetLength, level), 
			Algorithm.X32 => LL32.LZ4_compress_HC(source, target, sourceLength, targetLength, level), 
			_ => throw AlgorithmNotImplemented("LZ4_compress_HC"), 
		};
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	public unsafe static int LZ4_compress_HC_continue(LL.LZ4_streamHC_t* context, byte* source, byte* target, int sourceLength, int targetLength)
	{
		return LL.Algorithm switch
		{
			Algorithm.X64 => LL64.LZ4_compress_HC_continue(context, source, target, sourceLength, targetLength), 
			Algorithm.X32 => LL32.LZ4_compress_HC_continue(context, source, target, sourceLength, targetLength), 
			_ => throw AlgorithmNotImplemented("LZ4_compress_HC_continue"), 
		};
	}
}
