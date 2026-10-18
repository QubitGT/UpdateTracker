using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using K4os.Compression.LZ4.Internal;

namespace K4os.Compression.LZ4.Engine;

internal class LL
{
	public struct LZ4_stream_t
	{
		public unsafe fixed uint hashTable[4096];

		public uint currentOffset;

		public bool dirty;

		public tableType_t tableType;

		public unsafe byte* dictionary;

		public unsafe LZ4_stream_t* dictCtx;

		public uint dictSize;
	}

	public struct LZ4_streamDecode_t
	{
		public unsafe byte* externalDict;

		public uint extDictSize;

		public unsafe byte* prefixEnd;

		public uint prefixSize;
	}

	public enum limitedOutput_directive
	{
		notLimited,
		limitedOutput,
		fillOutput
	}

	public enum tableType_t
	{
		clearedTable,
		byPtr,
		byU32,
		byU16
	}

	public enum dict_directive
	{
		noDict,
		withPrefix64k,
		usingExtDict,
		usingDictCtx
	}

	public enum dictIssue_directive
	{
		noDictIssue,
		dictSmall
	}

	public enum endCondition_directive
	{
		endOnOutputSize,
		endOnInputSize
	}

	public enum earlyEnd_directive
	{
		full,
		partial
	}

	protected enum variable_length_error
	{
		loop_error = -2,
		initial_error,
		ok
	}

	public enum dictCtx_directive
	{
		noDictCtx,
		usingDictCtxHc
	}

	public struct LZ4_streamHC_t
	{
		public unsafe fixed uint hashTable[32768];

		public unsafe fixed ushort chainTable[65536];

		public unsafe byte* end;

		public unsafe byte* @base;

		public unsafe byte* dictBase;

		public uint dictLimit;

		public uint lowLimit;

		public uint nextToUpdate;

		public short compressionLevel;

		public bool favorDecSpeed;

		public bool dirty;

		public unsafe LZ4_streamHC_t* dictCtx;
	}

	protected enum repeat_state_e
	{
		rep_untested,
		rep_not,
		rep_confirmed
	}

	public enum HCfavor_e
	{
		favorCompressionRatio,
		favorDecompressionSpeed
	}

	public struct LZ4HC_match_t
	{
		public int off;

		public int len;
	}

	public struct LZ4HC_optimal_t
	{
		public int price;

		public int off;

		public int mlen;

		public int litlen;
	}

	public enum lz4hc_strat_e
	{
		lz4hc,
		lz4opt
	}

	public struct cParams_t
	{
		public lz4hc_strat_e strat;

		public uint nbSearches;

		public uint targetLength;

		public cParams_t(lz4hc_strat_e strat, uint nbSearches, uint targetLength)
		{
			this.strat = strat;
			this.nbSearches = nbSearches;
			this.targetLength = targetLength;
		}
	}

	private static readonly uint[] _inc32table = new uint[8] { 0u, 1u, 2u, 1u, 0u, 4u, 4u, 4u };

	private static readonly int[] _dec64table = new int[8] { 0, 0, 0, -1, -4, 1, 2, 3 };

	protected unsafe static readonly uint* inc32table = Mem.CloneArray(_inc32table);

	protected unsafe static readonly int* dec64table = Mem.CloneArray(_dec64table);

	protected const int LZ4_MEMORY_USAGE = 14;

	protected const int LZ4_MAX_INPUT_SIZE = 2113929216;

	protected const int LZ4_DISTANCE_MAX = 65535;

	protected const int LZ4_DISTANCE_ABSOLUTE_MAX = 65535;

	protected const int LZ4_HASHLOG = 12;

	protected const int LZ4_HASHTABLESIZE = 16384;

	protected const int LZ4_HASH_SIZE_U32 = 4096;

	protected const int ACCELERATION_DEFAULT = 1;

	protected const int MINMATCH = 4;

	protected const int WILDCOPYLENGTH = 8;

	protected const int LASTLITERALS = 5;

	protected const int MFLIMIT = 12;

	protected const int MATCH_SAFEGUARD_DISTANCE = 12;

	protected const int FASTLOOP_SAFE_DISTANCE = 64;

	protected const int LZ4_minLength = 13;

	protected const int KB = 1024;

	protected const int MB = 1048576;

	protected const uint GB = 1073741824u;

	protected const int ML_BITS = 4;

	protected const uint ML_MASK = 15u;

	protected const int RUN_BITS = 4;

	protected const uint RUN_MASK = 15u;

	protected const int OPTIMAL_ML = 18;

	protected const int LZ4_OPT_NUM = 4096;

	protected const int LZ4_64Klimit = 65547;

	protected const int LZ4_skipTrigger = 6;

	protected const int LZ4HC_DICTIONARY_LOGSIZE = 16;

	protected const int LZ4HC_MAXD = 65536;

	protected const int LZ4HC_MAXD_MASK = 65535;

	protected const int LZ4HC_HASH_LOG = 15;

	protected const int LZ4HC_HASHTABLESIZE = 32768;

	protected const int LZ4HC_HASH_MASK = 32767;

	protected const int LZ4HC_CLEVEL_MIN = 3;

	protected const int LZ4HC_CLEVEL_DEFAULT = 9;

	protected const int LZ4HC_CLEVEL_OPT_MIN = 10;

	protected const int LZ4HC_CLEVEL_MAX = 12;

	public static bool Enforce32 { get; set; } = false;


	public static Algorithm Algorithm
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (!Enforce32 && !Mem.System32)
			{
				return Algorithm.X64;
			}
			return Algorithm.X32;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int LZ4_sizeofStateHC()
	{
		return sizeof(LZ4_streamHC_t);
	}

	public unsafe static void LZ4_setCompressionLevel(LZ4_streamHC_t* LZ4_streamHCPtr, int compressionLevel)
	{
		if (compressionLevel < 1)
		{
			compressionLevel = 9;
		}
		if (compressionLevel > 12)
		{
			compressionLevel = 12;
		}
		LZ4_streamHCPtr->compressionLevel = (short)compressionLevel;
	}

	public unsafe static void LZ4_favorDecompressionSpeed(LZ4_streamHC_t* LZ4_streamHCPtr, int favor)
	{
		LZ4_streamHCPtr->favorDecSpeed = favor != 0;
	}

	public unsafe static LZ4_streamHC_t* LZ4_initStreamHC(void* buffer, int size)
	{
		if (buffer == null)
		{
			return null;
		}
		if (size < sizeof(LZ4_streamHC_t))
		{
			return null;
		}
		((LZ4_streamHC_t*)buffer)->end = (byte*)(-1);
		((LZ4_streamHC_t*)buffer)->@base = null;
		((LZ4_streamHC_t*)buffer)->dictCtx = null;
		((LZ4_streamHC_t*)buffer)->favorDecSpeed = false;
		((LZ4_streamHC_t*)buffer)->dirty = false;
		LZ4_setCompressionLevel((LZ4_streamHC_t*)buffer, 9);
		return (LZ4_streamHC_t*)buffer;
	}

	public unsafe static LZ4_streamHC_t* LZ4_initStreamHC(LZ4_streamHC_t* stream)
	{
		return LZ4_initStreamHC(stream, sizeof(LZ4_streamHC_t));
	}

	public unsafe static void LZ4_resetStreamHC_fast(LZ4_streamHC_t* LZ4_streamHCPtr, int compressionLevel)
	{
		if (LZ4_streamHCPtr->dirty)
		{
			LZ4_initStreamHC(LZ4_streamHCPtr);
		}
		else
		{
			byte** end = &LZ4_streamHCPtr->end;
			*end -= LZ4_streamHCPtr->@base;
			LZ4_streamHCPtr->@base = null;
			LZ4_streamHCPtr->dictCtx = null;
		}
		LZ4_setCompressionLevel(LZ4_streamHCPtr, compressionLevel);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint HASH_FUNCTION(uint value)
	{
		return (uint)((int)value * -1640531535) >> 17;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static ref ushort DELTANEXTU16(ushort* table, uint pos)
	{
		return ref table[(int)(ushort)pos];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static uint LZ4HC_hashPtr(void* ptr)
	{
		return HASH_FUNCTION(Mem.Peek4(ptr));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static void LZ4HC_Insert(LZ4_streamHC_t* hc4, byte* ip)
	{
		ushort* table = hc4->chainTable;
		uint* ptr = hc4->hashTable;
		byte* @base = hc4->@base;
		uint num = (uint)(ip - @base);
		for (uint num2 = hc4->nextToUpdate; num2 < num; num2++)
		{
			uint num3 = LZ4HC_hashPtr(@base + num2);
			uint num4 = num2 - ptr[num3];
			if (num4 > 65535)
			{
				num4 = 65535u;
			}
			DELTANEXTU16(table, num2) = (ushort)num4;
			ptr[num3] = num2;
		}
		hc4->nextToUpdate = num;
	}

	public unsafe static void LZ4HC_setExternalDict(LZ4_streamHC_t* ctxPtr, byte* newBlock)
	{
		if (ctxPtr->end >= ctxPtr->@base + ctxPtr->dictLimit + 4)
		{
			LZ4HC_Insert(ctxPtr, ctxPtr->end - 3);
		}
		ctxPtr->lowLimit = ctxPtr->dictLimit;
		ctxPtr->dictLimit = (uint)(ctxPtr->end - ctxPtr->@base);
		ctxPtr->dictBase = ctxPtr->@base;
		ctxPtr->@base = newBlock - ctxPtr->dictLimit;
		ctxPtr->end = newBlock;
		ctxPtr->nextToUpdate = ctxPtr->dictLimit;
		ctxPtr->dictCtx = null;
	}

	public unsafe static void LZ4HC_clearTables(LZ4_streamHC_t* hc4)
	{
		Mem.Fill((byte*)hc4->hashTable, 0, 131072);
		Mem.Fill((byte*)hc4->chainTable, byte.MaxValue, 131072);
	}

	public unsafe static void LZ4HC_init_internal(LZ4_streamHC_t* hc4, byte* start)
	{
		long num = hc4->end - hc4->@base;
		if (num < 0 || num > 1073741824)
		{
			LZ4HC_clearTables(hc4);
			num = 0L;
		}
		num += 65536;
		hc4->nextToUpdate = (uint)num;
		hc4->@base = start - num;
		hc4->end = start;
		hc4->dictBase = start - num;
		hc4->dictLimit = (uint)num;
		hc4->lowLimit = (uint)num;
	}

	public unsafe static int LZ4_saveDictHC(LZ4_streamHC_t* LZ4_streamHCPtr, byte* safeBuffer, int dictSize)
	{
		int num = (int)(LZ4_streamHCPtr->end - (LZ4_streamHCPtr->@base + LZ4_streamHCPtr->dictLimit));
		if (dictSize > 65536)
		{
			dictSize = 65536;
		}
		if (dictSize < 4)
		{
			dictSize = 0;
		}
		if (dictSize > num)
		{
			dictSize = num;
		}
		Mem.Move(safeBuffer, LZ4_streamHCPtr->end - dictSize, dictSize);
		uint num2 = (uint)(LZ4_streamHCPtr->end - LZ4_streamHCPtr->@base);
		LZ4_streamHCPtr->end = safeBuffer + dictSize;
		LZ4_streamHCPtr->@base = LZ4_streamHCPtr->end - num2;
		LZ4_streamHCPtr->dictLimit = num2 - (uint)dictSize;
		LZ4_streamHCPtr->lowLimit = num2 - (uint)dictSize;
		if (LZ4_streamHCPtr->nextToUpdate < LZ4_streamHCPtr->dictLimit)
		{
			LZ4_streamHCPtr->nextToUpdate = LZ4_streamHCPtr->dictLimit;
		}
		return dictSize;
	}

	public unsafe static int LZ4_loadDictHC(LZ4_streamHC_t* LZ4_streamHCPtr, byte* dictionary, int dictSize)
	{
		if (dictSize > 65536)
		{
			dictionary += dictSize - 65536;
			dictSize = 65536;
		}
		int compressionLevel = LZ4_streamHCPtr->compressionLevel;
		LZ4_initStreamHC(LZ4_streamHCPtr);
		LZ4_setCompressionLevel(LZ4_streamHCPtr, compressionLevel);
		LZ4HC_init_internal(LZ4_streamHCPtr, dictionary);
		LZ4_streamHCPtr->end = dictionary + dictSize;
		if (dictSize >= 4)
		{
			LZ4HC_Insert(LZ4_streamHCPtr, LZ4_streamHCPtr->end - 3);
		}
		return dictSize;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint LZ4HC_rotl32(uint x, int r)
	{
		return (x << r) | (x >> 32 - r);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool LZ4HC_protectDictEnd(uint dictLimit, uint matchIndex)
	{
		return dictLimit - 1 - matchIndex >= 3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int LZ4HC_countBack(byte* ip, byte* match, byte* iMin, byte* mMin)
	{
		int num = 0;
		int num2 = (int)MAX(iMin - ip, mMin - match);
		while (num > num2 && ip[num - 1] == match[num - 1])
		{
			num--;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static uint LZ4HC_reverseCountPattern(byte* ip, byte* iLow, uint pattern)
	{
		byte* ptr = ip;
		while (ip >= iLow + 4 && Mem.Peek4(ip - 4) == pattern)
		{
			ip -= 4;
		}
		byte* ptr2 = (byte*)(&pattern) + 3;
		while (ip > iLow && ip[-1] == *ptr2)
		{
			ip--;
			ptr2--;
		}
		return (uint)(ptr - ip);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint LZ4HC_rotatePattern(uint rotate, uint pattern)
	{
		uint num = (rotate & 3) << 3;
		if (num == 0)
		{
			return pattern;
		}
		return LZ4HC_rotl32(pattern, (int)num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LZ4HC_literalsPrice(int litlen)
	{
		int num = litlen;
		if (litlen >= 15)
		{
			num += 1 + (litlen - 15) / 255;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int LZ4HC_sequencePrice(int litlen, int mlen)
	{
		int num = 3;
		num += LZ4HC_literalsPrice(litlen);
		if (mlen >= 19)
		{
			num += 1 + (mlen - 19) / 255;
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public static void Assert(bool value, [CallerArgumentExpression("value")] string message = null)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int LZ4_compressBound(int isize)
	{
		if (isize <= 2113929216)
		{
			return isize + isize / 255 + 16;
		}
		return 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int LZ4_decoderRingBufferSize(int isize)
	{
		return 65550 + isize;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static uint LZ4_hash4(uint sequence, tableType_t tableType)
	{
		int num = ((tableType == tableType_t.byU16) ? 13 : 12);
		return (uint)((int)sequence * -1640531535) >> 32 - num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected static uint LZ4_hash5(ulong sequence, tableType_t tableType)
	{
		int num = ((tableType == tableType_t.byU16) ? 13 : 12);
		return (uint)((sequence << 24) * 889523592379L >> 64 - num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static void LZ4_clearHash(uint h, void* tableBase, tableType_t tableType)
	{
		switch (tableType)
		{
		case tableType_t.byPtr:
			*(IntPtr*)((byte*)tableBase + h * sizeof(byte*)) = (nint)0;
			break;
		case tableType_t.byU32:
			*(int*)((byte*)tableBase + (long)h * 4L) = 0;
			break;
		case tableType_t.byU16:
			*(short*)((byte*)tableBase + (long)h * 2L) = 0;
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static void LZ4_putIndexOnHash(uint idx, uint h, void* tableBase, tableType_t tableType)
	{
		switch (tableType)
		{
		case tableType_t.byU32:
			*(uint*)((byte*)tableBase + (long)h * 4L) = idx;
			break;
		case tableType_t.byU16:
			*(ushort*)((byte*)tableBase + (long)h * 2L) = (ushort)idx;
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static void LZ4_putPositionOnHash(byte* p, uint h, void* tableBase, tableType_t tableType, byte* srcBase)
	{
		switch (tableType)
		{
		case tableType_t.byPtr:
			*(byte**)((byte*)tableBase + h * sizeof(byte*)) = p;
			break;
		case tableType_t.byU32:
			*(int*)((byte*)tableBase + (long)h * 4L) = (int)(p - srcBase);
			break;
		case tableType_t.byU16:
			*(ushort*)((byte*)tableBase + (long)h * 2L) = (ushort)(p - srcBase);
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static uint LZ4_getIndexOnHash(uint h, void* tableBase, tableType_t tableType)
	{
		return tableType switch
		{
			tableType_t.byU32 => *(uint*)((byte*)tableBase + (long)h * 4L), 
			tableType_t.byU16 => *(ushort*)((byte*)tableBase + (long)h * 2L), 
			_ => 0u, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static byte* LZ4_getPositionOnHash(uint h, void* tableBase, tableType_t tableType, byte* srcBase)
	{
		return tableType switch
		{
			tableType_t.byPtr => *(byte**)((byte*)tableBase + h * sizeof(byte*)), 
			tableType_t.byU32 => (uint)(*(int*)((byte*)tableBase + (long)h * 4L)) + srcBase, 
			_ => (int)(*(ushort*)((byte*)tableBase + (long)h * 2L)) + srcBase, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int MIN(int a, int b)
	{
		if (a >= b)
		{
			return b;
		}
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint MIN(uint a, uint b)
	{
		if (a >= b)
		{
			return b;
		}
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint MAX(uint a, uint b)
	{
		if (a >= b)
		{
			return a;
		}
		return b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long MAX(long a, long b)
	{
		if (a >= b)
		{
			return a;
		}
		return b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long MIN(long a, long b)
	{
		if (a >= b)
		{
			return b;
		}
		return a;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected unsafe static uint LZ4_readVLE(byte** ip, byte* lencheck, bool loop_check, bool initial_check, variable_length_error* error)
	{
		uint num = 0u;
		if (initial_check && *ip >= lencheck)
		{
			*error = variable_length_error.initial_error;
			return num;
		}
		uint num2;
		do
		{
			num2 = *(*ip);
			(*ip)++;
			num += num2;
			if (loop_check && *ip >= lencheck)
			{
				*error = variable_length_error.loop_error;
				return num;
			}
		}
		while (num2 == 255);
		return num;
	}

	public unsafe static int LZ4_saveDict(LZ4_stream_t* LZ4_dict, byte* safeBuffer, int dictSize)
	{
		byte* ptr = LZ4_dict->dictionary + LZ4_dict->dictSize;
		if ((uint)dictSize > 65536u)
		{
			dictSize = 65536;
		}
		if ((uint)dictSize > LZ4_dict->dictSize)
		{
			dictSize = (int)LZ4_dict->dictSize;
		}
		Mem.Move(safeBuffer, ptr - dictSize, dictSize);
		LZ4_dict->dictionary = safeBuffer;
		LZ4_dict->dictSize = (uint)dictSize;
		return dictSize;
	}

	public unsafe static LZ4_stream_t* LZ4_initStream(LZ4_stream_t* buffer)
	{
		Mem.Zero((byte*)buffer, sizeof(LZ4_stream_t));
		return buffer;
	}

	public unsafe static void LZ4_setStreamDecode(LZ4_streamDecode_t* LZ4_streamDecode, byte* dictionary, int dictSize)
	{
		LZ4_streamDecode->prefixSize = (uint)dictSize;
		LZ4_streamDecode->prefixEnd = dictionary + dictSize;
		LZ4_streamDecode->externalDict = null;
		LZ4_streamDecode->extDictSize = 0u;
	}
}
