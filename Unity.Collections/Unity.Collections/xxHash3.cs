using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
[BurstCompile]
[GenerateTestsForBurstCompatibility]
[GenerateTestsForBurstCompatibility]
public static class xxHash3
{
	private struct ulong2
	{
		public ulong x;

		public ulong y;

		public ulong2(ulong x, ulong y)
		{
			this.x = x;
			this.y = y;
		}
	}

	[GenerateTestsForBurstCompatibility]
	public struct StreamingState
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct StreamingStateData
		{
			[FieldOffset(0)]
			public ulong Acc;

			[FieldOffset(64)]
			public byte Buffer;

			[FieldOffset(320)]
			public int IsHash64;

			[FieldOffset(324)]
			public int BufferedSize;

			[FieldOffset(328)]
			public int NbStripesSoFar;

			[FieldOffset(336)]
			public long TotalLength;

			[FieldOffset(344)]
			public ulong Seed;

			[FieldOffset(352)]
			public byte SecretKey;

			[FieldOffset(540)]
			public byte _PadEnd;
		}

		private static readonly int SECRET_LIMIT = 128;

		private static readonly int NB_STRIPES_PER_BLOCK = SECRET_LIMIT / 8;

		private static readonly int INTERNAL_BUFFER_SIZE = 256;

		private static readonly int INTERNAL_BUFFER_STRIPES = INTERNAL_BUFFER_SIZE / 64;

		private StreamingStateData State;

		private unsafe ulong* Acc
		{
			[DebuggerStepThrough]
			get
			{
				return (ulong*)UnsafeUtility.AddressOf(ref State.Acc);
			}
		}

		private unsafe byte* Buffer
		{
			[DebuggerStepThrough]
			get
			{
				return (byte*)UnsafeUtility.AddressOf(ref State.Buffer);
			}
		}

		private unsafe byte* SecretKey
		{
			[DebuggerStepThrough]
			get
			{
				return (byte*)UnsafeUtility.AddressOf(ref State.SecretKey);
			}
		}

		public StreamingState(bool isHash64, ulong seed = 0uL)
		{
			State = default(StreamingStateData);
			Reset(isHash64, seed);
		}

		public unsafe void Reset(bool isHash64, ulong seed = 0uL)
		{
			int num = UnsafeUtility.SizeOf<StreamingStateData>();
			UnsafeUtility.MemClear(UnsafeUtility.AddressOf(ref State), num);
			State.IsHash64 = (isHash64 ? 1 : 0);
			ulong* acc = Acc;
			*acc = 3266489917uL;
			acc[1] = 11400714785074694791uL;
			acc[2] = 14029467366897019727uL;
			acc[3] = 1609587929392839161uL;
			acc[4] = 9650029242287828579uL;
			acc[5] = 2246822519uL;
			acc[6] = 2870177450012600261uL;
			acc[7] = 2654435761uL;
			State.Seed = seed;
			fixed (byte* ptr = xxHashDefaultKey.kSecret)
			{
				if (seed != 0L)
				{
					EncodeSecretKey(SecretKey, ptr, seed);
				}
				else
				{
					UnsafeUtility.MemCpy(SecretKey, ptr, 192L);
				}
			}
		}

		public unsafe void Update(void* input, int length)
		{
			byte* ptr = (byte*)input;
			byte* ptr2 = ptr + length;
			int isHash = State.IsHash64;
			byte* secretKey = SecretKey;
			State.TotalLength += length;
			if (State.BufferedSize + length <= INTERNAL_BUFFER_SIZE)
			{
				UnsafeUtility.MemCpy(Buffer + State.BufferedSize, ptr, length);
				State.BufferedSize += length;
				return;
			}
			if (State.BufferedSize != 0)
			{
				int num = INTERNAL_BUFFER_SIZE - State.BufferedSize;
				UnsafeUtility.MemCpy(Buffer + State.BufferedSize, ptr, num);
				ptr += num;
				ConsumeStripes(Acc, ref State.NbStripesSoFar, Buffer, INTERNAL_BUFFER_STRIPES, secretKey, isHash);
				State.BufferedSize = 0;
			}
			if (ptr + INTERNAL_BUFFER_SIZE < ptr2)
			{
				byte* ptr3 = ptr2 - INTERNAL_BUFFER_SIZE;
				do
				{
					ConsumeStripes(Acc, ref State.NbStripesSoFar, ptr, INTERNAL_BUFFER_STRIPES, secretKey, isHash);
					ptr += INTERNAL_BUFFER_SIZE;
				}
				while (ptr < ptr3);
				UnsafeUtility.MemCpy(Buffer + INTERNAL_BUFFER_SIZE - 64, ptr - 64, 64L);
			}
			if (ptr < ptr2)
			{
				long num2 = ptr2 - ptr;
				UnsafeUtility.MemCpy(Buffer, ptr, num2);
				State.BufferedSize = (int)num2;
			}
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe void Update<T>(in T input) where T : unmanaged
		{
			Update(UnsafeUtilityExtensions.AddressOf(in input), UnsafeUtility.SizeOf<T>());
		}

		public unsafe uint4 DigestHash128()
		{
			byte* secretKey = SecretKey;
			uint4 result;
			if (State.TotalLength > 240)
			{
				ulong* acc = stackalloc ulong[8];
				DigestLong(acc, secretKey, 0);
				ulong ul = MergeAcc(acc, secretKey + 11, (ulong)(State.TotalLength * -7046029288634856825L));
				ulong ul2 = MergeAcc(acc, secretKey + SECRET_LIMIT - 11, (ulong)(~(State.TotalLength * -4417276706812531889L)));
				result = ToUint4(ul, ul2);
			}
			else
			{
				result = Hash128(Buffer, State.TotalLength, State.Seed);
			}
			Reset(State.IsHash64 == 1, State.Seed);
			return result;
		}

		public unsafe uint2 DigestHash64()
		{
			byte* secretKey = SecretKey;
			uint2 result;
			if (State.TotalLength > 240)
			{
				ulong* acc = stackalloc ulong[8];
				DigestLong(acc, secretKey, 1);
				result = ToUint2(MergeAcc(acc, secretKey + 11, (ulong)(State.TotalLength * -7046029288634856825L)));
			}
			else
			{
				result = Hash64(Buffer, State.TotalLength, State.Seed);
			}
			Reset(State.IsHash64 == 1, State.Seed);
			return result;
		}

		private unsafe void DigestLong(ulong* acc, byte* secret, int isHash64)
		{
			UnsafeUtility.MemCpy(acc, Acc, 64L);
			if (State.BufferedSize >= 64)
			{
				int num = (State.BufferedSize - 1) / 64;
				ConsumeStripes(acc, ref State.NbStripesSoFar, Buffer, num, secret, isHash64);
				if (X86.Avx2.IsAvx2Supported)
				{
					Avx2Accumulate512(acc, Buffer + State.BufferedSize - 64, null, secret + SECRET_LIMIT - 7);
				}
				else
				{
					DefaultAccumulate512(acc, Buffer + State.BufferedSize - 64, null, secret + SECRET_LIMIT - 7, isHash64);
				}
				return;
			}
			byte* ptr = stackalloc byte[64];
			int num2 = 64 - State.BufferedSize;
			UnsafeUtility.MemCpy(ptr, Buffer + INTERNAL_BUFFER_SIZE - num2, num2);
			UnsafeUtility.MemCpy(ptr + num2, Buffer, State.BufferedSize);
			if (X86.Avx2.IsAvx2Supported)
			{
				Avx2Accumulate512(acc, ptr, null, secret + SECRET_LIMIT - 7);
			}
			else
			{
				DefaultAccumulate512(acc, ptr, null, secret + SECRET_LIMIT - 7, isHash64);
			}
		}

		private unsafe void ConsumeStripes(ulong* acc, ref int nbStripesSoFar, byte* input, long totalStripes, byte* secret, int isHash64)
		{
			if (NB_STRIPES_PER_BLOCK - nbStripesSoFar <= totalStripes)
			{
				int num = NB_STRIPES_PER_BLOCK - nbStripesSoFar;
				if (X86.Avx2.IsAvx2Supported)
				{
					Avx2Accumulate(acc, input, null, secret + nbStripesSoFar * 8, num, isHash64);
					Avx2ScrambleAcc(acc, secret + SECRET_LIMIT);
					Avx2Accumulate(acc, input + num * 64, null, secret, totalStripes - num, isHash64);
				}
				else
				{
					DefaultAccumulate(acc, input, null, secret + nbStripesSoFar * 8, num, isHash64);
					DefaultScrambleAcc(acc, secret + SECRET_LIMIT);
					DefaultAccumulate(acc, input + num * 64, null, secret, totalStripes - num, isHash64);
				}
				nbStripesSoFar = (int)totalStripes - num;
			}
			else
			{
				if (X86.Avx2.IsAvx2Supported)
				{
					Avx2Accumulate(acc, input, null, secret + nbStripesSoFar * 8, totalStripes, isHash64);
				}
				else
				{
					DefaultAccumulate(acc, input, null, secret + nbStripesSoFar * 8, totalStripes, isHash64);
				}
				nbStripesSoFar += (int)totalStripes;
			}
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void CheckKeySize(int isHash64)
		{
			if (State.IsHash64 != isHash64)
			{
				string text = ((State.IsHash64 != 0) ? "64" : "128");
				throw new InvalidOperationException("The streaming state was create for " + text + " bits hash key, the calling method doesn't support this key size, please use the appropriate API");
			}
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate ulong Hash64Long_00000A6B_0024PostfixBurstDelegate(byte* input, byte* dest, long length, byte* secret);

	internal static class Hash64Long_00000A6B_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Hash64Long_00000A6B_0024PostfixBurstDelegate>(Hash64Long).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static ulong Invoke(byte* input, byte* dest, long length, byte* secret)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<byte*, byte*, long, byte*, ulong>)functionPointer)(input, dest, length, secret);
				}
			}
			return Hash64Long_0024BurstManaged(input, dest, length, secret);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate void Hash128Long_00000A72_0024PostfixBurstDelegate(byte* input, byte* dest, long length, byte* secret, out uint4 result);

	internal static class Hash128Long_00000A72_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private unsafe static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Hash128Long_00000A72_0024PostfixBurstDelegate>(Hash128Long).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(byte* input, byte* dest, long length, byte* secret, out uint4 result)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<byte*, byte*, long, byte*, ref uint4, void>)functionPointer)(input, dest, length, secret, ref result);
					return;
				}
			}
			Hash128Long_0024BurstManaged(input, dest, length, secret, out result);
		}
	}

	private const int STRIPE_LEN = 64;

	private const int ACC_NB = 8;

	private const int SECRET_CONSUME_RATE = 8;

	private const int SECRET_KEY_SIZE = 192;

	private const int SECRET_KEY_MIN_SIZE = 136;

	private const int SECRET_LASTACC_START = 7;

	private const int NB_ROUNDS = 16;

	private const int BLOCK_LEN = 1024;

	private const uint PRIME32_1 = 2654435761u;

	private const uint PRIME32_2 = 2246822519u;

	private const uint PRIME32_3 = 3266489917u;

	private const uint PRIME32_5 = 374761393u;

	private const ulong PRIME64_1 = 11400714785074694791uL;

	private const ulong PRIME64_2 = 14029467366897019727uL;

	private const ulong PRIME64_3 = 1609587929392839161uL;

	private const ulong PRIME64_4 = 9650029242287828579uL;

	private const ulong PRIME64_5 = 2870177450012600261uL;

	private const int MIDSIZE_MAX = 240;

	private const int MIDSIZE_STARTOFFSET = 3;

	private const int MIDSIZE_LASTOFFSET = 17;

	private const int SECRET_MERGEACCS_START = 11;

	internal unsafe static void Avx2HashLongInternalLoop(ulong* acc, byte* input, byte* dest, long length, byte* secret, int isHash64)
	{
		if (!X86.Avx2.IsAvx2Supported)
		{
			return;
		}
		long num = (length - 1) / 1024;
		for (int i = 0; i < num; i++)
		{
			Avx2Accumulate(acc, input + i * 1024, (dest == null) ? null : (dest + i * 1024), secret, 16L, isHash64);
			Avx2ScrambleAcc(acc, secret + 192 - 64);
		}
		long nbStripes = (length - 1 - 1024 * num) / 64;
		Avx2Accumulate(acc, input + num * 1024, (dest == null) ? null : (dest + num * 1024), secret, nbStripes, isHash64);
		byte* input2 = input + length - 64;
		Avx2Accumulate512(acc, input2, null, secret + 192 - 64 - 7);
		if (dest != null)
		{
			long num2 = length % 64;
			if (num2 != 0L)
			{
				UnsafeUtility.MemCpy(dest + length - num2, input + length - num2, num2);
			}
		}
	}

	internal unsafe static void Avx2ScrambleAcc(ulong* acc, byte* secret)
	{
		if (X86.Avx2.IsAvx2Supported)
		{
			v256 b = X86.Avx.mm256_set1_epi32(-1640531535);
			v256 a = *(v256*)acc;
			v256 b2 = X86.Avx2.mm256_srli_epi64(a, 47);
			v256 a2 = X86.Avx2.mm256_xor_si256(a, b2);
			v256 b3 = X86.Avx.mm256_loadu_si256(secret);
			v256 a3 = X86.Avx2.mm256_xor_si256(a2, b3);
			v256 a4 = X86.Avx2.mm256_shuffle_epi32(a3, X86.Sse.SHUFFLE(0, 3, 0, 1));
			v256 a5 = X86.Avx2.mm256_mul_epu32(a3, b);
			v256 a6 = X86.Avx2.mm256_mul_epu32(a4, b);
			*(v256*)acc = X86.Avx2.mm256_add_epi64(a5, X86.Avx2.mm256_slli_epi64(a6, 32));
			v256 a7 = *(v256*)((byte*)acc + sizeof(v256));
			b2 = X86.Avx2.mm256_srli_epi64(a7, 47);
			v256 a8 = X86.Avx2.mm256_xor_si256(a7, b2);
			b3 = X86.Avx.mm256_loadu_si256(secret + sizeof(v256));
			v256 a9 = X86.Avx2.mm256_xor_si256(a8, b3);
			a4 = X86.Avx2.mm256_shuffle_epi32(a9, X86.Sse.SHUFFLE(0, 3, 0, 1));
			a5 = X86.Avx2.mm256_mul_epu32(a9, b);
			a6 = X86.Avx2.mm256_mul_epu32(a4, b);
			*(v256*)((byte*)acc + sizeof(v256)) = X86.Avx2.mm256_add_epi64(a5, X86.Avx2.mm256_slli_epi64(a6, 32));
		}
	}

	internal unsafe static void Avx2Accumulate(ulong* acc, byte* input, byte* dest, byte* secret, long nbStripes, int isHash64)
	{
		if (X86.Avx2.IsAvx2Supported)
		{
			for (int i = 0; i < nbStripes; i++)
			{
				byte* input2 = input + i * 64;
				Avx2Accumulate512(acc, input2, (dest == null) ? null : (dest + i * 64), secret + i * 8);
			}
		}
	}

	internal unsafe static void Avx2Accumulate512(ulong* acc, byte* input, byte* dest, byte* secret)
	{
		if (X86.Avx2.IsAvx2Supported)
		{
			v256 v = X86.Avx.mm256_loadu_si256(input);
			v256 b = X86.Avx.mm256_loadu_si256(secret);
			v256 a = X86.Avx2.mm256_xor_si256(v, b);
			if (dest != null)
			{
				X86.Avx.mm256_storeu_si256(dest, v);
			}
			v256 b2 = X86.Avx2.mm256_shuffle_epi32(a, X86.Sse.SHUFFLE(0, 3, 0, 1));
			v256 a2 = X86.Avx2.mm256_mul_epu32(a, b2);
			v256 b3 = X86.Avx2.mm256_shuffle_epi32(v, X86.Sse.SHUFFLE(1, 0, 3, 2));
			v256 b4 = X86.Avx2.mm256_add_epi64(*(v256*)acc, b3);
			*(v256*)acc = X86.Avx2.mm256_add_epi64(a2, b4);
			v = X86.Avx.mm256_loadu_si256(input + sizeof(v256));
			b = X86.Avx.mm256_loadu_si256(secret + sizeof(v256));
			v256 a3 = X86.Avx2.mm256_xor_si256(v, b);
			if (dest != null)
			{
				X86.Avx.mm256_storeu_si256(dest + 32, v);
			}
			b2 = X86.Avx2.mm256_shuffle_epi32(a3, X86.Sse.SHUFFLE(0, 3, 0, 1));
			a2 = X86.Avx2.mm256_mul_epu32(a3, b2);
			b3 = X86.Avx2.mm256_shuffle_epi32(v, X86.Sse.SHUFFLE(1, 0, 3, 2));
			b4 = X86.Avx2.mm256_add_epi64(*(v256*)((byte*)acc + sizeof(v256)), b3);
			*(v256*)((byte*)acc + sizeof(v256)) = X86.Avx2.mm256_add_epi64(a2, b4);
		}
	}

	public unsafe static uint2 Hash64(void* input, long length)
	{
		fixed (byte* ptr = xxHashDefaultKey.kSecret)
		{
			void* secret = ptr;
			return ToUint2(Hash64Internal((byte*)input, null, length, (byte*)secret, 0uL));
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static uint2 Hash64<T>(in T input) where T : unmanaged
	{
		return Hash64(UnsafeUtilityExtensions.AddressOf(in input), UnsafeUtility.SizeOf<T>());
	}

	public unsafe static uint2 Hash64(void* input, long length, ulong seed)
	{
		fixed (byte* secret = xxHashDefaultKey.kSecret)
		{
			return ToUint2(Hash64Internal((byte*)input, null, length, secret, seed));
		}
	}

	public unsafe static uint4 Hash128(void* input, long length)
	{
		fixed (byte* ptr = xxHashDefaultKey.kSecret)
		{
			void* secret = ptr;
			Hash128Internal((byte*)input, null, length, (byte*)secret, 0uL, out var result);
			return result;
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static uint4 Hash128<T>(in T input) where T : unmanaged
	{
		return Hash128(UnsafeUtilityExtensions.AddressOf(in input), UnsafeUtility.SizeOf<T>());
	}

	public unsafe static uint4 Hash128(void* input, void* destination, long length)
	{
		fixed (byte* secret = xxHashDefaultKey.kSecret)
		{
			Hash128Internal((byte*)input, (byte*)destination, length, secret, 0uL, out var result);
			return result;
		}
	}

	public unsafe static uint4 Hash128(void* input, long length, ulong seed)
	{
		fixed (byte* secret = xxHashDefaultKey.kSecret)
		{
			Hash128Internal((byte*)input, null, length, secret, seed, out var result);
			return result;
		}
	}

	public unsafe static uint4 Hash128(void* input, void* destination, long length, ulong seed)
	{
		fixed (byte* secret = xxHashDefaultKey.kSecret)
		{
			Hash128Internal((byte*)input, (byte*)destination, length, secret, seed, out var result);
			return result;
		}
	}

	internal unsafe static ulong Hash64Internal(byte* input, byte* dest, long length, byte* secret, ulong seed)
	{
		if (dest != null && length < 240)
		{
			UnsafeUtility.MemCpy(dest, input, length);
		}
		if (length <= 16)
		{
			return Hash64Len0To16(input, length, secret, seed);
		}
		if (length <= 128)
		{
			return Hash64Len17To128(input, length, secret, seed);
		}
		if (length <= 240)
		{
			return Hash64Len129To240(input, length, secret, seed);
		}
		if (seed != 0L)
		{
			byte* ptr = (byte*)(((ulong)stackalloc byte[223] + 31uL) & 0xFFFFFFFFFFFFFFE0uL);
			EncodeSecretKey(ptr, secret, seed);
			return Hash64Long(input, dest, length, ptr);
		}
		return Hash64Long(input, dest, length, secret);
	}

	internal unsafe static void Hash128Internal(byte* input, byte* dest, long length, byte* secret, ulong seed, out uint4 result)
	{
		if (dest != null && length < 240)
		{
			UnsafeUtility.MemCpy(dest, input, length);
		}
		if (length <= 16)
		{
			Hash128Len0To16(input, length, secret, seed, out result);
		}
		else if (length <= 128)
		{
			Hash128Len17To128(input, length, secret, seed, out result);
		}
		else if (length <= 240)
		{
			Hash128Len129To240(input, length, secret, seed, out result);
		}
		else if (seed != 0L)
		{
			byte* ptr = (byte*)(((ulong)stackalloc byte[223] + 31uL) & 0xFFFFFFFFFFFFFFE0uL);
			EncodeSecretKey(ptr, secret, seed);
			Hash128Long(input, dest, length, ptr, out result);
		}
		else
		{
			Hash128Long(input, dest, length, secret, out result);
		}
	}

	private unsafe static ulong Hash64Len1To3(byte* input, long len, byte* secret, ulong seed)
	{
		byte num = *input;
		byte b = input[len >> 1];
		byte b2 = input[len - 1];
		int num2 = (num << 16) | (b << 24) | b2 | ((int)len << 8);
		ulong num3 = (Read32LE(secret) ^ Read32LE(secret + 4)) + seed;
		return AvalancheH64((uint)num2 ^ num3);
	}

	private unsafe static ulong Hash64Len4To8(byte* input, long length, byte* secret, ulong seed)
	{
		seed ^= (ulong)Swap32((uint)seed) << 32;
		uint num = Read32LE(input);
		uint num2 = Read32LE(input + length - 4);
		ulong num3 = (Read64LE(secret + 8) ^ Read64LE(secret + 16)) - seed;
		return rrmxmx((num2 + ((ulong)num << 32)) ^ num3, (ulong)length);
	}

	private unsafe static ulong Hash64Len9To16(byte* input, long length, byte* secret, ulong seed)
	{
		ulong num = (Read64LE(secret + 24) ^ Read64LE(secret + 32)) + seed;
		ulong num2 = (Read64LE(secret + 40) ^ Read64LE(secret + 48)) - seed;
		ulong num3 = Read64LE(input) ^ num;
		ulong num4 = Read64LE(input + length - 8) ^ num2;
		return Avalanche((ulong)(length + (long)Swap64(num3) + (long)num4) + Mul128Fold64(num3, num4));
	}

	private unsafe static ulong Hash64Len0To16(byte* input, long length, byte* secret, ulong seed)
	{
		if (length > 8)
		{
			return Hash64Len9To16(input, length, secret, seed);
		}
		if (length >= 4)
		{
			return Hash64Len4To8(input, length, secret, seed);
		}
		if (length > 0)
		{
			return Hash64Len1To3(input, length, secret, seed);
		}
		return AvalancheH64(seed ^ (Read64LE(secret + 56) ^ Read64LE(secret + 64)));
	}

	private unsafe static ulong Hash64Len17To128(byte* input, long length, byte* secret, ulong seed)
	{
		ulong num = (ulong)(length * -7046029288634856825L);
		if (length > 32)
		{
			if (length > 64)
			{
				if (length > 96)
				{
					num += Mix16(input + 48, secret + 96, seed);
					num += Mix16(input + length - 64, secret + 112, seed);
				}
				num += Mix16(input + 32, secret + 64, seed);
				num += Mix16(input + length - 48, secret + 80, seed);
			}
			num += Mix16(input + 16, secret + 32, seed);
			num += Mix16(input + length - 32, secret + 48, seed);
		}
		num += Mix16(input, secret, seed);
		num += Mix16(input + length - 16, secret + 16, seed);
		return Avalanche(num);
	}

	private unsafe static ulong Hash64Len129To240(byte* input, long length, byte* secret, ulong seed)
	{
		ulong num = (ulong)(length * -7046029288634856825L);
		int num2 = (int)length / 16;
		for (int i = 0; i < 8; i++)
		{
			num += Mix16(input + 16 * i, secret + 16 * i, seed);
		}
		num = Avalanche(num);
		for (int j = 8; j < num2; j++)
		{
			num += Mix16(input + 16 * j, secret + 16 * (j - 8) + 3, seed);
		}
		num += Mix16(input + length - 16, secret + 136 - 17, seed);
		return Avalanche(num);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(Unity_002ECollections_002EHash64Long_00000A6B_0024PostfixBurstDelegate))]
	private unsafe static ulong Hash64Long(byte* input, byte* dest, long length, byte* secret)
	{
		return Hash64Long_00000A6B_0024BurstDirectCall.Invoke(input, dest, length, secret);
	}

	private unsafe static void Hash128Len1To3(byte* input, long length, byte* secret, ulong seed, out uint4 result)
	{
		byte num = *input;
		byte b = input[length >> 1];
		byte b2 = input[length - 1];
		int num2 = (num << 16) + (b << 24) + b2 + ((int)length << 8);
		uint num3 = RotL32(Swap32((uint)num2), 13);
		ulong num4 = (Read32LE(secret) ^ Read32LE(secret + 4)) + seed;
		ulong num5 = (Read32LE(secret + 8) ^ Read32LE(secret + 12)) - seed;
		ulong h = (uint)num2 ^ num4;
		ulong h2 = num3 ^ num5;
		result = ToUint4(AvalancheH64(h), AvalancheH64(h2));
	}

	private unsafe static void Hash128Len4To8(byte* input, long len, byte* secret, ulong seed, out uint4 result)
	{
		seed ^= (ulong)Swap32((uint)seed) << 32;
		uint num = Read32LE(input);
		uint num2 = Read32LE(input + len - 4);
		ulong num3 = num + ((ulong)num2 << 32);
		ulong num4 = (Read64LE(secret + 16) ^ Read64LE(secret + 24)) + seed;
		ulong high;
		ulong num5 = Common.umul128(num3 ^ num4, (ulong)(-7046029288634856825L + (len << 2)), out high);
		high += num5 << 1;
		num5 ^= high >> 3;
		num5 = XorShift64(num5, 35);
		num5 *= 11507291218515648293uL;
		num5 = XorShift64(num5, 28);
		high = Avalanche(high);
		result = ToUint4(num5, high);
	}

	private unsafe static void Hash128Len9To16(byte* input, long len, byte* secret, ulong seed, out uint4 result)
	{
		ulong num = (Read64LE(secret + 32) ^ Read64LE(secret + 40)) - seed;
		ulong num2 = (Read64LE(secret + 48) ^ Read64LE(secret + 56)) + seed;
		ulong num3 = Read64LE(input);
		ulong num4 = Read64LE(input + len - 8);
		ulong high;
		long num5 = (long)Common.umul128(num3 ^ num4 ^ num, 11400714785074694791uL, out high) + (len - 1 << 54);
		num4 ^= num2;
		high += num4 + Mul32To64((uint)num4, 2246822518u);
		ulong high2;
		ulong h = Common.umul128((ulong)num5 ^ Swap64(high), 14029467366897019727uL, out high2);
		high2 += (ulong)((long)high * -4417276706812531889L);
		result = ToUint4(Avalanche(h), Avalanche(high2));
	}

	private unsafe static void Hash128Len0To16(byte* input, long length, byte* secret, ulong seed, out uint4 result)
	{
		if (length > 8)
		{
			Hash128Len9To16(input, length, secret, seed, out result);
			return;
		}
		if (length >= 4)
		{
			Hash128Len4To8(input, length, secret, seed, out result);
			return;
		}
		if (length > 0)
		{
			Hash128Len1To3(input, length, secret, seed, out result);
			return;
		}
		ulong num = Read64LE(secret + 64) ^ Read64LE(secret + 72);
		ulong num2 = Read64LE(secret + 80) ^ Read64LE(secret + 88);
		ulong ul = AvalancheH64(seed ^ num);
		ulong ul2 = AvalancheH64(seed ^ num2);
		result = ToUint4(ul, ul2);
	}

	private unsafe static void Hash128Len17To128(byte* input, long length, byte* secret, ulong seed, out uint4 result)
	{
		ulong2 acc = new ulong2((ulong)(length * -7046029288634856825L), 0uL);
		if (length > 32)
		{
			if (length > 64)
			{
				if (length > 96)
				{
					acc = Mix32(acc, input + 48, input + length - 64, secret + 96, seed);
				}
				acc = Mix32(acc, input + 32, input + length - 48, secret + 64, seed);
			}
			acc = Mix32(acc, input + 16, input + length - 32, secret + 32, seed);
		}
		acc = Mix32(acc, input, input + length - 16, secret, seed);
		ulong h = acc.x + acc.y;
		ulong h2 = (ulong)((long)acc.x * -7046029288634856825L + (long)acc.y * -8796714831421723037L + (length - (long)seed) * -4417276706812531889L);
		result = ToUint4(Avalanche(h), 0 - Avalanche(h2));
	}

	private unsafe static void Hash128Len129To240(byte* input, long length, byte* secret, ulong seed, out uint4 result)
	{
		ulong2 acc = new ulong2((ulong)(length * -7046029288634856825L), 0uL);
		long num = length / 32;
		for (int i = 0; i < 4; i++)
		{
			acc = Mix32(acc, input + 32 * i, input + 32 * i + 16, secret + 32 * i, seed);
		}
		acc.x = Avalanche(acc.x);
		acc.y = Avalanche(acc.y);
		for (int i = 4; i < num; i++)
		{
			acc = Mix32(acc, input + 32 * i, input + 32 * i + 16, secret + 3 + 32 * (i - 4), seed);
		}
		acc = Mix32(acc, input + length - 16, input + length - 32, secret + 136 - 17 - 16, 0 - seed);
		ulong h = acc.x + acc.y;
		ulong h2 = (ulong)((long)acc.x * -7046029288634856825L + (long)acc.y * -8796714831421723037L + (length - (long)seed) * -4417276706812531889L);
		result = ToUint4(Avalanche(h), 0 - Avalanche(h2));
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(Unity_002ECollections_002EHash128Long_00000A72_0024PostfixBurstDelegate))]
	private unsafe static void Hash128Long(byte* input, byte* dest, long length, byte* secret, out uint4 result)
	{
		Hash128Long_00000A72_0024BurstDirectCall.Invoke(input, dest, length, secret, out result);
	}

	internal static uint2 ToUint2(ulong u)
	{
		return new uint2((uint)(u & 0xFFFFFFFFu), (uint)(u >> 32));
	}

	internal static uint4 ToUint4(ulong ul0, ulong ul1)
	{
		return new uint4((uint)(ul0 & 0xFFFFFFFFu), (uint)(ul0 >> 32), (uint)(ul1 & 0xFFFFFFFFu), (uint)(ul1 >> 32));
	}

	internal unsafe static void EncodeSecretKey(byte* dst, byte* secret, ulong seed)
	{
		int num = 12;
		for (int i = 0; i < num; i++)
		{
			Write64LE(dst + 16 * i, Read64LE(secret + 16 * i) + seed);
			Write64LE(dst + 16 * i + 8, Read64LE(secret + 16 * i + 8) - seed);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static ulong Read64LE(void* addr)
	{
		return *(ulong*)addr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static uint Read32LE(void* addr)
	{
		return *(uint*)addr;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static void Write64LE(void* addr, ulong value)
	{
		*(ulong*)addr = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static void Read32LE(void* addr, uint value)
	{
		*(uint*)addr = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong Mul32To64(uint x, uint y)
	{
		return (ulong)x * (ulong)y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong Swap64(ulong x)
	{
		return ((x << 56) & 0xFF00000000000000uL) | ((x << 40) & 0xFF000000000000uL) | ((x << 24) & 0xFF0000000000uL) | ((x << 8) & 0xFF00000000uL) | ((x >> 8) & 0xFF000000u) | ((x >> 24) & 0xFF0000) | ((x >> 40) & 0xFF00) | ((x >> 56) & 0xFF);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint Swap32(uint x)
	{
		return ((x << 24) & 0xFF000000u) | ((x << 8) & 0xFF0000u) | ((x >> 8) & 0xFF00u) | ((x >> 24) & 0xFFu);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static uint RotL32(uint x, int r)
	{
		return (x << r) | (x >> 32 - r);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong RotL64(ulong x, int r)
	{
		return (x << r) | (x >> 64 - r);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong XorShift64(ulong v64, int shift)
	{
		return v64 ^ (v64 >> shift);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong Mul128Fold64(ulong lhs, ulong rhs)
	{
		ulong high;
		return Common.umul128(lhs, rhs, out high) ^ high;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static ulong Mix16(byte* input, byte* secret, ulong seed)
	{
		ulong num = Read64LE(input);
		return Mul128Fold64(rhs: Read64LE(input + 8) ^ (Read64LE(secret + 8) - seed), lhs: num ^ (Read64LE(secret) + seed));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static ulong2 Mix32(ulong2 acc, byte* input_1, byte* input_2, byte* secret, ulong seed)
	{
		ulong x = (acc.x + Mix16(input_1, secret, seed)) ^ (Read64LE(input_2) + Read64LE(input_2 + 8));
		ulong num = acc.y + Mix16(input_2, secret + 16, seed);
		num ^= Read64LE(input_1) + Read64LE(input_1 + 8);
		return new ulong2(x, num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong Avalanche(ulong h64)
	{
		h64 = XorShift64(h64, 37);
		h64 *= 1609587791953885689L;
		h64 = XorShift64(h64, 32);
		return h64;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong AvalancheH64(ulong h64)
	{
		h64 ^= h64 >> 33;
		h64 *= 14029467366897019727uL;
		h64 ^= h64 >> 29;
		h64 *= 1609587929392839161L;
		h64 ^= h64 >> 32;
		return h64;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static ulong rrmxmx(ulong h64, ulong length)
	{
		h64 ^= RotL64(h64, 49) ^ RotL64(h64, 24);
		h64 *= 11507291218515648293uL;
		h64 ^= (h64 >> 35) + length;
		h64 *= 11507291218515648293uL;
		return XorShift64(h64, 28);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static ulong Mix2Acc(ulong acc0, ulong acc1, byte* secret)
	{
		return Mul128Fold64(acc0 ^ Read64LE(secret), acc1 ^ Read64LE(secret + 8));
	}

	internal unsafe static ulong MergeAcc(ulong* acc, byte* secret, ulong start)
	{
		return Avalanche(start + Mix2Acc(*acc, acc[1], secret) + Mix2Acc(acc[2], acc[3], secret + 16) + Mix2Acc(acc[4], acc[5], secret + 32) + Mix2Acc(acc[6], acc[7], secret + 48));
	}

	private unsafe static void DefaultHashLongInternalLoop(ulong* acc, byte* input, byte* dest, long length, byte* secret, int isHash64)
	{
		long num = (length - 1) / 1024;
		for (int i = 0; i < num; i++)
		{
			DefaultAccumulate(acc, input + i * 1024, (dest == null) ? null : (dest + i * 1024), secret, 16L, isHash64);
			DefaultScrambleAcc(acc, secret + 192 - 64);
		}
		long nbStripes = (length - 1 - 1024 * num) / 64;
		DefaultAccumulate(acc, input + num * 1024, (dest == null) ? null : (dest + num * 1024), secret, nbStripes, isHash64);
		byte* input2 = input + length - 64;
		DefaultAccumulate512(acc, input2, null, secret + 192 - 64 - 7, isHash64);
		if (dest != null)
		{
			long num2 = length % 64;
			if (num2 != 0L)
			{
				UnsafeUtility.MemCpy(dest + length - num2, input + length - num2, num2);
			}
		}
	}

	internal unsafe static void DefaultAccumulate(ulong* acc, byte* input, byte* dest, byte* secret, long nbStripes, int isHash64)
	{
		for (int i = 0; i < nbStripes; i++)
		{
			DefaultAccumulate512(acc, input + i * 64, (dest == null) ? null : (dest + i * 64), secret + i * 8, isHash64);
		}
	}

	internal unsafe static void DefaultAccumulate512(ulong* acc, byte* input, byte* dest, byte* secret, int isHash64)
	{
		int num = 8;
		for (int i = 0; i < num; i++)
		{
			ulong num2 = Read64LE(input + 8 * i);
			ulong num3 = num2 ^ Read64LE(secret + i * 8);
			if (dest != null)
			{
				Write64LE(dest + 8 * i, num2);
			}
			acc[i ^ 1] += num2;
			acc[i] += Mul32To64((uint)(num3 & 0xFFFFFFFFu), (uint)(num3 >> 32));
		}
	}

	internal unsafe static void DefaultScrambleAcc(ulong* acc, byte* secret)
	{
		for (int i = 0; i < 8; i++)
		{
			ulong num = Read64LE(secret + 8 * i);
			ulong v = acc[i];
			v = XorShift64(v, 47);
			v ^= num;
			v *= 2654435761u;
			acc[i] = v;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static ulong Hash64Long_0024BurstManaged(byte* input, byte* dest, long length, byte* secret)
	{
		ulong* ptr = (ulong*)(((ulong)stackalloc byte[95] + 31uL) & 0xFFFFFFFFFFFFFFE0uL);
		*ptr = 3266489917uL;
		ptr[1] = 11400714785074694791uL;
		ptr[2] = 14029467366897019727uL;
		ptr[3] = 1609587929392839161uL;
		ptr[4] = 9650029242287828579uL;
		ptr[5] = 2246822519uL;
		ptr[6] = 2870177450012600261uL;
		ptr[7] = 2654435761uL;
		if (X86.Avx2.IsAvx2Supported)
		{
			Avx2HashLongInternalLoop(ptr, input, dest, length, secret, 1);
		}
		else
		{
			DefaultHashLongInternalLoop(ptr, input, dest, length, secret, 1);
		}
		return MergeAcc(ptr, secret + 11, (ulong)(length * -7046029288634856825L));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal unsafe static void Hash128Long_0024BurstManaged(byte* input, byte* dest, long length, byte* secret, out uint4 result)
	{
		ulong* ptr = (ulong*)(((ulong)stackalloc byte[95] + 31uL) & 0xFFFFFFFFFFFFFFE0uL);
		*ptr = 3266489917uL;
		ptr[1] = 11400714785074694791uL;
		ptr[2] = 14029467366897019727uL;
		ptr[3] = 1609587929392839161uL;
		ptr[4] = 9650029242287828579uL;
		ptr[5] = 2246822519uL;
		ptr[6] = 2870177450012600261uL;
		ptr[7] = 2654435761uL;
		if (X86.Avx2.IsAvx2Supported)
		{
			Avx2HashLongInternalLoop(ptr, input, dest, length, secret, 0);
		}
		else
		{
			DefaultHashLongInternalLoop(ptr, input, dest, length, secret, 0);
		}
		ulong ul = MergeAcc(ptr, secret + 11, (ulong)(length * -7046029288634856825L));
		ulong ul2 = MergeAcc(ptr, secret + 192 - 64 - 11, (ulong)(~(length * -4417276706812531889L)));
		result = ToUint4(ul, ul2);
	}
}
