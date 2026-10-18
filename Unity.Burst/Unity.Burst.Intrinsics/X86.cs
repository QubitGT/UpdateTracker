using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;

namespace Unity.Burst.Intrinsics;

[BurstCompile]
public static class X86
{
	public static class Avx
	{
		public enum CMP
		{
			EQ_OQ,
			LT_OS,
			LE_OS,
			UNORD_Q,
			NEQ_UQ,
			NLT_US,
			NLE_US,
			ORD_Q,
			EQ_UQ,
			NGE_US,
			NGT_US,
			FALSE_OQ,
			NEQ_OQ,
			GE_OS,
			GT_OS,
			TRUE_UQ,
			EQ_OS,
			LT_OQ,
			LE_OQ,
			UNORD_S,
			NEQ_US,
			NLT_UQ,
			NLE_UQ,
			ORD_S,
			EQ_US,
			NGE_UQ,
			NGT_UQ,
			FALSE_OS,
			NEQ_OS,
			GE_OQ,
			GT_OQ,
			TRUE_US
		}

		public static bool IsAvxSupported => false;

		[DebuggerStepThrough]
		public static v256 mm256_add_pd(v256 a, v256 b)
		{
			return new v256(Sse2.add_pd(a.Lo128, b.Lo128), Sse2.add_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_add_ps(v256 a, v256 b)
		{
			return new v256(Sse.add_ps(a.Lo128, b.Lo128), Sse.add_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_addsub_pd(v256 a, v256 b)
		{
			return new v256(Sse3.addsub_pd(a.Lo128, b.Lo128), Sse3.addsub_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_addsub_ps(v256 a, v256 b)
		{
			return new v256(Sse3.addsub_ps(a.Lo128, b.Lo128), Sse3.addsub_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_and_pd(v256 a, v256 b)
		{
			return new v256(Sse2.and_pd(a.Lo128, b.Lo128), Sse2.and_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_and_ps(v256 a, v256 b)
		{
			return new v256(Sse.and_ps(a.Lo128, b.Lo128), Sse.and_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_andnot_pd(v256 a, v256 b)
		{
			return new v256(Sse2.andnot_pd(a.Lo128, b.Lo128), Sse2.andnot_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_andnot_ps(v256 a, v256 b)
		{
			return new v256(Sse.andnot_ps(a.Lo128, b.Lo128), Sse.andnot_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_blend_pd(v256 a, v256 b, int imm8)
		{
			return new v256(Sse4_1.blend_pd(a.Lo128, b.Lo128, imm8 & 3), Sse4_1.blend_pd(a.Hi128, b.Hi128, imm8 >> 2));
		}

		[DebuggerStepThrough]
		public static v256 mm256_blend_ps(v256 a, v256 b, int imm8)
		{
			return new v256(Sse4_1.blend_ps(a.Lo128, b.Lo128, imm8 & 0xF), Sse4_1.blend_ps(a.Hi128, b.Hi128, imm8 >> 4));
		}

		[DebuggerStepThrough]
		public static v256 mm256_blendv_pd(v256 a, v256 b, v256 mask)
		{
			return new v256(Sse4_1.blendv_pd(a.Lo128, b.Lo128, mask.Lo128), Sse4_1.blendv_pd(a.Hi128, b.Hi128, mask.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_blendv_ps(v256 a, v256 b, v256 mask)
		{
			return new v256(Sse4_1.blendv_ps(a.Lo128, b.Lo128, mask.Lo128), Sse4_1.blendv_ps(a.Hi128, b.Hi128, mask.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_div_pd(v256 a, v256 b)
		{
			return new v256(Sse2.div_pd(a.Lo128, b.Lo128), Sse2.div_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_div_ps(v256 a, v256 b)
		{
			return new v256(Sse.div_ps(a.Lo128, b.Lo128), Sse.div_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_dp_ps(v256 a, v256 b, int imm8)
		{
			return new v256(Sse4_1.dp_ps(a.Lo128, b.Lo128, imm8), Sse4_1.dp_ps(a.Hi128, b.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_hadd_pd(v256 a, v256 b)
		{
			return new v256(Sse3.hadd_pd(a.Lo128, b.Lo128), Sse3.hadd_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_hadd_ps(v256 a, v256 b)
		{
			return new v256(Sse3.hadd_ps(a.Lo128, b.Lo128), Sse3.hadd_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_hsub_pd(v256 a, v256 b)
		{
			return new v256(Sse3.hsub_pd(a.Lo128, b.Lo128), Sse3.hsub_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_hsub_ps(v256 a, v256 b)
		{
			return new v256(Sse3.hsub_ps(a.Lo128, b.Lo128), Sse3.hsub_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_max_pd(v256 a, v256 b)
		{
			return new v256(Sse2.max_pd(a.Lo128, b.Lo128), Sse2.max_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_max_ps(v256 a, v256 b)
		{
			return new v256(Sse.max_ps(a.Lo128, b.Lo128), Sse.max_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_min_pd(v256 a, v256 b)
		{
			return new v256(Sse2.min_pd(a.Lo128, b.Lo128), Sse2.min_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_min_ps(v256 a, v256 b)
		{
			return new v256(Sse.min_ps(a.Lo128, b.Lo128), Sse.min_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_mul_pd(v256 a, v256 b)
		{
			return new v256(Sse2.mul_pd(a.Lo128, b.Lo128), Sse2.mul_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_mul_ps(v256 a, v256 b)
		{
			return new v256(Sse.mul_ps(a.Lo128, b.Lo128), Sse.mul_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_or_pd(v256 a, v256 b)
		{
			return new v256(Sse2.or_pd(a.Lo128, b.Lo128), Sse2.or_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_or_ps(v256 a, v256 b)
		{
			return new v256(Sse.or_ps(a.Lo128, b.Lo128), Sse.or_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_shuffle_pd(v256 a, v256 b, int imm8)
		{
			return new v256(Sse2.shuffle_pd(a.Lo128, b.Lo128, imm8 & 3), Sse2.shuffle_pd(a.Hi128, b.Hi128, imm8 >> 2));
		}

		[DebuggerStepThrough]
		public static v256 mm256_shuffle_ps(v256 a, v256 b, int imm8)
		{
			return new v256(Sse.shuffle_ps(a.Lo128, b.Lo128, imm8), Sse.shuffle_ps(a.Hi128, b.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sub_pd(v256 a, v256 b)
		{
			return new v256(Sse2.sub_pd(a.Lo128, b.Lo128), Sse2.sub_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sub_ps(v256 a, v256 b)
		{
			return new v256(Sse.sub_ps(a.Lo128, b.Lo128), Sse.sub_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_xor_pd(v256 a, v256 b)
		{
			return new v256(Sse2.xor_pd(a.Lo128, b.Lo128), Sse2.xor_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_xor_ps(v256 a, v256 b)
		{
			return new v256(Sse.xor_ps(a.Lo128, b.Lo128), Sse.xor_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v128 cmp_pd(v128 a, v128 b, int imm8)
		{
			return (CMP)(imm8 & 0x1F) switch
			{
				CMP.EQ_OQ => Sse2.cmpeq_pd(a, b), 
				CMP.LT_OS => Sse2.cmplt_pd(a, b), 
				CMP.LE_OS => Sse2.cmple_pd(a, b), 
				CMP.UNORD_Q => Sse2.cmpunord_pd(a, b), 
				CMP.NEQ_UQ => Sse2.cmpneq_pd(a, b), 
				CMP.NLT_US => Sse2.cmpnlt_pd(a, b), 
				CMP.NLE_US => Sse2.cmpnle_pd(a, b), 
				CMP.ORD_Q => Sse2.cmpord_pd(a, b), 
				CMP.EQ_UQ => Sse2.or_pd(Sse2.cmpeq_pd(a, b), Sse2.cmpunord_pd(a, b)), 
				CMP.NGE_UQ => Sse2.or_pd(Sse2.cmpnge_pd(a, b), Sse2.cmpunord_pd(a, b)), 
				CMP.NGT_US => Sse2.or_pd(Sse2.cmpngt_pd(a, b), Sse2.cmpunord_pd(a, b)), 
				CMP.FALSE_OQ => default(v128), 
				CMP.NEQ_OQ => Sse2.and_pd(Sse2.cmpneq_pd(a, b), Sse2.cmpord_pd(a, b)), 
				CMP.GE_OS => Sse2.and_pd(Sse2.cmpge_pd(a, b), Sse2.cmpord_pd(a, b)), 
				CMP.GT_OS => Sse2.and_pd(Sse2.cmpgt_pd(a, b), Sse2.cmpord_pd(a, b)), 
				CMP.TRUE_UQ => new v128(-1), 
				CMP.EQ_OS => Sse2.and_pd(Sse2.cmpeq_pd(a, b), Sse2.cmpord_pd(a, b)), 
				CMP.LT_OQ => Sse2.and_pd(Sse2.cmplt_pd(a, b), Sse2.cmpord_pd(a, b)), 
				CMP.LE_OQ => Sse2.and_pd(Sse2.cmple_pd(a, b), Sse2.cmpord_pd(a, b)), 
				CMP.UNORD_S => Sse2.cmpunord_pd(a, b), 
				CMP.NEQ_US => Sse2.cmpneq_pd(a, b), 
				CMP.NLT_UQ => Sse2.or_pd(Sse2.cmpnlt_pd(a, b), Sse2.cmpunord_pd(a, b)), 
				CMP.NLE_UQ => Sse2.or_pd(Sse2.cmpnle_pd(a, b), Sse2.cmpunord_pd(a, b)), 
				CMP.ORD_S => Sse2.cmpord_pd(a, b), 
				CMP.EQ_US => Sse2.or_pd(Sse2.cmpeq_pd(a, b), Sse2.cmpunord_pd(a, b)), 
				CMP.NGE_US => Sse2.or_pd(Sse2.cmpnge_pd(a, b), Sse2.cmpunord_pd(a, b)), 
				CMP.NGT_UQ => Sse2.or_pd(Sse2.cmpngt_pd(a, b), Sse2.cmpunord_pd(a, b)), 
				CMP.FALSE_OS => default(v128), 
				CMP.NEQ_OS => Sse2.and_pd(Sse2.cmpneq_pd(a, b), Sse2.cmpord_pd(a, b)), 
				CMP.GE_OQ => Sse2.and_pd(Sse2.cmpge_pd(a, b), Sse2.cmpord_pd(a, b)), 
				CMP.GT_OQ => Sse2.and_pd(Sse2.cmpgt_pd(a, b), Sse2.cmpord_pd(a, b)), 
				_ => new v128(-1), 
			};
		}

		[DebuggerStepThrough]
		public static v256 mm256_cmp_pd(v256 a, v256 b, int imm8)
		{
			return new v256(cmp_pd(a.Lo128, b.Lo128, imm8), cmp_pd(a.Hi128, b.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v128 cmp_ps(v128 a, v128 b, int imm8)
		{
			return (CMP)(imm8 & 0x1F) switch
			{
				CMP.EQ_OQ => Sse.cmpeq_ps(a, b), 
				CMP.LT_OS => Sse.cmplt_ps(a, b), 
				CMP.LE_OS => Sse.cmple_ps(a, b), 
				CMP.UNORD_Q => Sse.cmpunord_ps(a, b), 
				CMP.NEQ_UQ => Sse.cmpneq_ps(a, b), 
				CMP.NLT_US => Sse.cmpnlt_ps(a, b), 
				CMP.NLE_US => Sse.cmpnle_ps(a, b), 
				CMP.ORD_Q => Sse.cmpord_ps(a, b), 
				CMP.EQ_UQ => Sse.or_ps(Sse.cmpeq_ps(a, b), Sse.cmpunord_ps(a, b)), 
				CMP.NGE_UQ => Sse.or_ps(Sse.cmpnge_ps(a, b), Sse.cmpunord_ps(a, b)), 
				CMP.NGT_US => Sse.or_ps(Sse.cmpngt_ps(a, b), Sse.cmpunord_ps(a, b)), 
				CMP.FALSE_OQ => default(v128), 
				CMP.NEQ_OQ => Sse.and_ps(Sse.cmpneq_ps(a, b), Sse.cmpord_ps(a, b)), 
				CMP.GE_OS => Sse.and_ps(Sse.cmpge_ps(a, b), Sse.cmpord_ps(a, b)), 
				CMP.GT_OS => Sse.and_ps(Sse.cmpgt_ps(a, b), Sse.cmpord_ps(a, b)), 
				CMP.TRUE_UQ => new v128(-1), 
				CMP.EQ_OS => Sse.and_ps(Sse.cmpeq_ps(a, b), Sse.cmpord_ps(a, b)), 
				CMP.LT_OQ => Sse.and_ps(Sse.cmplt_ps(a, b), Sse.cmpord_ps(a, b)), 
				CMP.LE_OQ => Sse.and_ps(Sse.cmple_ps(a, b), Sse.cmpord_ps(a, b)), 
				CMP.UNORD_S => Sse.cmpunord_ps(a, b), 
				CMP.NEQ_US => Sse.cmpneq_ps(a, b), 
				CMP.NLT_UQ => Sse.or_ps(Sse.cmpnlt_ps(a, b), Sse.cmpunord_ps(a, b)), 
				CMP.NLE_UQ => Sse.or_ps(Sse.cmpnle_ps(a, b), Sse.cmpunord_ps(a, b)), 
				CMP.ORD_S => Sse.cmpord_ps(a, b), 
				CMP.EQ_US => Sse.or_ps(Sse.cmpeq_ps(a, b), Sse.cmpunord_ps(a, b)), 
				CMP.NGE_US => Sse.or_ps(Sse.cmpnge_ps(a, b), Sse.cmpunord_ps(a, b)), 
				CMP.NGT_UQ => Sse.or_ps(Sse.cmpngt_ps(a, b), Sse.cmpunord_ps(a, b)), 
				CMP.FALSE_OS => default(v128), 
				CMP.NEQ_OS => Sse.and_ps(Sse.cmpneq_ps(a, b), Sse.cmpord_ps(a, b)), 
				CMP.GE_OQ => Sse.and_ps(Sse.cmpge_ps(a, b), Sse.cmpord_ps(a, b)), 
				CMP.GT_OQ => Sse.and_ps(Sse.cmpgt_ps(a, b), Sse.cmpord_ps(a, b)), 
				_ => new v128(-1), 
			};
		}

		[DebuggerStepThrough]
		public static v256 mm256_cmp_ps(v256 a, v256 b, int imm8)
		{
			return new v256(cmp_ps(a.Lo128, b.Lo128, imm8), cmp_ps(a.Hi128, b.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v128 cmp_sd(v128 a, v128 b, int imm8)
		{
			return new v128(cmp_pd(a, b, imm8).ULong0, a.ULong1);
		}

		[DebuggerStepThrough]
		public static v128 cmp_ss(v128 a, v128 b, int imm8)
		{
			return new v128(cmp_ps(a, b, imm8).UInt0, a.UInt1, a.UInt2, a.UInt3);
		}

		[DebuggerStepThrough]
		public static v256 mm256_cvtepi32_pd(v128 a)
		{
			return new v256((double)a.SInt0, (double)a.SInt1, (double)a.SInt2, (double)a.SInt3);
		}

		[DebuggerStepThrough]
		public static v256 mm256_cvtepi32_ps(v256 a)
		{
			return new v256(Sse2.cvtepi32_ps(a.Lo128), Sse2.cvtepi32_ps(a.Hi128));
		}

		[DebuggerStepThrough]
		public static v128 mm256_cvtpd_ps(v256 a)
		{
			v128 v257 = Sse2.cvtpd_ps(a.Lo128);
			v128 v258 = Sse2.cvtpd_ps(a.Hi128);
			return new v128(v257.Float0, v257.Float1, v258.Float0, v258.Float1);
		}

		[DebuggerStepThrough]
		public static v256 mm256_cvtps_epi32(v256 a)
		{
			return new v256(Sse2.cvtps_epi32(a.Lo128), Sse2.cvtps_epi32(a.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_cvtps_pd(v128 a)
		{
			return new v256(a.Float0, a.Float1, a.Float2, a.Float3);
		}

		[DebuggerStepThrough]
		public static v128 mm256_cvttpd_epi32(v256 a)
		{
			return new v128((int)a.Double0, (int)a.Double1, (int)a.Double2, (int)a.Double3);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v128 mm256_cvtpd_epi32(v256 a)
		{
			v128 v257 = Sse2.cvtpd_epi32(new v128(a.Double0, a.Double1));
			v128 v258 = Sse2.cvtpd_epi32(new v128(a.Double2, a.Double3));
			return new v128(v257.SInt0, v257.SInt1, v258.SInt0, v258.SInt1);
		}

		[DebuggerStepThrough]
		public static v256 mm256_cvttps_epi32(v256 a)
		{
			return new v256(Sse2.cvttps_epi32(a.Lo128), Sse2.cvttps_epi32(a.Hi128));
		}

		[DebuggerStepThrough]
		public static float mm256_cvtss_f32(v256 a)
		{
			return a.Float0;
		}

		[DebuggerStepThrough]
		public static v128 mm256_extractf128_ps(v256 a, int imm8)
		{
			if (imm8 == 0)
			{
				return a.Lo128;
			}
			return a.Hi128;
		}

		[DebuggerStepThrough]
		public static v128 mm256_extractf128_pd(v256 a, int imm8)
		{
			if (imm8 == 0)
			{
				return a.Lo128;
			}
			return a.Hi128;
		}

		[DebuggerStepThrough]
		public static v128 mm256_extractf128_si256(v256 a, int imm8)
		{
			if (imm8 == 0)
			{
				return a.Lo128;
			}
			return a.Hi128;
		}

		[DebuggerStepThrough]
		public static void mm256_zeroall()
		{
		}

		[DebuggerStepThrough]
		public static void mm256_zeroupper()
		{
		}

		[DebuggerStepThrough]
		public unsafe static v128 permutevar_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			uint* ptr = &result.UInt0;
			uint* ptr2 = &a.UInt0;
			int* ptr3 = &b.SInt0;
			for (int i = 0; i < 4; i++)
			{
				int num = ptr3[i] & 3;
				ptr[i] = ptr2[num];
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v256 mm256_permutevar_ps(v256 a, v256 b)
		{
			return new v256(permutevar_ps(a.Lo128, b.Lo128), permutevar_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v128 permute_ps(v128 a, int imm8)
		{
			return Sse2.shuffle_epi32(a, imm8);
		}

		[DebuggerStepThrough]
		public static v256 mm256_permute_ps(v256 a, int imm8)
		{
			return new v256(permute_ps(a.Lo128, imm8), permute_ps(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public unsafe static v128 permutevar_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			double* num = &result.Double0;
			double* ptr = &a.Double0;
			*num = ptr[(int)(b.SLong0 & 2) >> 1];
			num[1] = ptr[(int)(b.SLong1 & 2) >> 1];
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_permutevar_pd(v256 a, v256 b)
		{
			v256 result = default(v256);
			double* num = &result.Double0;
			double* ptr = &a.Double0;
			*num = ptr[(int)(b.SLong0 & 2) >> 1];
			num[1] = ptr[(int)(b.SLong1 & 2) >> 1];
			num[2] = ptr[2 + ((int)(b.SLong2 & 2) >> 1)];
			num[3] = ptr[2 + ((int)(b.SLong3 & 2) >> 1)];
			return result;
		}

		[DebuggerStepThrough]
		public static v256 mm256_permute_pd(v256 a, int imm8)
		{
			return new v256(permute_pd(a.Lo128, imm8 & 3), permute_pd(a.Hi128, imm8 >> 2));
		}

		[DebuggerStepThrough]
		public unsafe static v128 permute_pd(v128 a, int imm8)
		{
			v128 result = default(v128);
			double* num = &result.Double0;
			double* ptr = &a.Double0;
			*num = ptr[imm8 & 1];
			num[1] = ptr[(imm8 >> 1) & 1];
			return result;
		}

		private static v128 Select4(v256 src1, v256 src2, int control)
		{
			return (control & 3) switch
			{
				0 => src1.Lo128, 
				1 => src1.Hi128, 
				2 => src2.Lo128, 
				_ => src2.Hi128, 
			};
		}

		[DebuggerStepThrough]
		public static v256 mm256_permute2f128_ps(v256 a, v256 b, int imm8)
		{
			return new v256(Select4(a, b, imm8), Select4(a, b, imm8 >> 4));
		}

		[DebuggerStepThrough]
		public static v256 mm256_permute2f128_pd(v256 a, v256 b, int imm8)
		{
			return mm256_permute2f128_ps(a, b, imm8);
		}

		[DebuggerStepThrough]
		public static v256 mm256_permute2f128_si256(v256 a, v256 b, int imm8)
		{
			return mm256_permute2f128_ps(a, b, imm8);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_broadcast_ss(void* ptr)
		{
			return new v256(*(uint*)ptr);
		}

		[DebuggerStepThrough]
		public unsafe static v128 broadcast_ss(void* ptr)
		{
			return new v128(*(uint*)ptr);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_broadcast_sd(void* ptr)
		{
			return new v256(*(double*)ptr);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_broadcast_ps(void* ptr)
		{
			v128 obj = Sse.loadu_ps(ptr);
			return new v256(obj, obj);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_broadcast_pd(void* ptr)
		{
			return mm256_broadcast_ps(ptr);
		}

		[DebuggerStepThrough]
		public static v256 mm256_insertf128_ps(v256 a, v128 b, int imm8)
		{
			if ((imm8 & 1) == 0)
			{
				return new v256(b, a.Hi128);
			}
			return new v256(a.Lo128, b);
		}

		[DebuggerStepThrough]
		public static v256 mm256_insertf128_pd(v256 a, v128 b, int imm8)
		{
			return mm256_insertf128_ps(a, b, imm8);
		}

		[DebuggerStepThrough]
		public static v256 mm256_insertf128_si256(v256 a, v128 b, int imm8)
		{
			return mm256_insertf128_ps(a, b, imm8);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_load_ps(void* ptr)
		{
			return *(v256*)ptr;
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_store_ps(void* ptr, v256 val)
		{
			*(v256*)ptr = val;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_load_pd(void* ptr)
		{
			return mm256_load_ps(ptr);
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_store_pd(void* ptr, v256 a)
		{
			mm256_store_ps(ptr, a);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_loadu_pd(void* ptr)
		{
			return mm256_load_ps(ptr);
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_storeu_pd(void* ptr, v256 a)
		{
			mm256_store_ps(ptr, a);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_loadu_ps(void* ptr)
		{
			return mm256_load_ps(ptr);
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_storeu_ps(void* ptr, v256 a)
		{
			mm256_store_ps(ptr, a);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_load_si256(void* ptr)
		{
			return mm256_load_ps(ptr);
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_store_si256(void* ptr, v256 v)
		{
			mm256_store_ps(ptr, v);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_loadu_si256(void* ptr)
		{
			return mm256_load_ps(ptr);
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_storeu_si256(void* ptr, v256 v)
		{
			mm256_store_ps(ptr, v);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public unsafe static v256 mm256_loadu2_m128(void* hiaddr, void* loaddr)
		{
			return mm256_set_m128(Sse.loadu_ps(hiaddr), Sse.loadu_ps(loaddr));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public unsafe static v256 mm256_loadu2_m128d(void* hiaddr, void* loaddr)
		{
			return mm256_loadu2_m128(hiaddr, loaddr);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public unsafe static v256 mm256_loadu2_m128i(void* hiaddr, void* loaddr)
		{
			return mm256_loadu2_m128(hiaddr, loaddr);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set_m128(v128 hi, v128 lo)
		{
			return new v256(lo, hi);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public unsafe static void mm256_storeu2_m128(void* hiaddr, void* loaddr, v256 val)
		{
			Sse.storeu_ps(hiaddr, val.Hi128);
			Sse.storeu_ps(loaddr, val.Lo128);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public unsafe static void mm256_storeu2_m128d(void* hiaddr, void* loaddr, v256 val)
		{
			Sse.storeu_ps(hiaddr, val.Hi128);
			Sse.storeu_ps(loaddr, val.Lo128);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public unsafe static void mm256_storeu2_m128i(void* hiaddr, void* loaddr, v256 val)
		{
			Sse.storeu_ps(hiaddr, val.Hi128);
			Sse.storeu_ps(loaddr, val.Lo128);
		}

		[DebuggerStepThrough]
		public unsafe static v128 maskload_pd(void* mem_addr, v128 mask)
		{
			v128 result = default(v128);
			if (mask.SLong0 < 0)
			{
				result.ULong0 = *(ulong*)mem_addr;
			}
			if (mask.SLong1 < 0)
			{
				result.ULong1 = *(ulong*)((byte*)mem_addr + 8);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_maskload_pd(void* mem_addr, v256 mask)
		{
			return new v256(maskload_pd(mem_addr, mask.Lo128), maskload_pd((byte*)mem_addr + 16, mask.Hi128));
		}

		[DebuggerStepThrough]
		public unsafe static void maskstore_pd(void* mem_addr, v128 mask, v128 a)
		{
			if (mask.SLong0 < 0)
			{
				*(ulong*)mem_addr = a.ULong0;
			}
			if (mask.SLong1 < 0)
			{
				*(ulong*)((byte*)mem_addr + 8) = a.ULong1;
			}
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_maskstore_pd(void* mem_addr, v256 mask, v256 a)
		{
			maskstore_pd(mem_addr, mask.Lo128, a.Lo128);
			maskstore_pd((byte*)mem_addr + 16, mask.Hi128, a.Hi128);
		}

		[DebuggerStepThrough]
		public unsafe static v128 maskload_ps(void* mem_addr, v128 mask)
		{
			v128 result = default(v128);
			if (mask.SInt0 < 0)
			{
				result.UInt0 = *(uint*)mem_addr;
			}
			if (mask.SInt1 < 0)
			{
				result.UInt1 = *(uint*)((byte*)mem_addr + 4);
			}
			if (mask.SInt2 < 0)
			{
				result.UInt2 = *(uint*)((byte*)mem_addr + (nint)2 * (nint)4);
			}
			if (mask.SInt3 < 0)
			{
				result.UInt3 = *(uint*)((byte*)mem_addr + (nint)3 * (nint)4);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_maskload_ps(void* mem_addr, v256 mask)
		{
			return new v256(maskload_ps(mem_addr, mask.Lo128), maskload_ps((byte*)mem_addr + 16, mask.Hi128));
		}

		[DebuggerStepThrough]
		public unsafe static void maskstore_ps(void* mem_addr, v128 mask, v128 a)
		{
			if (mask.SInt0 < 0)
			{
				*(uint*)mem_addr = a.UInt0;
			}
			if (mask.SInt1 < 0)
			{
				*(uint*)((byte*)mem_addr + 4) = a.UInt1;
			}
			if (mask.SInt2 < 0)
			{
				*(uint*)((byte*)mem_addr + (nint)2 * (nint)4) = a.UInt2;
			}
			if (mask.SInt3 < 0)
			{
				*(uint*)((byte*)mem_addr + (nint)3 * (nint)4) = a.UInt3;
			}
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_maskstore_ps(void* mem_addr, v256 mask, v256 a)
		{
			maskstore_ps(mem_addr, mask.Lo128, a.Lo128);
			maskstore_ps((byte*)mem_addr + 16, mask.Hi128, a.Hi128);
		}

		[DebuggerStepThrough]
		public static v256 mm256_movehdup_ps(v256 a)
		{
			return new v256(a.UInt1, a.UInt1, a.UInt3, a.UInt3, a.UInt5, a.UInt5, a.UInt7, a.UInt7);
		}

		[DebuggerStepThrough]
		public static v256 mm256_moveldup_ps(v256 a)
		{
			return new v256(a.UInt0, a.UInt0, a.UInt2, a.UInt2, a.UInt4, a.UInt4, a.UInt6, a.UInt6);
		}

		[DebuggerStepThrough]
		public static v256 mm256_movedup_pd(v256 a)
		{
			return new v256(a.Double0, a.Double0, a.Double2, a.Double2);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_lddqu_si256(void* mem_addr)
		{
			return *(v256*)mem_addr;
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_stream_si256(void* mem_addr, v256 a)
		{
			*(v256*)mem_addr = a;
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_stream_pd(void* mem_addr, v256 a)
		{
			*(v256*)mem_addr = a;
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_stream_ps(void* mem_addr, v256 a)
		{
			*(v256*)mem_addr = a;
		}

		[DebuggerStepThrough]
		public static v256 mm256_rcp_ps(v256 a)
		{
			return new v256(Sse.rcp_ps(a.Lo128), Sse.rcp_ps(a.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_rsqrt_ps(v256 a)
		{
			return new v256(Sse.rsqrt_ps(a.Lo128), Sse.rsqrt_ps(a.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sqrt_pd(v256 a)
		{
			return new v256(Sse2.sqrt_pd(a.Lo128), Sse2.sqrt_pd(a.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sqrt_ps(v256 a)
		{
			return new v256(Sse.sqrt_ps(a.Lo128), Sse.sqrt_ps(a.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_round_pd(v256 a, int rounding)
		{
			return new v256(Sse4_1.round_pd(a.Lo128, rounding), Sse4_1.round_pd(a.Hi128, rounding));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v256 mm256_ceil_pd(v256 val)
		{
			return mm256_round_pd(val, 2);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v256 mm256_floor_pd(v256 val)
		{
			return mm256_round_pd(val, 1);
		}

		[DebuggerStepThrough]
		public static v256 mm256_round_ps(v256 a, int rounding)
		{
			return new v256(Sse4_1.round_ps(a.Lo128, rounding), Sse4_1.round_ps(a.Hi128, rounding));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v256 mm256_ceil_ps(v256 val)
		{
			return mm256_round_ps(val, 2);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v256 mm256_floor_ps(v256 val)
		{
			return mm256_round_ps(val, 1);
		}

		[DebuggerStepThrough]
		public static v256 mm256_unpackhi_pd(v256 a, v256 b)
		{
			return new v256(Sse2.unpackhi_pd(a.Lo128, b.Lo128), Sse2.unpackhi_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_unpacklo_pd(v256 a, v256 b)
		{
			return new v256(Sse2.unpacklo_pd(a.Lo128, b.Lo128), Sse2.unpacklo_pd(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v256 mm256_unpackhi_ps(v256 a, v256 b)
		{
			return new v256(Sse.unpackhi_ps(a.Lo128, b.Lo128), Sse.unpackhi_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v256 mm256_unpacklo_ps(v256 a, v256 b)
		{
			return new v256(Sse.unpacklo_ps(a.Lo128, b.Lo128), Sse.unpacklo_ps(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static int mm256_testz_si256(v256 a, v256 b)
		{
			return Sse4_1.testz_si128(a.Lo128, b.Lo128) & Sse4_1.testz_si128(a.Hi128, b.Hi128);
		}

		[DebuggerStepThrough]
		public static int mm256_testc_si256(v256 a, v256 b)
		{
			return Sse4_1.testc_si128(a.Lo128, b.Lo128) & Sse4_1.testc_si128(a.Hi128, b.Hi128);
		}

		[DebuggerStepThrough]
		public static int mm256_testnzc_si256(v256 a, v256 b)
		{
			int num = mm256_testz_si256(a, b);
			int num2 = mm256_testc_si256(a, b);
			return 1 - (num | num2);
		}

		[DebuggerStepThrough]
		public unsafe static int mm256_testz_pd(v256 a, v256 b)
		{
			ulong* ptr = &a.ULong0;
			ulong* ptr2 = &b.ULong0;
			for (int i = 0; i < 4; i++)
			{
				if ((ptr[i] & ptr2[i] & 0x8000000000000000uL) != 0L)
				{
					return 0;
				}
			}
			return 1;
		}

		[DebuggerStepThrough]
		public unsafe static int mm256_testc_pd(v256 a, v256 b)
		{
			ulong* ptr = &a.ULong0;
			ulong* ptr2 = &b.ULong0;
			for (int i = 0; i < 4; i++)
			{
				if ((~ptr[i] & ptr2[i] & 0x8000000000000000uL) != 0L)
				{
					return 0;
				}
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int mm256_testnzc_pd(v256 a, v256 b)
		{
			return 1 - (mm256_testz_pd(a, b) | mm256_testc_pd(a, b));
		}

		[DebuggerStepThrough]
		public unsafe static int testz_pd(v128 a, v128 b)
		{
			ulong* ptr = &a.ULong0;
			ulong* ptr2 = &b.ULong0;
			for (int i = 0; i < 2; i++)
			{
				if ((ptr[i] & ptr2[i] & 0x8000000000000000uL) != 0L)
				{
					return 0;
				}
			}
			return 1;
		}

		[DebuggerStepThrough]
		public unsafe static int testc_pd(v128 a, v128 b)
		{
			ulong* ptr = &a.ULong0;
			ulong* ptr2 = &b.ULong0;
			for (int i = 0; i < 2; i++)
			{
				if ((~ptr[i] & ptr2[i] & 0x8000000000000000uL) != 0L)
				{
					return 0;
				}
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int testnzc_pd(v128 a, v128 b)
		{
			return 1 - (testz_pd(a, b) | testc_pd(a, b));
		}

		[DebuggerStepThrough]
		public unsafe static int mm256_testz_ps(v256 a, v256 b)
		{
			uint* ptr = &a.UInt0;
			uint* ptr2 = &b.UInt0;
			for (int i = 0; i < 8; i++)
			{
				if ((ptr[i] & ptr2[i] & 0x80000000u) != 0)
				{
					return 0;
				}
			}
			return 1;
		}

		[DebuggerStepThrough]
		public unsafe static int mm256_testc_ps(v256 a, v256 b)
		{
			uint* ptr = &a.UInt0;
			uint* ptr2 = &b.UInt0;
			for (int i = 0; i < 8; i++)
			{
				if ((~ptr[i] & ptr2[i] & 0x80000000u) != 0)
				{
					return 0;
				}
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int mm256_testnzc_ps(v256 a, v256 b)
		{
			return 1 - (mm256_testz_ps(a, b) | mm256_testc_ps(a, b));
		}

		[DebuggerStepThrough]
		public unsafe static int testz_ps(v128 a, v128 b)
		{
			uint* ptr = &a.UInt0;
			uint* ptr2 = &b.UInt0;
			for (int i = 0; i < 4; i++)
			{
				if ((ptr[i] & ptr2[i] & 0x80000000u) != 0)
				{
					return 0;
				}
			}
			return 1;
		}

		[DebuggerStepThrough]
		public unsafe static int testc_ps(v128 a, v128 b)
		{
			uint* ptr = &a.UInt0;
			uint* ptr2 = &b.UInt0;
			for (int i = 0; i < 4; i++)
			{
				if ((~ptr[i] & ptr2[i] & 0x80000000u) != 0)
				{
					return 0;
				}
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int testnzc_ps(v128 a, v128 b)
		{
			return 1 - (testz_ps(a, b) | testc_ps(a, b));
		}

		[DebuggerStepThrough]
		public static int mm256_movemask_pd(v256 a)
		{
			return Sse2.movemask_pd(a.Lo128) | (Sse2.movemask_pd(a.Hi128) << 2);
		}

		[DebuggerStepThrough]
		public static int mm256_movemask_ps(v256 a)
		{
			return Sse.movemask_ps(a.Lo128) | (Sse.movemask_ps(a.Hi128) << 4);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setzero_pd()
		{
			return default(v256);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setzero_ps()
		{
			return default(v256);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setzero_si256()
		{
			return default(v256);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set_pd(double d, double c, double b, double a)
		{
			return new v256(a, b, c, d);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set_ps(float e7, float e6, float e5, float e4, float e3, float e2, float e1, float e0)
		{
			return new v256(e0, e1, e2, e3, e4, e5, e6, e7);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set_epi8(byte e31_, byte e30_, byte e29_, byte e28_, byte e27_, byte e26_, byte e25_, byte e24_, byte e23_, byte e22_, byte e21_, byte e20_, byte e19_, byte e18_, byte e17_, byte e16_, byte e15_, byte e14_, byte e13_, byte e12_, byte e11_, byte e10_, byte e9_, byte e8_, byte e7_, byte e6_, byte e5_, byte e4_, byte e3_, byte e2_, byte e1_, byte e0_)
		{
			return new v256(e0_, e1_, e2_, e3_, e4_, e5_, e6_, e7_, e8_, e9_, e10_, e11_, e12_, e13_, e14_, e15_, e16_, e17_, e18_, e19_, e20_, e21_, e22_, e23_, e24_, e25_, e26_, e27_, e28_, e29_, e30_, e31_);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set_epi16(short e15_, short e14_, short e13_, short e12_, short e11_, short e10_, short e9_, short e8_, short e7_, short e6_, short e5_, short e4_, short e3_, short e2_, short e1_, short e0_)
		{
			return new v256(e0_, e1_, e2_, e3_, e4_, e5_, e6_, e7_, e8_, e9_, e10_, e11_, e12_, e13_, e14_, e15_);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set_epi32(int e7, int e6, int e5, int e4, int e3, int e2, int e1, int e0)
		{
			return new v256(e0, e1, e2, e3, e4, e5, e6, e7);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set_epi64x(long e3, long e2, long e1, long e0)
		{
			return new v256(e0, e1, e2, e3);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set_m128d(v128 hi, v128 lo)
		{
			return new v256(lo, hi);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set_m128i(v128 hi, v128 lo)
		{
			return new v256(lo, hi);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setr_pd(double d, double c, double b, double a)
		{
			return new v256(d, c, b, a);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setr_ps(float e7, float e6, float e5, float e4, float e3, float e2, float e1, float e0)
		{
			return new v256(e7, e6, e5, e4, e3, e2, e1, e0);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setr_epi8(byte e31_, byte e30_, byte e29_, byte e28_, byte e27_, byte e26_, byte e25_, byte e24_, byte e23_, byte e22_, byte e21_, byte e20_, byte e19_, byte e18_, byte e17_, byte e16_, byte e15_, byte e14_, byte e13_, byte e12_, byte e11_, byte e10_, byte e9_, byte e8_, byte e7_, byte e6_, byte e5_, byte e4_, byte e3_, byte e2_, byte e1_, byte e0_)
		{
			return new v256(e31_, e30_, e29_, e28_, e27_, e26_, e25_, e24_, e23_, e22_, e21_, e20_, e19_, e18_, e17_, e16_, e15_, e14_, e13_, e12_, e11_, e10_, e9_, e8_, e7_, e6_, e5_, e4_, e3_, e2_, e1_, e0_);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setr_epi16(short e15_, short e14_, short e13_, short e12_, short e11_, short e10_, short e9_, short e8_, short e7_, short e6_, short e5_, short e4_, short e3_, short e2_, short e1_, short e0_)
		{
			return new v256(e15_, e14_, e13_, e12_, e11_, e10_, e9_, e8_, e7_, e6_, e5_, e4_, e3_, e2_, e1_, e0_);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setr_epi32(int e7, int e6, int e5, int e4, int e3, int e2, int e1, int e0)
		{
			return new v256(e7, e6, e5, e4, e3, e2, e1, e0);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setr_epi64x(long e3, long e2, long e1, long e0)
		{
			return new v256(e3, e2, e1, e0);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setr_m128(v128 hi, v128 lo)
		{
			return new v256(hi, lo);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setr_m128d(v128 hi, v128 lo)
		{
			return new v256(hi, lo);
		}

		[DebuggerStepThrough]
		public static v256 mm256_setr_m128i(v128 hi, v128 lo)
		{
			return new v256(hi, lo);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set1_pd(double a)
		{
			return new v256(a);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set1_ps(float a)
		{
			return new v256(a);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set1_epi8(byte a)
		{
			return new v256(a);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set1_epi16(short a)
		{
			return new v256(a);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set1_epi32(int a)
		{
			return new v256(a);
		}

		[DebuggerStepThrough]
		public static v256 mm256_set1_epi64x(long a)
		{
			return new v256(a);
		}

		[DebuggerStepThrough]
		public static v256 mm256_castpd_ps(v256 a)
		{
			return a;
		}

		[DebuggerStepThrough]
		public static v256 mm256_castps_pd(v256 a)
		{
			return a;
		}

		[DebuggerStepThrough]
		public static v256 mm256_castps_si256(v256 a)
		{
			return a;
		}

		[DebuggerStepThrough]
		public static v256 mm256_castpd_si256(v256 a)
		{
			return a;
		}

		[DebuggerStepThrough]
		public static v256 mm256_castsi256_ps(v256 a)
		{
			return a;
		}

		[DebuggerStepThrough]
		public static v256 mm256_castsi256_pd(v256 a)
		{
			return a;
		}

		[DebuggerStepThrough]
		public static v128 mm256_castps256_ps128(v256 a)
		{
			return a.Lo128;
		}

		[DebuggerStepThrough]
		public static v128 mm256_castpd256_pd128(v256 a)
		{
			return a.Lo128;
		}

		[DebuggerStepThrough]
		public static v128 mm256_castsi256_si128(v256 a)
		{
			return a.Lo128;
		}

		[DebuggerStepThrough]
		public static v256 mm256_castps128_ps256(v128 a)
		{
			return new v256(a, Sse.setzero_ps());
		}

		[DebuggerStepThrough]
		public static v256 mm256_castpd128_pd256(v128 a)
		{
			return new v256(a, Sse.setzero_ps());
		}

		[DebuggerStepThrough]
		public static v256 mm256_castsi128_si256(v128 a)
		{
			return new v256(a, Sse.setzero_ps());
		}

		[DebuggerStepThrough]
		public static v128 undefined_ps()
		{
			return default(v128);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v128 undefined_pd()
		{
			return undefined_ps();
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v128 undefined_si128()
		{
			return undefined_ps();
		}

		[DebuggerStepThrough]
		public static v256 mm256_undefined_ps()
		{
			return default(v256);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v256 mm256_undefined_pd()
		{
			return mm256_undefined_ps();
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v256 mm256_undefined_si256()
		{
			return mm256_undefined_ps();
		}

		[DebuggerStepThrough]
		public static v256 mm256_zextps128_ps256(v128 a)
		{
			return new v256(a, Sse.setzero_ps());
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v256 mm256_zextpd128_pd256(v128 a)
		{
			return mm256_zextps128_ps256(a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.AVX)]
		public static v256 mm256_zextsi128_si256(v128 a)
		{
			return mm256_zextps128_ps256(a);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_insert_epi8(v256 a, int i, int index)
		{
			v256 result = a;
			(&result.Byte0)[index & 0x1F] = (byte)i;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_insert_epi16(v256 a, int i, int index)
		{
			v256 result = a;
			(&result.SShort0)[index & 0xF] = (short)i;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_insert_epi32(v256 a, int i, int index)
		{
			v256 result = a;
			(&result.SInt0)[index & 7] = i;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_insert_epi64(v256 a, long i, int index)
		{
			v256 result = a;
			(&result.SLong0)[index & 3] = i;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static int mm256_extract_epi32(v256 a, int index)
		{
			return (&a.SInt0)[index & 7];
		}

		[DebuggerStepThrough]
		public unsafe static long mm256_extract_epi64(v256 a, int index)
		{
			return (&a.SLong0)[index & 3];
		}
	}

	public static class Avx2
	{
		public static bool IsAvx2Supported => false;

		[DebuggerStepThrough]
		public unsafe static int mm256_movemask_epi8(v256 a)
		{
			uint num = 0u;
			byte* ptr = &a.Byte0;
			uint num2 = 1u;
			int num3 = 0;
			while (num3 < 32)
			{
				num |= (uint)(ptr[num3] >>> 7) * num2;
				num3++;
				num2 <<= 1;
			}
			return (int)num;
		}

		[DebuggerStepThrough]
		public unsafe static int mm256_extract_epi8(v256 a, int index)
		{
			return (&a.Byte0)[index & 0x1F];
		}

		[DebuggerStepThrough]
		public unsafe static int mm256_extract_epi16(v256 a, int index)
		{
			return (&a.UShort0)[index & 0xF];
		}

		[DebuggerStepThrough]
		public static double mm256_cvtsd_f64(v256 a)
		{
			return a.Double0;
		}

		[DebuggerStepThrough]
		public static int mm256_cvtsi256_si32(v256 a)
		{
			return a.SInt0;
		}

		[DebuggerStepThrough]
		public static long mm256_cvtsi256_si64(v256 a)
		{
			return a.SLong0;
		}

		[DebuggerStepThrough]
		public static v256 mm256_cmpeq_epi8(v256 a, v256 b)
		{
			return new v256(Sse2.cmpeq_epi8(a.Lo128, b.Lo128), Sse2.cmpeq_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_cmpeq_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.cmpeq_epi16(a.Lo128, b.Lo128), Sse2.cmpeq_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_cmpeq_epi32(v256 a, v256 b)
		{
			return new v256(Sse2.cmpeq_epi32(a.Lo128, b.Lo128), Sse2.cmpeq_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_cmpeq_epi64(v256 a, v256 b)
		{
			return new v256(Sse4_1.cmpeq_epi64(a.Lo128, b.Lo128), Sse4_1.cmpeq_epi64(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_cmpgt_epi8(v256 a, v256 b)
		{
			return new v256(Sse2.cmpgt_epi8(a.Lo128, b.Lo128), Sse2.cmpgt_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_cmpgt_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.cmpgt_epi16(a.Lo128, b.Lo128), Sse2.cmpgt_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_cmpgt_epi32(v256 a, v256 b)
		{
			return new v256(Sse2.cmpgt_epi32(a.Lo128, b.Lo128), Sse2.cmpgt_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_cmpgt_epi64(v256 a, v256 b)
		{
			return new v256(Sse4_2.cmpgt_epi64(a.Lo128, b.Lo128), Sse4_2.cmpgt_epi64(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_max_epi8(v256 a, v256 b)
		{
			return new v256(Sse4_1.max_epi8(a.Lo128, b.Lo128), Sse4_1.max_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_max_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.max_epi16(a.Lo128, b.Lo128), Sse2.max_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_max_epi32(v256 a, v256 b)
		{
			return new v256(Sse4_1.max_epi32(a.Lo128, b.Lo128), Sse4_1.max_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_max_epu8(v256 a, v256 b)
		{
			return new v256(Sse2.max_epu8(a.Lo128, b.Lo128), Sse2.max_epu8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_max_epu16(v256 a, v256 b)
		{
			return new v256(Sse4_1.max_epu16(a.Lo128, b.Lo128), Sse4_1.max_epu16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_max_epu32(v256 a, v256 b)
		{
			return new v256(Sse4_1.max_epu32(a.Lo128, b.Lo128), Sse4_1.max_epu32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_min_epi8(v256 a, v256 b)
		{
			return new v256(Sse4_1.min_epi8(a.Lo128, b.Lo128), Sse4_1.min_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_min_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.min_epi16(a.Lo128, b.Lo128), Sse2.min_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_min_epi32(v256 a, v256 b)
		{
			return new v256(Sse4_1.min_epi32(a.Lo128, b.Lo128), Sse4_1.min_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_min_epu8(v256 a, v256 b)
		{
			return new v256(Sse2.min_epu8(a.Lo128, b.Lo128), Sse2.min_epu8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_min_epu16(v256 a, v256 b)
		{
			return new v256(Sse4_1.min_epu16(a.Lo128, b.Lo128), Sse4_1.min_epu16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_min_epu32(v256 a, v256 b)
		{
			return new v256(Sse4_1.min_epu32(a.Lo128, b.Lo128), Sse4_1.min_epu32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_and_si256(v256 a, v256 b)
		{
			return new v256(Sse2.and_si128(a.Lo128, b.Lo128), Sse2.and_si128(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_andnot_si256(v256 a, v256 b)
		{
			return new v256(Sse2.andnot_si128(a.Lo128, b.Lo128), Sse2.andnot_si128(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_or_si256(v256 a, v256 b)
		{
			return new v256(Sse2.or_si128(a.Lo128, b.Lo128), Sse2.or_si128(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_xor_si256(v256 a, v256 b)
		{
			return new v256(Sse2.xor_si128(a.Lo128, b.Lo128), Sse2.xor_si128(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_abs_epi8(v256 a)
		{
			return new v256(Ssse3.abs_epi8(a.Lo128), Ssse3.abs_epi8(a.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_abs_epi16(v256 a)
		{
			return new v256(Ssse3.abs_epi16(a.Lo128), Ssse3.abs_epi16(a.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_abs_epi32(v256 a)
		{
			return new v256(Ssse3.abs_epi32(a.Lo128), Ssse3.abs_epi32(a.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_add_epi8(v256 a, v256 b)
		{
			return new v256(Sse2.add_epi8(a.Lo128, b.Lo128), Sse2.add_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_add_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.add_epi16(a.Lo128, b.Lo128), Sse2.add_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_add_epi32(v256 a, v256 b)
		{
			return new v256(Sse2.add_epi32(a.Lo128, b.Lo128), Sse2.add_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_add_epi64(v256 a, v256 b)
		{
			return new v256(Sse2.add_epi64(a.Lo128, b.Lo128), Sse2.add_epi64(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_adds_epi8(v256 a, v256 b)
		{
			return new v256(Sse2.adds_epi8(a.Lo128, b.Lo128), Sse2.adds_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_adds_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.adds_epi16(a.Lo128, b.Lo128), Sse2.adds_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_adds_epu8(v256 a, v256 b)
		{
			return new v256(Sse2.adds_epu8(a.Lo128, b.Lo128), Sse2.adds_epu8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_adds_epu16(v256 a, v256 b)
		{
			return new v256(Sse2.adds_epu16(a.Lo128, b.Lo128), Sse2.adds_epu16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sub_epi8(v256 a, v256 b)
		{
			return new v256(Sse2.sub_epi8(a.Lo128, b.Lo128), Sse2.sub_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sub_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.sub_epi16(a.Lo128, b.Lo128), Sse2.sub_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sub_epi32(v256 a, v256 b)
		{
			return new v256(Sse2.sub_epi32(a.Lo128, b.Lo128), Sse2.sub_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sub_epi64(v256 a, v256 b)
		{
			return new v256(Sse2.sub_epi64(a.Lo128, b.Lo128), Sse2.sub_epi64(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_subs_epi8(v256 a, v256 b)
		{
			return new v256(Sse2.subs_epi8(a.Lo128, b.Lo128), Sse2.subs_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_subs_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.subs_epi16(a.Lo128, b.Lo128), Sse2.subs_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_subs_epu8(v256 a, v256 b)
		{
			return new v256(Sse2.subs_epu8(a.Lo128, b.Lo128), Sse2.subs_epu8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_subs_epu16(v256 a, v256 b)
		{
			return new v256(Sse2.subs_epu16(a.Lo128, b.Lo128), Sse2.subs_epu16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_avg_epu8(v256 a, v256 b)
		{
			return new v256(Sse2.avg_epu8(a.Lo128, b.Lo128), Sse2.avg_epu8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_avg_epu16(v256 a, v256 b)
		{
			return new v256(Sse2.avg_epu16(a.Lo128, b.Lo128), Sse2.avg_epu16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_hadd_epi16(v256 a, v256 b)
		{
			return new v256(Ssse3.hadd_epi16(a.Lo128, b.Lo128), Ssse3.hadd_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_hadd_epi32(v256 a, v256 b)
		{
			return new v256(Ssse3.hadd_epi32(a.Lo128, b.Lo128), Ssse3.hadd_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_hadds_epi16(v256 a, v256 b)
		{
			return new v256(Ssse3.hadds_epi16(a.Lo128, b.Lo128), Ssse3.hadds_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_hsub_epi16(v256 a, v256 b)
		{
			return new v256(Ssse3.hsub_epi16(a.Lo128, b.Lo128), Ssse3.hsub_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_hsub_epi32(v256 a, v256 b)
		{
			return new v256(Ssse3.hsub_epi32(a.Lo128, b.Lo128), Ssse3.hsub_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_hsubs_epi16(v256 a, v256 b)
		{
			return new v256(Ssse3.hsubs_epi16(a.Lo128, b.Lo128), Ssse3.hsubs_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_madd_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.madd_epi16(a.Lo128, b.Lo128), Sse2.madd_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_maddubs_epi16(v256 a, v256 b)
		{
			return new v256(Ssse3.maddubs_epi16(a.Lo128, b.Lo128), Ssse3.maddubs_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_mulhi_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.mulhi_epi16(a.Lo128, b.Lo128), Sse2.mulhi_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_mulhi_epu16(v256 a, v256 b)
		{
			return new v256(Sse2.mulhi_epu16(a.Lo128, b.Lo128), Sse2.mulhi_epu16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_mullo_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.mullo_epi16(a.Lo128, b.Lo128), Sse2.mullo_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_mullo_epi32(v256 a, v256 b)
		{
			return new v256(Sse4_1.mullo_epi32(a.Lo128, b.Lo128), Sse4_1.mullo_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_mul_epu32(v256 a, v256 b)
		{
			return new v256(Sse2.mul_epu32(a.Lo128, b.Lo128), Sse2.mul_epu32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_mul_epi32(v256 a, v256 b)
		{
			return new v256(Sse4_1.mul_epi32(a.Lo128, b.Lo128), Sse4_1.mul_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sign_epi8(v256 a, v256 b)
		{
			return new v256(Ssse3.sign_epi8(a.Lo128, b.Lo128), Ssse3.sign_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sign_epi16(v256 a, v256 b)
		{
			return new v256(Ssse3.sign_epi16(a.Lo128, b.Lo128), Ssse3.sign_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sign_epi32(v256 a, v256 b)
		{
			return new v256(Ssse3.sign_epi32(a.Lo128, b.Lo128), Ssse3.sign_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_mulhrs_epi16(v256 a, v256 b)
		{
			return new v256(Ssse3.mulhrs_epi16(a.Lo128, b.Lo128), Ssse3.mulhrs_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sad_epu8(v256 a, v256 b)
		{
			return new v256(Sse2.sad_epu8(a.Lo128, b.Lo128), Sse2.sad_epu8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_mpsadbw_epu8(v256 a, v256 b, int imm8)
		{
			return new v256(Sse4_1.mpsadbw_epu8(a.Lo128, b.Lo128, imm8 & 7), Sse4_1.mpsadbw_epu8(a.Hi128, b.Hi128, (imm8 >> 3) & 7));
		}

		[DebuggerStepThrough]
		public static v256 mm256_slli_si256(v256 a, int imm8)
		{
			return new v256(Sse2.slli_si128(a.Lo128, imm8), Sse2.slli_si128(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_bslli_epi128(v256 a, int imm8)
		{
			return mm256_slli_si256(a, imm8);
		}

		[DebuggerStepThrough]
		public static v256 mm256_srli_si256(v256 a, int imm8)
		{
			return new v256(Sse2.srli_si128(a.Lo128, imm8), Sse2.srli_si128(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_bsrli_epi128(v256 a, int imm8)
		{
			return mm256_srli_si256(a, imm8);
		}

		[DebuggerStepThrough]
		public static v256 mm256_sll_epi16(v256 a, v128 count)
		{
			return new v256(Sse2.sll_epi16(a.Lo128, count), Sse2.sll_epi16(a.Hi128, count));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sll_epi32(v256 a, v128 count)
		{
			return new v256(Sse2.sll_epi32(a.Lo128, count), Sse2.sll_epi32(a.Hi128, count));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sll_epi64(v256 a, v128 count)
		{
			return new v256(Sse2.sll_epi64(a.Lo128, count), Sse2.sll_epi64(a.Hi128, count));
		}

		[DebuggerStepThrough]
		public static v256 mm256_slli_epi16(v256 a, int imm8)
		{
			return new v256(Sse2.slli_epi16(a.Lo128, imm8), Sse2.slli_epi16(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_slli_epi32(v256 a, int imm8)
		{
			return new v256(Sse2.slli_epi32(a.Lo128, imm8), Sse2.slli_epi32(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_slli_epi64(v256 a, int imm8)
		{
			return new v256(Sse2.slli_epi64(a.Lo128, imm8), Sse2.slli_epi64(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sllv_epi32(v256 a, v256 count)
		{
			return new v256(sllv_epi32(a.Lo128, count.Lo128), sllv_epi32(a.Hi128, count.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sllv_epi64(v256 a, v256 count)
		{
			return new v256(sllv_epi64(a.Lo128, count.Lo128), sllv_epi64(a.Hi128, count.Hi128));
		}

		[DebuggerStepThrough]
		public unsafe static v128 sllv_epi32(v128 a, v128 count)
		{
			v128 result = default(v128);
			uint* ptr = &a.UInt0;
			uint* ptr2 = &result.UInt0;
			int* ptr3 = &count.SInt0;
			for (int i = 0; i < 4; i++)
			{
				int num = ptr3[i];
				if (num >= 0 && num <= 31)
				{
					ptr2[i] = ptr[i] << num;
				}
				else
				{
					ptr2[i] = 0u;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sllv_epi64(v128 a, v128 count)
		{
			v128 result = default(v128);
			ulong* ptr = &a.ULong0;
			ulong* ptr2 = &result.ULong0;
			long* ptr3 = &count.SLong0;
			for (int i = 0; i < 2; i++)
			{
				int num = (int)ptr3[i];
				if (num >= 0 && num <= 63)
				{
					ptr2[i] = ptr[i] << num;
				}
				else
				{
					ptr2[i] = 0uL;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v256 mm256_sra_epi16(v256 a, v128 count)
		{
			return new v256(Sse2.sra_epi16(a.Lo128, count), Sse2.sra_epi16(a.Hi128, count));
		}

		[DebuggerStepThrough]
		public static v256 mm256_sra_epi32(v256 a, v128 count)
		{
			return new v256(Sse2.sra_epi32(a.Lo128, count), Sse2.sra_epi32(a.Hi128, count));
		}

		[DebuggerStepThrough]
		public static v256 mm256_srai_epi16(v256 a, int imm8)
		{
			return new v256(Sse2.srai_epi16(a.Lo128, imm8), Sse2.srai_epi16(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_srai_epi32(v256 a, int imm8)
		{
			return new v256(Sse2.srai_epi32(a.Lo128, imm8), Sse2.srai_epi32(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_srav_epi32(v256 a, v256 count)
		{
			return new v256(srav_epi32(a.Lo128, count.Lo128), srav_epi32(a.Hi128, count.Hi128));
		}

		[DebuggerStepThrough]
		public unsafe static v128 srav_epi32(v128 a, v128 count)
		{
			v128 result = default(v128);
			int* ptr = &a.SInt0;
			int* ptr2 = &result.SInt0;
			int* ptr3 = &count.SInt0;
			for (int i = 0; i < 4; i++)
			{
				int num = Math.Min(ptr3[i] & 0xFF, 32);
				int num2 = 0;
				if (num >= 16)
				{
					num -= 16;
					num2 += 16;
				}
				ptr2[i] = ptr[i] >> num >> num2;
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v256 mm256_srl_epi16(v256 a, v128 count)
		{
			return new v256(Sse2.srl_epi16(a.Lo128, count), Sse2.srl_epi16(a.Hi128, count));
		}

		[DebuggerStepThrough]
		public static v256 mm256_srl_epi32(v256 a, v128 count)
		{
			return new v256(Sse2.srl_epi32(a.Lo128, count), Sse2.srl_epi32(a.Hi128, count));
		}

		[DebuggerStepThrough]
		public static v256 mm256_srl_epi64(v256 a, v128 count)
		{
			return new v256(Sse2.srl_epi64(a.Lo128, count), Sse2.srl_epi64(a.Hi128, count));
		}

		[DebuggerStepThrough]
		public static v256 mm256_srli_epi16(v256 a, int imm8)
		{
			return new v256(Sse2.srli_epi16(a.Lo128, imm8), Sse2.srli_epi16(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_srli_epi32(v256 a, int imm8)
		{
			return new v256(Sse2.srli_epi32(a.Lo128, imm8), Sse2.srli_epi32(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_srli_epi64(v256 a, int imm8)
		{
			return new v256(Sse2.srli_epi64(a.Lo128, imm8), Sse2.srli_epi64(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_srlv_epi32(v256 a, v256 count)
		{
			return new v256(srlv_epi32(a.Lo128, count.Lo128), srlv_epi32(a.Hi128, count.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_srlv_epi64(v256 a, v256 count)
		{
			return new v256(srlv_epi64(a.Lo128, count.Lo128), srlv_epi64(a.Hi128, count.Hi128));
		}

		[DebuggerStepThrough]
		public unsafe static v128 srlv_epi32(v128 a, v128 count)
		{
			v128 result = default(v128);
			uint* ptr = &a.UInt0;
			uint* ptr2 = &result.UInt0;
			int* ptr3 = &count.SInt0;
			for (int i = 0; i < 4; i++)
			{
				int num = ptr3[i];
				if (num >= 0 && num <= 31)
				{
					ptr2[i] = ptr[i] >> num;
				}
				else
				{
					ptr2[i] = 0u;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 srlv_epi64(v128 a, v128 count)
		{
			v128 result = default(v128);
			ulong* ptr = &a.ULong0;
			ulong* ptr2 = &result.ULong0;
			long* ptr3 = &count.SLong0;
			for (int i = 0; i < 2; i++)
			{
				int num = (int)ptr3[i];
				if (num >= 0 && num <= 63)
				{
					ptr2[i] = ptr[i] >> num;
				}
				else
				{
					ptr2[i] = 0uL;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 blend_epi32(v128 a, v128 b, int imm8)
		{
			return Sse4_1.blend_ps(a, b, imm8);
		}

		[DebuggerStepThrough]
		public static v256 mm256_blend_epi32(v256 a, v256 b, int imm8)
		{
			return Avx.mm256_blend_ps(a, b, imm8);
		}

		[DebuggerStepThrough]
		public static v256 mm256_alignr_epi8(v256 a, v256 b, int imm8)
		{
			return new v256(Ssse3.alignr_epi8(a.Lo128, b.Lo128, imm8), Ssse3.alignr_epi8(a.Hi128, b.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_blendv_epi8(v256 a, v256 b, v256 mask)
		{
			return new v256(Sse4_1.blendv_epi8(a.Lo128, b.Lo128, mask.Lo128), Sse4_1.blendv_epi8(a.Hi128, b.Hi128, mask.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_blend_epi16(v256 a, v256 b, int imm8)
		{
			return new v256(Sse4_1.blend_epi16(a.Lo128, b.Lo128, imm8), Sse4_1.blend_epi16(a.Hi128, b.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_packs_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.packs_epi16(a.Lo128, b.Lo128), Sse2.packs_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_packs_epi32(v256 a, v256 b)
		{
			return new v256(Sse2.packs_epi32(a.Lo128, b.Lo128), Sse2.packs_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_packus_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.packus_epi16(a.Lo128, b.Lo128), Sse2.packus_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_packus_epi32(v256 a, v256 b)
		{
			return new v256(Sse4_1.packus_epi32(a.Lo128, b.Lo128), Sse4_1.packus_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_unpackhi_epi8(v256 a, v256 b)
		{
			return new v256(Sse2.unpackhi_epi8(a.Lo128, b.Lo128), Sse2.unpackhi_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_unpackhi_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.unpackhi_epi16(a.Lo128, b.Lo128), Sse2.unpackhi_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_unpackhi_epi32(v256 a, v256 b)
		{
			return new v256(Sse2.unpackhi_epi32(a.Lo128, b.Lo128), Sse2.unpackhi_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_unpackhi_epi64(v256 a, v256 b)
		{
			return new v256(Sse2.unpackhi_epi64(a.Lo128, b.Lo128), Sse2.unpackhi_epi64(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_unpacklo_epi8(v256 a, v256 b)
		{
			return new v256(Sse2.unpacklo_epi8(a.Lo128, b.Lo128), Sse2.unpacklo_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_unpacklo_epi16(v256 a, v256 b)
		{
			return new v256(Sse2.unpacklo_epi16(a.Lo128, b.Lo128), Sse2.unpacklo_epi16(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_unpacklo_epi32(v256 a, v256 b)
		{
			return new v256(Sse2.unpacklo_epi32(a.Lo128, b.Lo128), Sse2.unpacklo_epi32(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_unpacklo_epi64(v256 a, v256 b)
		{
			return new v256(Sse2.unpacklo_epi64(a.Lo128, b.Lo128), Sse2.unpacklo_epi64(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_shuffle_epi8(v256 a, v256 b)
		{
			return new v256(Ssse3.shuffle_epi8(a.Lo128, b.Lo128), Ssse3.shuffle_epi8(a.Hi128, b.Hi128));
		}

		[DebuggerStepThrough]
		public static v256 mm256_shuffle_epi32(v256 a, int imm8)
		{
			return new v256(Sse2.shuffle_epi32(a.Lo128, imm8), Sse2.shuffle_epi32(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_shufflehi_epi16(v256 a, int imm8)
		{
			return new v256(Sse2.shufflehi_epi16(a.Lo128, imm8), Sse2.shufflehi_epi16(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v256 mm256_shufflelo_epi16(v256 a, int imm8)
		{
			return new v256(Sse2.shufflelo_epi16(a.Lo128, imm8), Sse2.shufflelo_epi16(a.Hi128, imm8));
		}

		[DebuggerStepThrough]
		public static v128 mm256_extracti128_si256(v256 a, int imm8)
		{
			return Avx.mm256_extractf128_si256(a, imm8);
		}

		[DebuggerStepThrough]
		public static v256 mm256_inserti128_si256(v256 a, v128 b, int imm8)
		{
			return Avx.mm256_insertf128_ps(a, b, imm8);
		}

		[DebuggerStepThrough]
		public static v128 broadcastss_ps(v128 a)
		{
			return new v128(a.Float0);
		}

		[DebuggerStepThrough]
		public static v256 mm256_broadcastss_ps(v128 a)
		{
			return new v256(a.Float0);
		}

		[DebuggerStepThrough]
		public static v128 broadcastsd_pd(v128 a)
		{
			return new v128(a.Double0);
		}

		[DebuggerStepThrough]
		public static v256 mm256_broadcastsd_pd(v128 a)
		{
			return new v256(a.Double0);
		}

		[DebuggerStepThrough]
		public static v128 broadcastb_epi8(v128 a)
		{
			return new v128(a.Byte0);
		}

		[DebuggerStepThrough]
		public static v128 broadcastw_epi16(v128 a)
		{
			return new v128(a.SShort0);
		}

		[DebuggerStepThrough]
		public static v128 broadcastd_epi32(v128 a)
		{
			return new v128(a.SInt0);
		}

		[DebuggerStepThrough]
		public static v128 broadcastq_epi64(v128 a)
		{
			return new v128(a.SLong0);
		}

		[DebuggerStepThrough]
		public static v256 mm256_broadcastb_epi8(v128 a)
		{
			return new v256(a.Byte0);
		}

		[DebuggerStepThrough]
		public static v256 mm256_broadcastw_epi16(v128 a)
		{
			return new v256(a.SShort0);
		}

		[DebuggerStepThrough]
		public static v256 mm256_broadcastd_epi32(v128 a)
		{
			return new v256(a.SInt0);
		}

		[DebuggerStepThrough]
		public static v256 mm256_broadcastq_epi64(v128 a)
		{
			return new v256(a.SLong0);
		}

		[DebuggerStepThrough]
		public static v256 mm256_broadcastsi128_si256(v128 a)
		{
			return new v256(a, a);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepi8_epi16(v128 a)
		{
			v256 result = default(v256);
			short* ptr = &result.SShort0;
			sbyte* ptr2 = &a.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepi8_epi32(v128 a)
		{
			v256 result = default(v256);
			int* ptr = &result.SInt0;
			sbyte* ptr2 = &a.SByte0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepi8_epi64(v128 a)
		{
			v256 result = default(v256);
			long* ptr = &result.SLong0;
			sbyte* ptr2 = &a.SByte0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepi16_epi32(v128 a)
		{
			v256 result = default(v256);
			int* ptr = &result.SInt0;
			short* ptr2 = &a.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepi16_epi64(v128 a)
		{
			v256 result = default(v256);
			long* ptr = &result.SLong0;
			short* ptr2 = &a.SShort0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepi32_epi64(v128 a)
		{
			v256 result = default(v256);
			long* ptr = &result.SLong0;
			int* ptr2 = &a.SInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepu8_epi16(v128 a)
		{
			v256 result = default(v256);
			short* ptr = &result.SShort0;
			byte* ptr2 = &a.Byte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepu8_epi32(v128 a)
		{
			v256 result = default(v256);
			int* ptr = &result.SInt0;
			byte* ptr2 = &a.Byte0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepu8_epi64(v128 a)
		{
			v256 result = default(v256);
			long* ptr = &result.SLong0;
			byte* ptr2 = &a.Byte0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepu16_epi32(v128 a)
		{
			v256 result = default(v256);
			int* ptr = &result.SInt0;
			ushort* ptr2 = &a.UShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepu16_epi64(v128 a)
		{
			v256 result = default(v256);
			long* ptr = &result.SLong0;
			ushort* ptr2 = &a.UShort0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_cvtepu32_epi64(v128 a)
		{
			v256 result = default(v256);
			long* ptr = &result.SLong0;
			uint* ptr2 = &a.UInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 maskload_epi32(void* mem_addr, v128 mask)
		{
			v128 result = default(v128);
			int* ptr = &mask.SInt0;
			int* ptr2 = &result.SInt0;
			for (int i = 0; i < 4; i++)
			{
				if (ptr[i] < 0)
				{
					ptr2[i] = *(int*)((byte*)mem_addr + (nint)i * (nint)4);
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 maskload_epi64(void* mem_addr, v128 mask)
		{
			v128 result = default(v128);
			long* ptr = &mask.SLong0;
			long* ptr2 = &result.SLong0;
			for (int i = 0; i < 2; i++)
			{
				if (ptr[i] < 0)
				{
					ptr2[i] = *(long*)((byte*)mem_addr + (nint)i * (nint)8);
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static void maskstore_epi32(void* mem_addr, v128 mask, v128 a)
		{
			int* ptr = &mask.SInt0;
			int* ptr2 = &a.SInt0;
			for (int i = 0; i < 4; i++)
			{
				if (ptr[i] < 0)
				{
					*(int*)((byte*)mem_addr + (nint)i * (nint)4) = ptr2[i];
				}
			}
		}

		[DebuggerStepThrough]
		public unsafe static void maskstore_epi64(void* mem_addr, v128 mask, v128 a)
		{
			long* ptr = &mask.SLong0;
			long* ptr2 = &a.SLong0;
			for (int i = 0; i < 2; i++)
			{
				if (ptr[i] < 0)
				{
					*(long*)((byte*)mem_addr + (nint)i * (nint)8) = ptr2[i];
				}
			}
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_maskload_epi32(void* mem_addr, v256 mask)
		{
			v256 result = default(v256);
			int* ptr = &mask.SInt0;
			int* ptr2 = &result.SInt0;
			for (int i = 0; i < 8; i++)
			{
				if (ptr[i] < 0)
				{
					ptr2[i] = *(int*)((byte*)mem_addr + (nint)i * (nint)4);
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_maskload_epi64(void* mem_addr, v256 mask)
		{
			v256 result = default(v256);
			long* ptr = &mask.SLong0;
			long* ptr2 = &result.SLong0;
			for (int i = 0; i < 4; i++)
			{
				if (ptr[i] < 0)
				{
					ptr2[i] = *(long*)((byte*)mem_addr + (nint)i * (nint)8);
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_maskstore_epi32(void* mem_addr, v256 mask, v256 a)
		{
			int* ptr = &mask.SInt0;
			int* ptr2 = &a.SInt0;
			for (int i = 0; i < 8; i++)
			{
				if (ptr[i] < 0)
				{
					*(int*)((byte*)mem_addr + (nint)i * (nint)4) = ptr2[i];
				}
			}
		}

		[DebuggerStepThrough]
		public unsafe static void mm256_maskstore_epi64(void* mem_addr, v256 mask, v256 a)
		{
			long* ptr = &mask.SLong0;
			long* ptr2 = &a.SLong0;
			for (int i = 0; i < 4; i++)
			{
				if (ptr[i] < 0)
				{
					*(long*)((byte*)mem_addr + (nint)i * (nint)8) = ptr2[i];
				}
			}
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_permutevar8x32_epi32(v256 a, v256 idx)
		{
			v256 result = default(v256);
			int* ptr = &idx.SInt0;
			int* ptr2 = &a.SInt0;
			int* ptr3 = &result.SInt0;
			for (int i = 0; i < 8; i++)
			{
				int num = ptr[i] & 7;
				ptr3[i] = ptr2[num];
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v256 mm256_permutevar8x32_ps(v256 a, v256 idx)
		{
			return mm256_permutevar8x32_epi32(a, idx);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_permute4x64_epi64(v256 a, int imm8)
		{
			v256 result = default(v256);
			long* ptr = &a.SLong0;
			long* ptr2 = &result.SLong0;
			int num = 0;
			while (num < 4)
			{
				ptr2[num] = ptr[imm8 & 3];
				num++;
				imm8 >>= 2;
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v256 mm256_permute4x64_pd(v256 a, int imm8)
		{
			return mm256_permute4x64_epi64(a, imm8);
		}

		[DebuggerStepThrough]
		public static v256 mm256_permute2x128_si256(v256 a, v256 b, int imm8)
		{
			return Avx.mm256_permute2f128_si256(a, b, imm8);
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_stream_load_si256(void* mem_addr)
		{
			return *(v256*)mem_addr;
		}

		private unsafe static void EmulatedGather<T, U>(T* dptr, void* base_addr, long* indexPtr, int scale, int n, U* mask) where T : unmanaged where U : unmanaged, IComparable<U>
		{
			U other = default(U);
			for (int i = 0; i < n; i++)
			{
				long num = indexPtr[i] * scale;
				T* ptr = (T*)((byte*)base_addr + num);
				if (mask == null || mask[i].CompareTo(other) < 0)
				{
					dptr[i] = *ptr;
				}
			}
		}

		private unsafe static void EmulatedGather<T, U>(T* dptr, void* base_addr, int* indexPtr, int scale, int n, U* mask) where T : unmanaged where U : unmanaged, IComparable<U>
		{
			U other = default(U);
			for (int i = 0; i < n; i++)
			{
				long num = (long)indexPtr[i] * (long)scale;
				T* ptr = (T*)((byte*)base_addr + num);
				if (mask == null || mask[i].CompareTo(other) < 0)
				{
					dptr[i] = *ptr;
				}
			}
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_i32gather_epi32(void* base_addr, v256 vindex, int scale)
		{
			v256 result = default(v256);
			EmulatedGather<int, int>(&result.SInt0, base_addr, &vindex.SInt0, scale, sizeof(v256) / 4, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_i32gather_pd(void* base_addr, v128 vindex, int scale)
		{
			v256 result = default(v256);
			EmulatedGather<double, long>(&result.Double0, base_addr, &vindex.SInt0, scale, 4, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_i32gather_ps(void* base_addr, v256 vindex, int scale)
		{
			v256 result = default(v256);
			EmulatedGather<float, int>(&result.Float0, base_addr, &vindex.SInt0, scale, 8, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_i64gather_pd(void* base_addr, v256 vindex, int scale)
		{
			v256 result = default(v256);
			EmulatedGather<double, long>(&result.Double0, base_addr, &vindex.SLong0, scale, 4, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mm256_i64gather_ps(void* base_addr, v256 vindex, int scale)
		{
			v128 result = default(v128);
			EmulatedGather<float, int>(&result.Float0, base_addr, &vindex.SLong0, scale, 4, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 i32gather_pd(void* base_addr, v128 vindex, int scale)
		{
			v128 result = default(v128);
			EmulatedGather<double, long>(&result.Double0, base_addr, &vindex.SInt0, scale, 2, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 i32gather_ps(void* base_addr, v128 vindex, int scale)
		{
			v128 result = default(v128);
			EmulatedGather<float, int>(&result.Float0, base_addr, &vindex.SInt0, scale, 4, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 i64gather_pd(void* base_addr, v128 vindex, int scale)
		{
			v128 result = default(v128);
			EmulatedGather<double, long>(&result.Double0, base_addr, &vindex.SLong0, scale, 2, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 i64gather_ps(void* base_addr, v128 vindex, int scale)
		{
			v128 result = default(v128);
			EmulatedGather<float, int>(&result.Float0, base_addr, &vindex.SLong0, scale, 2, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_i32gather_epi64(void* base_addr, v128 vindex, int scale)
		{
			v256 result = default(v256);
			EmulatedGather<long, long>(&result.SLong0, base_addr, &vindex.SInt0, scale, 4, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mm256_i64gather_epi32(void* base_addr, v256 vindex, int scale)
		{
			v128 result = default(v128);
			EmulatedGather<int, int>(&result.SInt0, base_addr, &vindex.SLong0, scale, 4, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_i64gather_epi64(void* base_addr, v256 vindex, int scale)
		{
			v256 result = default(v256);
			EmulatedGather<long, long>(&result.SLong0, base_addr, &vindex.SLong0, scale, 4, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 i32gather_epi32(void* base_addr, v128 vindex, int scale)
		{
			v128 result = default(v128);
			EmulatedGather<int, int>(&result.SInt0, base_addr, &vindex.SInt0, scale, 4, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 i32gather_epi64(void* base_addr, v128 vindex, int scale)
		{
			v128 result = default(v128);
			EmulatedGather<long, long>(&result.SLong0, base_addr, &vindex.SInt0, scale, 2, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 i64gather_epi32(void* base_addr, v128 vindex, int scale)
		{
			v128 result = default(v128);
			EmulatedGather<int, int>(&result.SInt0, base_addr, &vindex.SLong0, scale, 2, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 i64gather_epi64(void* base_addr, v128 vindex, int scale)
		{
			v128 result = default(v128);
			EmulatedGather<long, long>(&result.SLong0, base_addr, &vindex.SLong0, scale, 2, null);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_mask_i32gather_pd(v256 src, void* base_addr, v128 vindex, v256 mask, int scale)
		{
			v256 result = src;
			EmulatedGather(&result.Double0, base_addr, &vindex.SInt0, scale, 4, &mask.SLong0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_mask_i32gather_ps(v256 src, void* base_addr, v256 vindex, v256 mask, int scale)
		{
			v256 result = src;
			EmulatedGather(&result.Float0, base_addr, &vindex.SInt0, scale, 8, &mask.SInt0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_mask_i64gather_pd(v256 src, void* base_addr, v256 vindex, v256 mask, int scale)
		{
			v256 result = src;
			EmulatedGather(&result.Double0, base_addr, &vindex.SLong0, scale, 4, &mask.SLong0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mm256_mask_i64gather_ps(v128 src, void* base_addr, v256 vindex, v128 mask, int scale)
		{
			v128 result = src;
			EmulatedGather(&result.Float0, base_addr, &vindex.SLong0, scale, 4, &mask.SInt0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_mask_i32gather_epi32(v256 src, void* base_addr, v256 vindex, v256 mask, int scale)
		{
			v256 result = src;
			EmulatedGather(&result.SInt0, base_addr, &vindex.SInt0, scale, 8, &mask.SInt0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_mask_i32gather_epi64(v256 src, void* base_addr, v128 vindex, v256 mask, int scale)
		{
			v256 result = src;
			EmulatedGather(&result.SLong0, base_addr, &vindex.SInt0, scale, 4, &mask.SLong0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v256 mm256_mask_i64gather_epi64(v256 src, void* base_addr, v256 vindex, v256 mask, int scale)
		{
			v256 result = src;
			EmulatedGather(&result.SLong0, base_addr, &vindex.SLong0, scale, 4, &mask.SLong0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mm256_mask_i64gather_epi32(v128 src, void* base_addr, v256 vindex, v128 mask, int scale)
		{
			v128 result = src;
			EmulatedGather(&result.SInt0, base_addr, &vindex.SLong0, scale, 4, &mask.SInt0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mask_i32gather_pd(v128 src, void* base_addr, v128 vindex, v128 mask, int scale)
		{
			v128 result = src;
			EmulatedGather(&result.Double0, base_addr, &vindex.SInt0, scale, 2, &mask.SLong0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mask_i32gather_ps(v128 src, void* base_addr, v128 vindex, v128 mask, int scale)
		{
			v128 result = src;
			EmulatedGather(&result.Float0, base_addr, &vindex.SInt0, scale, 4, &mask.SInt0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mask_i64gather_pd(v128 src, void* base_addr, v128 vindex, v128 mask, int scale)
		{
			v128 result = src;
			EmulatedGather(&result.Double0, base_addr, &vindex.SLong0, scale, 2, &mask.SLong0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mask_i64gather_ps(v128 src, void* base_addr, v128 vindex, v128 mask, int scale)
		{
			v128 result = src;
			result.UInt2 = (result.UInt3 = 0u);
			EmulatedGather(&result.Float0, base_addr, &vindex.SLong0, scale, 2, &mask.SInt0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mask_i32gather_epi32(v128 src, void* base_addr, v128 vindex, v128 mask, int scale)
		{
			v128 result = src;
			EmulatedGather(&result.SInt0, base_addr, &vindex.SInt0, scale, 4, &mask.SInt0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mask_i32gather_epi64(v128 src, void* base_addr, v128 vindex, v128 mask, int scale)
		{
			v128 result = src;
			EmulatedGather(&result.SLong0, base_addr, &vindex.SInt0, scale, 2, &mask.SLong0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mask_i64gather_epi32(v128 src, void* base_addr, v128 vindex, v128 mask, int scale)
		{
			v128 result = src;
			result.UInt2 = (result.UInt3 = 0u);
			EmulatedGather(&result.SInt0, base_addr, &vindex.SLong0, scale, 2, &mask.SInt0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mask_i64gather_epi64(v128 src, void* base_addr, v128 vindex, v128 mask, int scale)
		{
			v128 result = src;
			EmulatedGather(&result.SLong0, base_addr, &vindex.SLong0, scale, 2, &mask.SLong0);
			return result;
		}
	}

	public static class Bmi1
	{
		public static bool IsBmi1Supported => Avx2.IsAvx2Supported;

		[DebuggerStepThrough]
		public static uint andn_u32(uint a, uint b)
		{
			return ~a & b;
		}

		[DebuggerStepThrough]
		public static ulong andn_u64(ulong a, ulong b)
		{
			return ~a & b;
		}

		[DebuggerStepThrough]
		public static uint bextr_u32(uint a, uint start, uint len)
		{
			start &= 0xFFu;
			if (start >= 32)
			{
				return 0u;
			}
			uint num = a >> (int)start;
			len &= 0xFFu;
			if (len >= 32)
			{
				return num;
			}
			return num & (uint)((1 << (int)len) - 1);
		}

		[DebuggerStepThrough]
		public static ulong bextr_u64(ulong a, uint start, uint len)
		{
			start &= 0xFFu;
			if (start >= 64)
			{
				return 0uL;
			}
			ulong num = a >> (int)start;
			len &= 0xFFu;
			if (len >= 64)
			{
				return num;
			}
			return num & (ulong)((1L << (int)len) - 1);
		}

		[DebuggerStepThrough]
		public static uint bextr2_u32(uint a, uint control)
		{
			uint start = control & 0xFFu;
			uint len = (control >> 8) & 0xFFu;
			return bextr_u32(a, start, len);
		}

		[DebuggerStepThrough]
		public static ulong bextr2_u64(ulong a, ulong control)
		{
			uint start = (uint)(control & 0xFF);
			uint len = (uint)((control >> 8) & 0xFF);
			return bextr_u64(a, start, len);
		}

		[DebuggerStepThrough]
		public static uint blsi_u32(uint a)
		{
			return (0 - a) & a;
		}

		[DebuggerStepThrough]
		public static ulong blsi_u64(ulong a)
		{
			return (0L - a) & a;
		}

		[DebuggerStepThrough]
		public static uint blsmsk_u32(uint a)
		{
			return (a - 1) ^ a;
		}

		[DebuggerStepThrough]
		public static ulong blsmsk_u64(ulong a)
		{
			return (a - 1) ^ a;
		}

		[DebuggerStepThrough]
		public static uint blsr_u32(uint a)
		{
			return (a - 1) & a;
		}

		[DebuggerStepThrough]
		public static ulong blsr_u64(ulong a)
		{
			return (a - 1) & a;
		}

		[DebuggerStepThrough]
		public static uint tzcnt_u32(uint a)
		{
			uint num = 32u;
			a &= 0 - a;
			if (a != 0)
			{
				num--;
			}
			if ((a & 0xFFFFu) != 0)
			{
				num -= 16;
			}
			if ((a & 0xFF00FFu) != 0)
			{
				num -= 8;
			}
			if ((a & 0xF0F0F0Fu) != 0)
			{
				num -= 4;
			}
			if ((a & 0x33333333u) != 0)
			{
				num -= 2;
			}
			if ((a & 0x55555555u) != 0)
			{
				num--;
			}
			return num;
		}

		[DebuggerStepThrough]
		public static ulong tzcnt_u64(ulong a)
		{
			ulong num = 64uL;
			a &= 0L - a;
			if (a != 0L)
			{
				num--;
			}
			if ((a & 0xFFFFFFFFu) != 0L)
			{
				num -= 32;
			}
			if ((a & 0xFFFF0000FFFFL) != 0L)
			{
				num -= 16;
			}
			if ((a & 0xFF00FF00FF00FFL) != 0L)
			{
				num -= 8;
			}
			if ((a & 0xF0F0F0F0F0F0F0FL) != 0L)
			{
				num -= 4;
			}
			if ((a & 0x3333333333333333L) != 0L)
			{
				num -= 2;
			}
			if ((a & 0x5555555555555555L) != 0L)
			{
				num--;
			}
			return num;
		}
	}

	public static class Bmi2
	{
		public static bool IsBmi2Supported => Avx2.IsAvx2Supported;

		[DebuggerStepThrough]
		public static uint bzhi_u32(uint a, uint index)
		{
			index &= 0xFFu;
			if (index >= 32)
			{
				return a;
			}
			return a & (uint)((1 << (int)index) - 1);
		}

		[DebuggerStepThrough]
		public static ulong bzhi_u64(ulong a, ulong index)
		{
			index &= 0xFF;
			if (index >= 64)
			{
				return a;
			}
			return a & (ulong)((1L << (int)index) - 1);
		}

		[DebuggerStepThrough]
		public static uint mulx_u32(uint a, uint b, out uint hi)
		{
			long num = a;
			ulong num2 = b;
			ulong num3 = (ulong)num * num2;
			hi = (uint)(num3 >> 32);
			return (uint)(num3 & 0xFFFFFFFFu);
		}

		[DebuggerStepThrough]
		public static ulong mulx_u64(ulong a, ulong b, out ulong hi)
		{
			return Common.umul128(a, b, out hi);
		}

		[DebuggerStepThrough]
		public static uint pdep_u32(uint a, uint mask)
		{
			uint num = 0u;
			int num2 = 0;
			for (int i = 0; i < 32; i++)
			{
				if ((mask & (uint)(1 << i)) != 0)
				{
					num |= ((a >> num2) & 1) << i;
					num2++;
				}
			}
			return num;
		}

		[DebuggerStepThrough]
		public static ulong pdep_u64(ulong a, ulong mask)
		{
			ulong num = 0uL;
			int num2 = 0;
			for (int i = 0; i < 64; i++)
			{
				if ((mask & (ulong)(1L << i)) != 0L)
				{
					num |= ((a >> num2) & 1) << i;
					num2++;
				}
			}
			return num;
		}

		[DebuggerStepThrough]
		public static uint pext_u32(uint a, uint mask)
		{
			uint num = 0u;
			int num2 = 0;
			for (int i = 0; i < 32; i++)
			{
				if ((mask & (uint)(1 << i)) != 0)
				{
					num |= ((a >> i) & 1) << num2;
					num2++;
				}
			}
			return num;
		}

		[DebuggerStepThrough]
		public static ulong pext_u64(ulong a, ulong mask)
		{
			ulong num = 0uL;
			int num2 = 0;
			for (int i = 0; i < 64; i++)
			{
				if ((mask & (ulong)(1L << i)) != 0L)
				{
					num |= ((a >> i) & 1) << num2;
					num2++;
				}
			}
			return num;
		}
	}

	[Flags]
	public enum MXCSRBits
	{
		FlushToZero = 0x8000,
		RoundingControlMask = 0x6000,
		RoundToNearest = 0,
		RoundDown = 0x2000,
		RoundUp = 0x4000,
		RoundTowardZero = 0x6000,
		PrecisionMask = 0x1000,
		UnderflowMask = 0x800,
		OverflowMask = 0x400,
		DivideByZeroMask = 0x200,
		DenormalOperationMask = 0x100,
		InvalidOperationMask = 0x80,
		ExceptionMask = 0x1F80,
		DenormalsAreZeroes = 0x40,
		PrecisionFlag = 0x20,
		UnderflowFlag = 0x10,
		OverflowFlag = 8,
		DivideByZeroFlag = 4,
		DenormalFlag = 2,
		InvalidOperationFlag = 1,
		FlagMask = 0x3F
	}

	[Flags]
	public enum RoundingMode
	{
		FROUND_TO_NEAREST_INT = 0,
		FROUND_TO_NEG_INF = 1,
		FROUND_TO_POS_INF = 2,
		FROUND_TO_ZERO = 3,
		FROUND_CUR_DIRECTION = 4,
		FROUND_RAISE_EXC = 0,
		FROUND_NO_EXC = 8,
		FROUND_NINT = 0,
		FROUND_FLOOR = 1,
		FROUND_CEIL = 2,
		FROUND_TRUNC = 3,
		FROUND_RINT = 4,
		FROUND_NEARBYINT = 0xC,
		FROUND_NINT_NOEXC = 8,
		FROUND_FLOOR_NOEXC = 9,
		FROUND_CEIL_NOEXC = 0xA,
		FROUND_TRUNC_NOEXC = 0xB,
		FROUND_RINT_NOEXC = 0xC
	}

	internal struct RoundingScope : IDisposable
	{
		private MXCSRBits OldBits;

		public RoundingScope(MXCSRBits roundingMode)
		{
			OldBits = MXCSR;
			MXCSR = (OldBits & ~MXCSRBits.RoundingControlMask) | roundingMode;
		}

		public void Dispose()
		{
			MXCSR = OldBits;
		}
	}

	public static class F16C
	{
		private static readonly ushort[] BaseTable = new ushort[512]
		{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 1, 2, 4, 8, 16, 32, 64,
			128, 256, 512, 1024, 2048, 3072, 4096, 5120, 6144, 7168,
			8192, 9216, 10240, 11264, 12288, 13312, 14336, 15360, 16384, 17408,
			18432, 19456, 20480, 21504, 22528, 23552, 24576, 25600, 26624, 27648,
			28672, 29696, 30720, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744, 31744,
			31744, 31744, 31744, 31744, 31744, 31744, 32768, 32768, 32768, 32768,
			32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768,
			32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768,
			32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768,
			32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768,
			32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768,
			32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768,
			32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768,
			32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768,
			32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768,
			32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32768, 32769,
			32770, 32772, 32776, 32784, 32800, 32832, 32896, 33024, 33280, 33792,
			34816, 35840, 36864, 37888, 38912, 39936, 40960, 41984, 43008, 44032,
			45056, 46080, 47104, 48128, 49152, 50176, 51200, 52224, 53248, 54272,
			55296, 56320, 57344, 58368, 59392, 60416, 61440, 62464, 63488, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512, 64512,
			64512, 64512
		};

		private static readonly sbyte[] ShiftTable = new sbyte[512]
		{
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 23, 22, 21, 20, 19, 18, 17,
			16, 15, 14, 13, 13, 13, 13, 13, 13, 13,
			13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
			13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
			13, 13, 13, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 13, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 23,
			22, 21, 20, 19, 18, 17, 16, 15, 14, 13,
			13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
			13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
			13, 13, 13, 13, 13, 13, 13, 13, 13, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
			24, 13
		};

		public static bool IsF16CSupported => Avx2.IsAvx2Supported;

		[DebuggerStepThrough]
		private static uint HalfToFloat(ushort h)
		{
			bool num = (h & 0x8000) != 0;
			long num2 = (long)(h >> 10) & 0x1FL;
			uint num3 = h & 0x3FFu;
			uint num4 = (num ? 2147483648u : 0u);
			if (num2 != 0L || num3 != 0)
			{
				if (num2 == 0L)
				{
					num2 = -1L;
					do
					{
						num2++;
						num3 <<= 1;
					}
					while ((num3 & 0x400) == 0);
					num4 |= (uint)(int)(112 - num2 << 23);
					num4 |= (num3 & 0x3FF) << 13;
				}
				else
				{
					bool flag = num2 == 31;
					num4 |= (uint)(int)(flag ? 255 : (112 + num2 << 23));
					num4 |= num3 << 13;
				}
			}
			return num4;
		}

		[DebuggerStepThrough]
		public static v128 cvtph_ps(v128 a)
		{
			return new v128(HalfToFloat(a.UShort0), HalfToFloat(a.UShort1), HalfToFloat(a.UShort2), HalfToFloat(a.UShort3));
		}

		[DebuggerStepThrough]
		public static v256 mm256_cvtph_ps(v128 a)
		{
			return new v256(HalfToFloat(a.UShort0), HalfToFloat(a.UShort1), HalfToFloat(a.UShort2), HalfToFloat(a.UShort3), HalfToFloat(a.UShort4), HalfToFloat(a.UShort5), HalfToFloat(a.UShort6), HalfToFloat(a.UShort7));
		}

		[DebuggerStepThrough]
		private static ushort FloatToHalf(uint f, int rounding)
		{
			uint num = f >> 23;
			sbyte b = ShiftTable[num];
			uint num2 = (uint)(BaseTable[num] + (ushort)((f & 0x7FFFFF) >> (int)b));
			bool flag = (num2 & 0x7C00) != 31744;
			bool flag2 = (num2 & 0x8000) != 0;
			switch (rounding)
			{
			case 8:
			{
				uint num4 = (f & 0x7FFFFF) >> b - 1;
				if ((num & 0xFF) == 102)
				{
					num2++;
				}
				if (flag && (num4 & (true ? 1u : 0u)) != 0)
				{
					num2++;
				}
				break;
			}
			case 11:
				if (!flag)
				{
					num2 -= (uint)(~b & 1);
				}
				break;
			case 10:
			{
				if (flag && !flag2)
				{
					if (num <= 102 && num != 0)
					{
						num2++;
					}
					else if ((f & 0x7FFFFFu & (uint)((1 << (int)b) - 1)) != 0)
					{
						num2++;
					}
				}
				bool num5 = num2 == 64512;
				bool flag4 = num != 511;
				if (num5 && flag4)
				{
					num2--;
				}
				break;
			}
			case 9:
			{
				if (flag && flag2)
				{
					if (num <= 358 && num != 256)
					{
						num2++;
					}
					else if ((f & 0x7FFFFFu & (uint)((1 << (int)b) - 1)) != 0)
					{
						num2++;
					}
				}
				bool num3 = num2 == 31744;
				bool flag3 = num != 255;
				if (num3 && flag3)
				{
					num2--;
				}
				break;
			}
			}
			return (ushort)num2;
		}

		[DebuggerStepThrough]
		public static v128 cvtps_ph(v128 a, int rounding)
		{
			if (rounding == 12)
			{
				switch (MXCSR & MXCSRBits.RoundingControlMask)
				{
				case MXCSRBits.RoundToNearest:
					rounding = 8;
					break;
				case MXCSRBits.RoundDown:
					rounding = 9;
					break;
				case MXCSRBits.RoundUp:
					rounding = 10;
					break;
				case MXCSRBits.RoundingControlMask:
					rounding = 11;
					break;
				}
			}
			return new v128(FloatToHalf(a.UInt0, rounding), FloatToHalf(a.UInt1, rounding), FloatToHalf(a.UInt2, rounding), FloatToHalf(a.UInt3, rounding), 0, 0, 0, 0);
		}

		[DebuggerStepThrough]
		public static v128 mm256_cvtps_ph(v256 a, int rounding)
		{
			if (rounding == 12)
			{
				switch (MXCSR & MXCSRBits.RoundingControlMask)
				{
				case MXCSRBits.RoundToNearest:
					rounding = 8;
					break;
				case MXCSRBits.RoundDown:
					rounding = 9;
					break;
				case MXCSRBits.RoundUp:
					rounding = 10;
					break;
				case MXCSRBits.RoundingControlMask:
					rounding = 11;
					break;
				}
			}
			return new v128(FloatToHalf(a.UInt0, rounding), FloatToHalf(a.UInt1, rounding), FloatToHalf(a.UInt2, rounding), FloatToHalf(a.UInt3, rounding), FloatToHalf(a.UInt4, rounding), FloatToHalf(a.UInt5, rounding), FloatToHalf(a.UInt6, rounding), FloatToHalf(a.UInt7, rounding));
		}
	}

	public static class Fma
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct Union
		{
			[FieldOffset(0)]
			public float f;

			[FieldOffset(0)]
			public uint u;
		}

		public static bool IsFmaSupported => Avx2.IsAvx2Supported;

		[DebuggerStepThrough]
		private static float FmaHelper(float a, float b, float c)
		{
			return (float)((double)a * (double)b + (double)c);
		}

		[DebuggerStepThrough]
		private static float FnmaHelper(float a, float b, float c)
		{
			return FmaHelper(0f - a, b, c);
		}

		[DebuggerStepThrough]
		public static v128 fmadd_pd(v128 a, v128 b, v128 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v256 mm256_fmadd_pd(v256 a, v256 b, v256 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v128 fmadd_ps(v128 a, v128 b, v128 c)
		{
			return new v128(FmaHelper(a.Float0, b.Float0, c.Float0), FmaHelper(a.Float1, b.Float1, c.Float1), FmaHelper(a.Float2, b.Float2, c.Float2), FmaHelper(a.Float3, b.Float3, c.Float3));
		}

		[DebuggerStepThrough]
		public static v256 mm256_fmadd_ps(v256 a, v256 b, v256 c)
		{
			return new v256(FmaHelper(a.Float0, b.Float0, c.Float0), FmaHelper(a.Float1, b.Float1, c.Float1), FmaHelper(a.Float2, b.Float2, c.Float2), FmaHelper(a.Float3, b.Float3, c.Float3), FmaHelper(a.Float4, b.Float4, c.Float4), FmaHelper(a.Float5, b.Float5, c.Float5), FmaHelper(a.Float6, b.Float6, c.Float6), FmaHelper(a.Float7, b.Float7, c.Float7));
		}

		[DebuggerStepThrough]
		public static v128 fmadd_sd(v128 a, v128 b, v128 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v128 fmadd_ss(v128 a, v128 b, v128 c)
		{
			v128 result = a;
			result.Float0 = FmaHelper(a.Float0, b.Float0, c.Float0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 fmaddsub_pd(v128 a, v128 b, v128 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v256 mm256_fmaddsub_pd(v256 a, v256 b, v256 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v128 fmaddsub_ps(v128 a, v128 b, v128 c)
		{
			return new v128(FmaHelper(a.Float0, b.Float0, 0f - c.Float0), FmaHelper(a.Float1, b.Float1, c.Float1), FmaHelper(a.Float2, b.Float2, 0f - c.Float2), FmaHelper(a.Float3, b.Float3, c.Float3));
		}

		[DebuggerStepThrough]
		public static v256 mm256_fmaddsub_ps(v256 a, v256 b, v256 c)
		{
			return new v256(FmaHelper(a.Float0, b.Float0, 0f - c.Float0), FmaHelper(a.Float1, b.Float1, c.Float1), FmaHelper(a.Float2, b.Float2, 0f - c.Float2), FmaHelper(a.Float3, b.Float3, c.Float3), FmaHelper(a.Float4, b.Float4, 0f - c.Float4), FmaHelper(a.Float5, b.Float5, c.Float5), FmaHelper(a.Float6, b.Float6, 0f - c.Float6), FmaHelper(a.Float7, b.Float7, c.Float7));
		}

		[DebuggerStepThrough]
		public static v128 fmsub_pd(v128 a, v128 b, v128 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v256 mm256_fmsub_pd(v256 a, v256 b, v256 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v128 fmsub_ps(v128 a, v128 b, v128 c)
		{
			return new v128(FmaHelper(a.Float0, b.Float0, 0f - c.Float0), FmaHelper(a.Float1, b.Float1, 0f - c.Float1), FmaHelper(a.Float2, b.Float2, 0f - c.Float2), FmaHelper(a.Float3, b.Float3, 0f - c.Float3));
		}

		[DebuggerStepThrough]
		public static v256 mm256_fmsub_ps(v256 a, v256 b, v256 c)
		{
			return new v256(FmaHelper(a.Float0, b.Float0, 0f - c.Float0), FmaHelper(a.Float1, b.Float1, 0f - c.Float1), FmaHelper(a.Float2, b.Float2, 0f - c.Float2), FmaHelper(a.Float3, b.Float3, 0f - c.Float3), FmaHelper(a.Float4, b.Float4, 0f - c.Float4), FmaHelper(a.Float5, b.Float5, 0f - c.Float5), FmaHelper(a.Float6, b.Float6, 0f - c.Float6), FmaHelper(a.Float7, b.Float7, 0f - c.Float7));
		}

		[DebuggerStepThrough]
		public static v128 fmsub_sd(v128 a, v128 b, v128 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v128 fmsub_ss(v128 a, v128 b, v128 c)
		{
			v128 result = a;
			result.Float0 = FmaHelper(a.Float0, b.Float0, 0f - c.Float0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 fmsubadd_pd(v128 a, v128 b, v128 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v256 mm256_fmsubadd_pd(v256 a, v256 b, v256 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v128 fmsubadd_ps(v128 a, v128 b, v128 c)
		{
			return new v128(FmaHelper(a.Float0, b.Float0, c.Float0), FmaHelper(a.Float1, b.Float1, 0f - c.Float1), FmaHelper(a.Float2, b.Float2, c.Float2), FmaHelper(a.Float3, b.Float3, 0f - c.Float3));
		}

		[DebuggerStepThrough]
		public static v256 mm256_fmsubadd_ps(v256 a, v256 b, v256 c)
		{
			return new v256(FmaHelper(a.Float0, b.Float0, c.Float0), FmaHelper(a.Float1, b.Float1, 0f - c.Float1), FmaHelper(a.Float2, b.Float2, c.Float2), FmaHelper(a.Float3, b.Float3, 0f - c.Float3), FmaHelper(a.Float4, b.Float4, c.Float4), FmaHelper(a.Float5, b.Float5, 0f - c.Float5), FmaHelper(a.Float6, b.Float6, c.Float6), FmaHelper(a.Float7, b.Float7, 0f - c.Float7));
		}

		[DebuggerStepThrough]
		public static v128 fnmadd_pd(v128 a, v128 b, v128 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v256 mm256_fnmadd_pd(v256 a, v256 b, v256 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v128 fnmadd_ps(v128 a, v128 b, v128 c)
		{
			return new v128(FnmaHelper(a.Float0, b.Float0, c.Float0), FnmaHelper(a.Float1, b.Float1, c.Float1), FnmaHelper(a.Float2, b.Float2, c.Float2), FnmaHelper(a.Float3, b.Float3, c.Float3));
		}

		[DebuggerStepThrough]
		public static v256 mm256_fnmadd_ps(v256 a, v256 b, v256 c)
		{
			return new v256(FnmaHelper(a.Float0, b.Float0, c.Float0), FnmaHelper(a.Float1, b.Float1, c.Float1), FnmaHelper(a.Float2, b.Float2, c.Float2), FnmaHelper(a.Float3, b.Float3, c.Float3), FnmaHelper(a.Float4, b.Float4, c.Float4), FnmaHelper(a.Float5, b.Float5, c.Float5), FnmaHelper(a.Float6, b.Float6, c.Float6), FnmaHelper(a.Float7, b.Float7, c.Float7));
		}

		[DebuggerStepThrough]
		public static v128 fnmadd_sd(v128 a, v128 b, v128 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v128 fnmadd_ss(v128 a, v128 b, v128 c)
		{
			v128 result = a;
			result.Float0 = FnmaHelper(a.Float0, b.Float0, c.Float0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 fnmsub_pd(v128 a, v128 b, v128 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v256 mm256_fnmsub_pd(v256 a, v256 b, v256 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v128 fnmsub_ps(v128 a, v128 b, v128 c)
		{
			return new v128(FnmaHelper(a.Float0, b.Float0, 0f - c.Float0), FnmaHelper(a.Float1, b.Float1, 0f - c.Float1), FnmaHelper(a.Float2, b.Float2, 0f - c.Float2), FnmaHelper(a.Float3, b.Float3, 0f - c.Float3));
		}

		[DebuggerStepThrough]
		public static v256 mm256_fnmsub_ps(v256 a, v256 b, v256 c)
		{
			return new v256(FnmaHelper(a.Float0, b.Float0, 0f - c.Float0), FnmaHelper(a.Float1, b.Float1, 0f - c.Float1), FnmaHelper(a.Float2, b.Float2, 0f - c.Float2), FnmaHelper(a.Float3, b.Float3, 0f - c.Float3), FnmaHelper(a.Float4, b.Float4, 0f - c.Float4), FnmaHelper(a.Float5, b.Float5, 0f - c.Float5), FnmaHelper(a.Float6, b.Float6, 0f - c.Float6), FnmaHelper(a.Float7, b.Float7, 0f - c.Float7));
		}

		[DebuggerStepThrough]
		public static v128 fnmsub_sd(v128 a, v128 b, v128 c)
		{
			throw new Exception("Double-precision FMA not emulated in C#");
		}

		[DebuggerStepThrough]
		public static v128 fnmsub_ss(v128 a, v128 b, v128 c)
		{
			v128 result = a;
			result.Float0 = FnmaHelper(a.Float0, b.Float0, 0f - c.Float0);
			return result;
		}
	}

	public static class Popcnt
	{
		public static bool IsPopcntSupported => Sse4_2.IsSse42Supported;

		[DebuggerStepThrough]
		public static int popcnt_u32(uint v)
		{
			int num = 0;
			for (uint num2 = 2147483648u; num2 != 0; num2 >>= 1)
			{
				num += (((v & num2) != 0) ? 1 : 0);
			}
			return num;
		}

		[DebuggerStepThrough]
		public static int popcnt_u64(ulong v)
		{
			int num = 0;
			for (ulong num2 = 9223372036854775808uL; num2 != 0L; num2 >>= 1)
			{
				num += (((v & num2) != 0L) ? 1 : 0);
			}
			return num;
		}
	}

	public static class Sse
	{
		public static bool IsSseSupported => false;

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public unsafe static v128 load_ps(void* ptr)
		{
			return GenericCSharpLoad(ptr);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public unsafe static v128 loadu_ps(void* ptr)
		{
			return GenericCSharpLoad(ptr);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public unsafe static void store_ps(void* ptr, v128 val)
		{
			GenericCSharpStore(ptr, val);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public unsafe static void storeu_ps(void* ptr, v128 val)
		{
			GenericCSharpStore(ptr, val);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public unsafe static void stream_ps(void* mem_addr, v128 a)
		{
			GenericCSharpStore(mem_addr, a);
		}

		[DebuggerStepThrough]
		public static v128 cvtsi32_ss(v128 a, int b)
		{
			v128 result = a;
			result.Float0 = b;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cvtsi64_ss(v128 a, long b)
		{
			v128 result = a;
			result.Float0 = b;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 add_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 += b.Float0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 add_ps(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 += b.Float0;
			result.Float1 += b.Float1;
			result.Float2 += b.Float2;
			result.Float3 += b.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 sub_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 = a.Float0 - b.Float0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 sub_ps(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 -= b.Float0;
			result.Float1 -= b.Float1;
			result.Float2 -= b.Float2;
			result.Float3 -= b.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 mul_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 = a.Float0 * b.Float0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 mul_ps(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 *= b.Float0;
			result.Float1 *= b.Float1;
			result.Float2 *= b.Float2;
			result.Float3 *= b.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 div_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 = a.Float0 / b.Float0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 div_ps(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 /= b.Float0;
			result.Float1 /= b.Float1;
			result.Float2 /= b.Float2;
			result.Float3 /= b.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 sqrt_ss(v128 a)
		{
			v128 result = a;
			result.Float0 = (float)Math.Sqrt(a.Float0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 sqrt_ps(v128 a)
		{
			v128 result = default(v128);
			result.Float0 = (float)Math.Sqrt(a.Float0);
			result.Float1 = (float)Math.Sqrt(a.Float1);
			result.Float2 = (float)Math.Sqrt(a.Float2);
			result.Float3 = (float)Math.Sqrt(a.Float3);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 rcp_ss(v128 a)
		{
			v128 result = a;
			result.Float0 = 1f / a.Float0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 rcp_ps(v128 a)
		{
			v128 result = default(v128);
			result.Float0 = 1f / a.Float0;
			result.Float1 = 1f / a.Float1;
			result.Float2 = 1f / a.Float2;
			result.Float3 = 1f / a.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 rsqrt_ss(v128 a)
		{
			v128 result = a;
			result.Float0 = 1f / (float)Math.Sqrt(a.Float0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 rsqrt_ps(v128 a)
		{
			v128 result = default(v128);
			result.Float0 = 1f / (float)Math.Sqrt(a.Float0);
			result.Float1 = 1f / (float)Math.Sqrt(a.Float1);
			result.Float2 = 1f / (float)Math.Sqrt(a.Float2);
			result.Float3 = 1f / (float)Math.Sqrt(a.Float3);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 min_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 = Math.Min(a.Float0, b.Float0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 min_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Float0 = Math.Min(a.Float0, b.Float0);
			result.Float1 = Math.Min(a.Float1, b.Float1);
			result.Float2 = Math.Min(a.Float2, b.Float2);
			result.Float3 = Math.Min(a.Float3, b.Float3);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 max_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 = Math.Max(a.Float0, b.Float0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 max_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Float0 = Math.Max(a.Float0, b.Float0);
			result.Float1 = Math.Max(a.Float1, b.Float1);
			result.Float2 = Math.Max(a.Float2, b.Float2);
			result.Float3 = Math.Max(a.Float3, b.Float3);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 and_ps(v128 a, v128 b)
		{
			v128 result = a;
			result.UInt0 &= b.UInt0;
			result.UInt1 &= b.UInt1;
			result.UInt2 &= b.UInt2;
			result.UInt3 &= b.UInt3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 andnot_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = ~a.UInt0 & b.UInt0;
			result.UInt1 = ~a.UInt1 & b.UInt1;
			result.UInt2 = ~a.UInt2 & b.UInt2;
			result.UInt3 = ~a.UInt3 & b.UInt3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 or_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = a.UInt0 | b.UInt0;
			result.UInt1 = a.UInt1 | b.UInt1;
			result.UInt2 = a.UInt2 | b.UInt2;
			result.UInt3 = a.UInt3 | b.UInt3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 xor_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = a.UInt0 ^ b.UInt0;
			result.UInt1 = a.UInt1 ^ b.UInt1;
			result.UInt2 = a.UInt2 ^ b.UInt2;
			result.UInt3 = a.UInt3 ^ b.UInt3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpeq_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.UInt0 = ((a.Float0 == b.Float0) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpeq_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = ((a.Float0 == b.Float0) ? uint.MaxValue : 0u);
			result.UInt1 = ((a.Float1 == b.Float1) ? uint.MaxValue : 0u);
			result.UInt2 = ((a.Float2 == b.Float2) ? uint.MaxValue : 0u);
			result.UInt3 = ((a.Float3 == b.Float3) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmplt_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.UInt0 = ((a.Float0 < b.Float0) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmplt_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = ((a.Float0 < b.Float0) ? uint.MaxValue : 0u);
			result.UInt1 = ((a.Float1 < b.Float1) ? uint.MaxValue : 0u);
			result.UInt2 = ((a.Float2 < b.Float2) ? uint.MaxValue : 0u);
			result.UInt3 = ((a.Float3 < b.Float3) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmple_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.UInt0 = ((a.Float0 <= b.Float0) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmple_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = ((a.Float0 <= b.Float0) ? uint.MaxValue : 0u);
			result.UInt1 = ((a.Float1 <= b.Float1) ? uint.MaxValue : 0u);
			result.UInt2 = ((a.Float2 <= b.Float2) ? uint.MaxValue : 0u);
			result.UInt3 = ((a.Float3 <= b.Float3) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpgt_ss(v128 a, v128 b)
		{
			return cmplt_ss(b, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpgt_ps(v128 a, v128 b)
		{
			return cmplt_ps(b, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpge_ss(v128 a, v128 b)
		{
			return cmple_ss(b, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpge_ps(v128 a, v128 b)
		{
			return cmple_ps(b, a);
		}

		[DebuggerStepThrough]
		public static v128 cmpneq_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.UInt0 = ((a.Float0 != b.Float0) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpneq_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = ((a.Float0 != b.Float0) ? uint.MaxValue : 0u);
			result.UInt1 = ((a.Float1 != b.Float1) ? uint.MaxValue : 0u);
			result.UInt2 = ((a.Float2 != b.Float2) ? uint.MaxValue : 0u);
			result.UInt3 = ((a.Float3 != b.Float3) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpnlt_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.UInt0 = ((!(a.Float0 < b.Float0)) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpnlt_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = ((!(a.Float0 < b.Float0)) ? uint.MaxValue : 0u);
			result.UInt1 = ((!(a.Float1 < b.Float1)) ? uint.MaxValue : 0u);
			result.UInt2 = ((!(a.Float2 < b.Float2)) ? uint.MaxValue : 0u);
			result.UInt3 = ((!(a.Float3 < b.Float3)) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpnle_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.UInt0 = ((!(a.Float0 <= b.Float0)) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpnle_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = ((!(a.Float0 <= b.Float0)) ? uint.MaxValue : 0u);
			result.UInt1 = ((!(a.Float1 <= b.Float1)) ? uint.MaxValue : 0u);
			result.UInt2 = ((!(a.Float2 <= b.Float2)) ? uint.MaxValue : 0u);
			result.UInt3 = ((!(a.Float3 <= b.Float3)) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpngt_ss(v128 a, v128 b)
		{
			return cmpnlt_ss(b, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpngt_ps(v128 a, v128 b)
		{
			return cmpnlt_ps(b, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpnge_ss(v128 a, v128 b)
		{
			return cmpnle_ss(b, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpnge_ps(v128 a, v128 b)
		{
			return cmpnle_ps(b, a);
		}

		[DebuggerStepThrough]
		public static v128 cmpord_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.UInt0 = ((!IsNaN(a.UInt0) && !IsNaN(b.UInt0)) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpord_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = ((!IsNaN(a.UInt0) && !IsNaN(b.UInt0)) ? uint.MaxValue : 0u);
			result.UInt1 = ((!IsNaN(a.UInt1) && !IsNaN(b.UInt1)) ? uint.MaxValue : 0u);
			result.UInt2 = ((!IsNaN(a.UInt2) && !IsNaN(b.UInt2)) ? uint.MaxValue : 0u);
			result.UInt3 = ((!IsNaN(a.UInt3) && !IsNaN(b.UInt3)) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpunord_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.UInt0 = ((IsNaN(a.UInt0) || IsNaN(b.UInt0)) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpunord_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = ((IsNaN(a.UInt0) || IsNaN(b.UInt0)) ? uint.MaxValue : 0u);
			result.UInt1 = ((IsNaN(a.UInt1) || IsNaN(b.UInt1)) ? uint.MaxValue : 0u);
			result.UInt2 = ((IsNaN(a.UInt2) || IsNaN(b.UInt2)) ? uint.MaxValue : 0u);
			result.UInt3 = ((IsNaN(a.UInt3) || IsNaN(b.UInt3)) ? uint.MaxValue : 0u);
			return result;
		}

		[DebuggerStepThrough]
		public static int comieq_ss(v128 a, v128 b)
		{
			if (a.Float0 != b.Float0)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int comilt_ss(v128 a, v128 b)
		{
			if (!(a.Float0 < b.Float0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int comile_ss(v128 a, v128 b)
		{
			if (!(a.Float0 <= b.Float0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int comigt_ss(v128 a, v128 b)
		{
			if (!(a.Float0 > b.Float0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int comige_ss(v128 a, v128 b)
		{
			if (!(a.Float0 >= b.Float0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int comineq_ss(v128 a, v128 b)
		{
			if (a.Float0 == b.Float0)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomieq_ss(v128 a, v128 b)
		{
			if (a.Float0 != b.Float0)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomilt_ss(v128 a, v128 b)
		{
			if (!(a.Float0 < b.Float0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomile_ss(v128 a, v128 b)
		{
			if (!(a.Float0 <= b.Float0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomigt_ss(v128 a, v128 b)
		{
			if (!(a.Float0 > b.Float0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomige_ss(v128 a, v128 b)
		{
			if (!(a.Float0 >= b.Float0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomineq_ss(v128 a, v128 b)
		{
			if (a.Float0 == b.Float0)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static int cvtss_si32(v128 a)
		{
			return cvt_ss2si(a);
		}

		[DebuggerStepThrough]
		public static int cvt_ss2si(v128 a)
		{
			return (int)Math.Round(a.Float0, MidpointRounding.ToEven);
		}

		[DebuggerStepThrough]
		public static long cvtss_si64(v128 a)
		{
			return (long)Math.Round(a.Float0, MidpointRounding.ToEven);
		}

		[DebuggerStepThrough]
		public static float cvtss_f32(v128 a)
		{
			return a.Float0;
		}

		[DebuggerStepThrough]
		public static int cvttss_si32(v128 a)
		{
			using (new RoundingScope(MXCSRBits.RoundingControlMask))
			{
				return (int)a.Float0;
			}
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static int cvtt_ss2si(v128 a)
		{
			return cvttss_si32(a);
		}

		[DebuggerStepThrough]
		public static long cvttss_si64(v128 a)
		{
			using (new RoundingScope(MXCSRBits.RoundingControlMask))
			{
				return (long)a.Float0;
			}
		}

		[DebuggerStepThrough]
		public static v128 set_ss(float a)
		{
			return new v128(a, 0f, 0f, 0f);
		}

		[DebuggerStepThrough]
		public static v128 set1_ps(float a)
		{
			return new v128(a, a, a, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 set_ps1(float a)
		{
			return set1_ps(a);
		}

		[DebuggerStepThrough]
		public static v128 set_ps(float e3, float e2, float e1, float e0)
		{
			return new v128(e0, e1, e2, e3);
		}

		[DebuggerStepThrough]
		public static v128 setr_ps(float e3, float e2, float e1, float e0)
		{
			return new v128(e3, e2, e1, e0);
		}

		[DebuggerStepThrough]
		public static v128 move_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 = b.Float0;
			return result;
		}

		public static int SHUFFLE(int d, int c, int b, int a)
		{
			return (a & 3) | ((b & 3) << 2) | ((c & 3) << 4) | ((d & 3) << 6);
		}

		[DebuggerStepThrough]
		public unsafe static v128 shuffle_ps(v128 a, v128 b, int imm8)
		{
			v128 result = default(v128);
			uint* ptr = &a.UInt0;
			uint* ptr2 = &b.UInt0;
			result.UInt0 = ptr[imm8 & 3];
			result.UInt1 = ptr[(imm8 >> 2) & 3];
			result.UInt2 = ptr2[(imm8 >> 4) & 3];
			result.UInt3 = ptr2[(imm8 >> 6) & 3];
			return result;
		}

		[DebuggerStepThrough]
		public static v128 unpackhi_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Float0 = a.Float2;
			result.Float1 = b.Float2;
			result.Float2 = a.Float3;
			result.Float3 = b.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 unpacklo_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Float0 = a.Float0;
			result.Float1 = b.Float0;
			result.Float2 = a.Float1;
			result.Float3 = b.Float1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 movehl_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Float0 = b.Float2;
			result.Float1 = b.Float3;
			result.Float2 = a.Float2;
			result.Float3 = a.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 movelh_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Float0 = a.Float0;
			result.Float1 = a.Float1;
			result.Float2 = b.Float0;
			result.Float3 = b.Float1;
			return result;
		}

		[DebuggerStepThrough]
		public static int movemask_ps(v128 a)
		{
			int num = 0;
			if ((a.UInt0 & 0x80000000u) != 0)
			{
				num |= 1;
			}
			if ((a.UInt1 & 0x80000000u) != 0)
			{
				num |= 2;
			}
			if ((a.UInt2 & 0x80000000u) != 0)
			{
				num |= 4;
			}
			if ((a.UInt3 & 0x80000000u) != 0)
			{
				num |= 8;
			}
			return num;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static void TRANSPOSE4_PS(ref v128 row0, ref v128 row1, ref v128 row2, ref v128 row3)
		{
			v128 a = shuffle_ps(row0, row1, 68);
			v128 a2 = shuffle_ps(row0, row1, 238);
			v128 b = shuffle_ps(row2, row3, 68);
			v128 b2 = shuffle_ps(row2, row3, 238);
			row0 = shuffle_ps(a, b, 136);
			row1 = shuffle_ps(a, b, 221);
			row2 = shuffle_ps(a2, b2, 136);
			row3 = shuffle_ps(a2, b2, 221);
		}

		[DebuggerStepThrough]
		public static v128 setzero_ps()
		{
			return default(v128);
		}

		[DebuggerStepThrough]
		public unsafe static v128 loadu_si16(void* mem_addr)
		{
			return new v128(*(short*)mem_addr, 0, 0, 0, 0, 0, 0, 0);
		}

		public unsafe static void storeu_si16(void* mem_addr, v128 a)
		{
			*(short*)mem_addr = a.SShort0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 loadu_si64(void* mem_addr)
		{
			return new v128(*(long*)mem_addr, 0L);
		}

		[DebuggerStepThrough]
		public unsafe static void storeu_si64(void* mem_addr, v128 a)
		{
			*(long*)mem_addr = a.SLong0;
		}
	}

	public static class Sse2
	{
		public static bool IsSse2Supported => false;

		[DebuggerStepThrough]
		public static int SHUFFLE2(int x, int y)
		{
			return y | (x << 1);
		}

		[DebuggerStepThrough]
		public unsafe static void stream_si32(int* mem_addr, int a)
		{
			*mem_addr = a;
		}

		[DebuggerStepThrough]
		public unsafe static void stream_si64(long* mem_addr, long a)
		{
			*mem_addr = a;
		}

		[DebuggerStepThrough]
		public unsafe static void stream_pd(void* mem_addr, v128 a)
		{
			GenericCSharpStore(mem_addr, a);
		}

		[DebuggerStepThrough]
		public unsafe static void stream_si128(void* mem_addr, v128 a)
		{
			GenericCSharpStore(mem_addr, a);
		}

		[DebuggerStepThrough]
		public unsafe static v128 add_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			sbyte* ptr = &result.SByte0;
			sbyte* ptr2 = &a.SByte0;
			sbyte* ptr3 = &b.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = (sbyte)(ptr2[i] + ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 add_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = (short)(ptr2[i] + ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 add_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.SInt0 = a.SInt0 + b.SInt0;
			result.SInt1 = a.SInt1 + b.SInt1;
			result.SInt2 = a.SInt2 + b.SInt2;
			result.SInt3 = a.SInt3 + b.SInt3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 add_epi64(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.SLong0 = a.SLong0 + b.SLong0;
			result.SLong1 = a.SLong1 + b.SLong1;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 adds_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			sbyte* ptr = &result.SByte0;
			sbyte* ptr2 = &a.SByte0;
			sbyte* ptr3 = &b.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = Saturate_To_Int8(ptr2[i] + ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 adds_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = Saturate_To_Int16(ptr2[i] + ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 adds_epu8(v128 a, v128 b)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			byte* ptr3 = &b.Byte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = Saturate_To_UnsignedInt8(ptr2[i] + ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 adds_epu16(v128 a, v128 b)
		{
			v128 result = default(v128);
			ushort* ptr = &result.UShort0;
			ushort* ptr2 = &a.UShort0;
			ushort* ptr3 = &b.UShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = Saturate_To_UnsignedInt16(ptr2[i] + ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 avg_epu8(v128 a, v128 b)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			byte* ptr3 = &b.Byte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = (byte)(ptr2[i] + ptr3[i] + 1 >> 1);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 avg_epu16(v128 a, v128 b)
		{
			v128 result = default(v128);
			ushort* ptr = &result.UShort0;
			ushort* ptr2 = &a.UShort0;
			ushort* ptr3 = &b.UShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = (ushort)(ptr2[i] + ptr3[i] + 1 >> 1);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 madd_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			int* ptr = &result.SInt0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 3; i++)
			{
				int num = 2 * i;
				int num2 = ptr2[num + 1] * ptr3[num + 1];
				int num3 = ptr2[num] * ptr3[num];
				ptr[i] = num2 + num3;
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 max_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = Math.Max(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 max_epu8(v128 a, v128 b)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			byte* ptr3 = &b.Byte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = Math.Max(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 min_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = Math.Min(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 min_epu8(v128 a, v128 b)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			byte* ptr3 = &b.Byte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = Math.Min(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mulhi_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				int num = ptr2[i] * ptr3[i];
				ptr[i] = (short)(num >> 16);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mulhi_epu16(v128 a, v128 b)
		{
			v128 result = default(v128);
			ushort* ptr = &result.UShort0;
			ushort* ptr2 = &a.UShort0;
			ushort* ptr3 = &b.UShort0;
			for (int i = 0; i <= 7; i++)
			{
				uint num = (uint)(ptr2[i] * ptr3[i]);
				ptr[i] = (ushort)(num >> 16);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mullo_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				int num = ptr2[i] * ptr3[i];
				ptr[i] = (short)num;
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 mul_epu32(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)a.UInt0 * (ulong)b.UInt0;
			result.ULong1 = (ulong)a.UInt2 * (ulong)b.UInt2;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sad_epu8(v128 a, v128 b)
		{
			v128 v257 = default(v128);
			byte* ptr = &v257.Byte0;
			byte* ptr2 = &a.Byte0;
			byte* ptr3 = &b.Byte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = (byte)Math.Abs(ptr2[i] - ptr3[i]);
			}
			v128 result = default(v128);
			ushort* ptr4 = &result.UShort0;
			for (int j = 0; j <= 1; j++)
			{
				int num = j * 8;
				ptr4[4 * j] = (ushort)(ptr[num] + ptr[num + 1] + ptr[num + 2] + ptr[num + 3] + ptr[num + 4] + ptr[num + 5] + ptr[num + 6] + ptr[num + 7]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sub_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			sbyte* ptr = &result.SByte0;
			sbyte* ptr2 = &a.SByte0;
			sbyte* ptr3 = &b.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = (sbyte)(ptr2[i] - ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sub_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = (short)(ptr2[i] - ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sub_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			int* ptr = &result.SInt0;
			int* ptr2 = &a.SInt0;
			int* ptr3 = &b.SInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i] - ptr3[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sub_epi64(v128 a, v128 b)
		{
			v128 result = default(v128);
			long* ptr = &result.SLong0;
			long* ptr2 = &a.SLong0;
			long* ptr3 = &b.SLong0;
			for (int i = 0; i <= 1; i++)
			{
				ptr[i] = ptr2[i] - ptr3[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 subs_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			sbyte* ptr = &result.SByte0;
			sbyte* ptr2 = &a.SByte0;
			sbyte* ptr3 = &b.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = Saturate_To_Int8(ptr2[i] - ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 subs_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = Saturate_To_Int16(ptr2[i] - ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 subs_epu8(v128 a, v128 b)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			byte* ptr3 = &b.Byte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = Saturate_To_UnsignedInt8(ptr2[i] - ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 subs_epu16(v128 a, v128 b)
		{
			v128 result = default(v128);
			ushort* ptr = &result.UShort0;
			ushort* ptr2 = &a.UShort0;
			ushort* ptr3 = &b.UShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = Saturate_To_UnsignedInt16(ptr2[i] - ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 slli_si128(v128 a, int imm8)
		{
			int num = Math.Min(imm8 & 0xFF, 16);
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			for (int i = 0; i < num; i++)
			{
				ptr[i] = 0;
			}
			for (int j = num; j < 16; j++)
			{
				ptr[j] = ptr2[j - num];
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 bslli_si128(v128 a, int imm8)
		{
			return slli_si128(a, imm8);
		}

		[DebuggerStepThrough]
		public unsafe static v128 bsrli_si128(v128 a, int imm8)
		{
			int num = Math.Min(imm8 & 0xFF, 16);
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			for (int i = 0; i < 16 - num; i++)
			{
				ptr[i] = ptr2[num + i];
			}
			for (int j = 16 - num; j < 16; j++)
			{
				ptr[j] = 0;
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 slli_epi16(v128 a, int imm8)
		{
			v128 result = default(v128);
			int num = imm8 & 0xFF;
			ushort* ptr = &result.UShort0;
			ushort* ptr2 = &a.UShort0;
			for (int i = 0; i <= 7; i++)
			{
				if (num > 15)
				{
					ptr[i] = 0;
				}
				else
				{
					ptr[i] = (ushort)(ptr2[i] << num);
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sll_epi16(v128 a, v128 count)
		{
			v128 result = default(v128);
			int num = (int)Math.Min(count.ULong0, 16uL);
			ushort* ptr = &result.UShort0;
			ushort* ptr2 = &a.UShort0;
			for (int i = 0; i <= 7; i++)
			{
				if (num > 15)
				{
					ptr[i] = 0;
				}
				else
				{
					ptr[i] = (ushort)(ptr2[i] << num);
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 slli_epi32(v128 a, int imm8)
		{
			v128 result = default(v128);
			int num = Math.Min(imm8 & 0xFF, 32);
			uint* ptr = &result.UInt0;
			uint* ptr2 = &a.UInt0;
			for (int i = 0; i <= 3; i++)
			{
				if (num > 31)
				{
					ptr[i] = 0u;
				}
				else
				{
					ptr[i] = ptr2[i] << num;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sll_epi32(v128 a, v128 count)
		{
			v128 result = default(v128);
			int num = (int)Math.Min(count.ULong0, 32uL);
			uint* ptr = &result.UInt0;
			uint* ptr2 = &a.UInt0;
			for (int i = 0; i <= 3; i++)
			{
				if (num > 31)
				{
					ptr[i] = 0u;
				}
				else
				{
					ptr[i] = ptr2[i] << num;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 slli_epi64(v128 a, int imm8)
		{
			v128 result = default(v128);
			int num = Math.Min(imm8 & 0xFF, 64);
			ulong* ptr = &result.ULong0;
			ulong* ptr2 = &a.ULong0;
			for (int i = 0; i <= 1; i++)
			{
				if (num > 63)
				{
					ptr[i] = 0uL;
				}
				else
				{
					ptr[i] = ptr2[i] << num;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sll_epi64(v128 a, v128 count)
		{
			v128 result = default(v128);
			int num = (int)Math.Min(count.ULong0, 64uL);
			ulong* ptr = &result.ULong0;
			ulong* ptr2 = &a.ULong0;
			for (int i = 0; i <= 1; i++)
			{
				if (num > 63)
				{
					ptr[i] = 0uL;
				}
				else
				{
					ptr[i] = ptr2[i] << num;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 srai_epi16(v128 a, int imm8)
		{
			int num = Math.Min(imm8 & 0xFF, 16);
			v128 result = a;
			short* ptr = &result.SShort0;
			if (num > 0)
			{
				num--;
				for (int i = 0; i <= 7; i++)
				{
					ptr[i] >>= 1;
					short* num2 = ptr + i;
					*num2 = (short)(*num2 >> num);
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sra_epi16(v128 a, v128 count)
		{
			int num = (int)Math.Min(count.ULong0, 16uL);
			v128 result = a;
			short* ptr = &result.SShort0;
			if (num > 0)
			{
				num--;
				for (int i = 0; i <= 7; i++)
				{
					ptr[i] >>= 1;
					short* num2 = ptr + i;
					*num2 = (short)(*num2 >> num);
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 srai_epi32(v128 a, int imm8)
		{
			int num = Math.Min(imm8 & 0xFF, 32);
			v128 result = a;
			int* ptr = &result.SInt0;
			if (num > 0)
			{
				num--;
				for (int i = 0; i <= 3; i++)
				{
					ptr[i] >>= 1;
					ptr[i] >>= num;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sra_epi32(v128 a, v128 count)
		{
			int num = (int)Math.Min(count.ULong0, 32uL);
			v128 result = a;
			int* ptr = &result.SInt0;
			if (num > 0)
			{
				num--;
				for (int i = 0; i <= 3; i++)
				{
					ptr[i] >>= 1;
					ptr[i] >>= num;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 srli_si128(v128 a, int imm8)
		{
			return bsrli_si128(a, imm8);
		}

		[DebuggerStepThrough]
		public unsafe static v128 srli_epi16(v128 a, int imm8)
		{
			int num = Math.Min(imm8 & 0xFF, 16);
			v128 result = a;
			ushort* ptr = &result.UShort0;
			if (num > 0)
			{
				num--;
				for (int i = 0; i <= 7; i++)
				{
					ushort* num2 = ptr + i;
					*num2 >>= 1;
					ushort* num3 = ptr + i;
					*num3 = (ushort)(*num3 >> num);
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 srl_epi16(v128 a, v128 count)
		{
			int num = (int)Math.Min(count.ULong0, 16uL);
			v128 result = a;
			ushort* ptr = &result.UShort0;
			if (num > 0)
			{
				num--;
				for (int i = 0; i <= 7; i++)
				{
					ushort* num2 = ptr + i;
					*num2 >>= 1;
					ushort* num3 = ptr + i;
					*num3 = (ushort)(*num3 >> num);
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 srli_epi32(v128 a, int imm8)
		{
			int num = Math.Min(imm8 & 0xFF, 32);
			v128 result = a;
			uint* ptr = &result.UInt0;
			if (num > 0)
			{
				num--;
				for (int i = 0; i <= 3; i++)
				{
					ptr[i] >>>= 1;
					ptr[i] >>>= num;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 srl_epi32(v128 a, v128 count)
		{
			int num = (int)Math.Min(count.ULong0, 32uL);
			v128 result = a;
			uint* ptr = &result.UInt0;
			if (num > 0)
			{
				num--;
				for (int i = 0; i <= 3; i++)
				{
					ptr[i] >>>= 1;
					ptr[i] >>>= num;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 srli_epi64(v128 a, int imm8)
		{
			int num = Math.Min(imm8 & 0xFF, 64);
			v128 result = a;
			ulong* ptr = &result.ULong0;
			if (num > 0)
			{
				num--;
				for (int i = 0; i <= 1; i++)
				{
					ptr[i] >>>= 1;
					ptr[i] >>>= num;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 srl_epi64(v128 a, v128 count)
		{
			int num = (int)Math.Min(count.ULong0, 64uL);
			v128 result = a;
			ulong* ptr = &result.ULong0;
			if (num > 0)
			{
				num--;
				for (int i = 0; i <= 1; i++)
				{
					ptr[i] >>>= 1;
					ptr[i] >>>= num;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 and_si128(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = a.ULong0 & b.ULong0;
			result.ULong1 = a.ULong1 & b.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 andnot_si128(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = ~a.ULong0 & b.ULong0;
			result.ULong1 = ~a.ULong1 & b.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 or_si128(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = a.ULong0 | b.ULong0;
			result.ULong1 = a.ULong1 | b.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 xor_si128(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = a.ULong0 ^ b.ULong0;
			result.ULong1 = a.ULong1 ^ b.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cmpeq_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			byte* ptr = &a.Byte0;
			byte* ptr2 = &b.Byte0;
			byte* ptr3 = &result.Byte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr3[i] = (byte)((ptr[i] == ptr2[i]) ? 255u : 0u);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cmpeq_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			ushort* ptr = &a.UShort0;
			ushort* ptr2 = &b.UShort0;
			ushort* ptr3 = &result.UShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr3[i] = (ushort)((ptr[i] == ptr2[i]) ? 65535u : 0u);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cmpeq_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			uint* ptr = &a.UInt0;
			uint* ptr2 = &b.UInt0;
			uint* ptr3 = &result.UInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr3[i] = ((ptr[i] == ptr2[i]) ? uint.MaxValue : 0u);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cmpgt_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			sbyte* ptr = &a.SByte0;
			sbyte* ptr2 = &b.SByte0;
			sbyte* ptr3 = &result.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr3[i] = (sbyte)((ptr[i] > ptr2[i]) ? (-1) : 0);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cmpgt_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &a.SShort0;
			short* ptr2 = &b.SShort0;
			short* ptr3 = &result.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr3[i] = (short)((ptr[i] > ptr2[i]) ? (-1) : 0);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cmpgt_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			int* ptr = &a.SInt0;
			int* ptr2 = &b.SInt0;
			int* ptr3 = &result.SInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr3[i] = ((ptr[i] > ptr2[i]) ? (-1) : 0);
			}
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmplt_epi8(v128 a, v128 b)
		{
			return cmpgt_epi8(b, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmplt_epi16(v128 a, v128 b)
		{
			return cmpgt_epi16(b, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmplt_epi32(v128 a, v128 b)
		{
			return cmpgt_epi32(b, a);
		}

		[DebuggerStepThrough]
		public static v128 cvtepi32_pd(v128 a)
		{
			v128 result = default(v128);
			result.Double0 = a.SInt0;
			result.Double1 = a.SInt1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cvtsi32_sd(v128 a, int b)
		{
			v128 result = a;
			result.Double0 = b;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cvtsi64_sd(v128 a, long b)
		{
			v128 result = a;
			result.Double0 = b;
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cvtsi64x_sd(v128 a, long b)
		{
			return cvtsi64_sd(a, b);
		}

		[DebuggerStepThrough]
		public static v128 cvtepi32_ps(v128 a)
		{
			v128 result = default(v128);
			result.Float0 = a.SInt0;
			result.Float1 = a.SInt1;
			result.Float2 = a.SInt2;
			result.Float3 = a.SInt3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cvtsi32_si128(int a)
		{
			v128 result = default(v128);
			result.SInt0 = a;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cvtsi64_si128(long a)
		{
			v128 result = default(v128);
			result.SLong0 = a;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cvtsi64x_si128(long a)
		{
			return cvtsi64_si128(a);
		}

		[DebuggerStepThrough]
		public static int cvtsi128_si32(v128 a)
		{
			return a.SInt0;
		}

		[DebuggerStepThrough]
		public static long cvtsi128_si64(v128 a)
		{
			return a.SLong0;
		}

		[DebuggerStepThrough]
		public static long cvtsi128_si64x(v128 a)
		{
			return a.SLong0;
		}

		[DebuggerStepThrough]
		public static v128 set_epi64x(long e1, long e0)
		{
			v128 result = default(v128);
			result.SLong0 = e0;
			result.SLong1 = e1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 set_epi32(int e3, int e2, int e1, int e0)
		{
			v128 result = default(v128);
			result.SInt0 = e0;
			result.SInt1 = e1;
			result.SInt2 = e2;
			result.SInt3 = e3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 set_epi16(short e7, short e6, short e5, short e4, short e3, short e2, short e1, short e0)
		{
			v128 result = default(v128);
			result.SShort0 = e0;
			result.SShort1 = e1;
			result.SShort2 = e2;
			result.SShort3 = e3;
			result.SShort4 = e4;
			result.SShort5 = e5;
			result.SShort6 = e6;
			result.SShort7 = e7;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 set_epi8(sbyte e15_, sbyte e14_, sbyte e13_, sbyte e12_, sbyte e11_, sbyte e10_, sbyte e9_, sbyte e8_, sbyte e7_, sbyte e6_, sbyte e5_, sbyte e4_, sbyte e3_, sbyte e2_, sbyte e1_, sbyte e0_)
		{
			v128 result = default(v128);
			result.SByte0 = e0_;
			result.SByte1 = e1_;
			result.SByte2 = e2_;
			result.SByte3 = e3_;
			result.SByte4 = e4_;
			result.SByte5 = e5_;
			result.SByte6 = e6_;
			result.SByte7 = e7_;
			result.SByte8 = e8_;
			result.SByte9 = e9_;
			result.SByte10 = e10_;
			result.SByte11 = e11_;
			result.SByte12 = e12_;
			result.SByte13 = e13_;
			result.SByte14 = e14_;
			result.SByte15 = e15_;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 set1_epi64x(long a)
		{
			v128 result = default(v128);
			result.SLong0 = a;
			result.SLong1 = a;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 set1_epi32(int a)
		{
			v128 result = default(v128);
			result.SInt0 = a;
			result.SInt1 = a;
			result.SInt2 = a;
			result.SInt3 = a;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 set1_epi16(short a)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = a;
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 set1_epi8(sbyte a)
		{
			v128 result = default(v128);
			sbyte* ptr = &result.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = a;
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 setr_epi32(int e3, int e2, int e1, int e0)
		{
			v128 result = default(v128);
			result.SInt0 = e3;
			result.SInt1 = e2;
			result.SInt2 = e1;
			result.SInt3 = e0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 setr_epi16(short e7, short e6, short e5, short e4, short e3, short e2, short e1, short e0)
		{
			v128 result = default(v128);
			result.SShort0 = e7;
			result.SShort1 = e6;
			result.SShort2 = e5;
			result.SShort3 = e4;
			result.SShort4 = e3;
			result.SShort5 = e2;
			result.SShort6 = e1;
			result.SShort7 = e0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 setr_epi8(sbyte e15_, sbyte e14_, sbyte e13_, sbyte e12_, sbyte e11_, sbyte e10_, sbyte e9_, sbyte e8_, sbyte e7_, sbyte e6_, sbyte e5_, sbyte e4_, sbyte e3_, sbyte e2_, sbyte e1_, sbyte e0_)
		{
			v128 result = default(v128);
			result.SByte0 = e15_;
			result.SByte1 = e14_;
			result.SByte2 = e13_;
			result.SByte3 = e12_;
			result.SByte4 = e11_;
			result.SByte5 = e10_;
			result.SByte6 = e9_;
			result.SByte7 = e8_;
			result.SByte8 = e7_;
			result.SByte9 = e6_;
			result.SByte10 = e5_;
			result.SByte11 = e4_;
			result.SByte12 = e3_;
			result.SByte13 = e2_;
			result.SByte14 = e1_;
			result.SByte15 = e0_;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 setzero_si128()
		{
			return default(v128);
		}

		[DebuggerStepThrough]
		public static v128 move_epi64(v128 a)
		{
			v128 result = default(v128);
			result.ULong0 = a.ULong0;
			result.ULong1 = 0uL;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 packs_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &a.SShort0;
			short* ptr2 = &b.SShort0;
			sbyte* ptr3 = &result.SByte0;
			for (int i = 0; i < 8; i++)
			{
				ptr3[i] = Saturate_To_Int8(ptr[i]);
			}
			for (int j = 0; j < 8; j++)
			{
				ptr3[j + 8] = Saturate_To_Int8(ptr2[j]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 packs_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			int* ptr = &a.SInt0;
			int* ptr2 = &b.SInt0;
			short* ptr3 = &result.SShort0;
			for (int i = 0; i < 4; i++)
			{
				ptr3[i] = Saturate_To_Int16(ptr[i]);
			}
			for (int j = 0; j < 4; j++)
			{
				ptr3[j + 4] = Saturate_To_Int16(ptr2[j]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 packus_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &a.SShort0;
			short* ptr2 = &b.SShort0;
			byte* ptr3 = &result.Byte0;
			for (int i = 0; i < 8; i++)
			{
				ptr3[i] = Saturate_To_UnsignedInt8(ptr[i]);
			}
			for (int j = 0; j < 8; j++)
			{
				ptr3[j + 8] = Saturate_To_UnsignedInt8(ptr2[j]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static ushort extract_epi16(v128 a, int imm8)
		{
			return (&a.UShort0)[imm8 & 7];
		}

		[DebuggerStepThrough]
		public unsafe static v128 insert_epi16(v128 a, int i, int imm8)
		{
			v128 result = a;
			(&result.SShort0)[imm8 & 7] = (short)i;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static int movemask_epi8(v128 a)
		{
			int num = 0;
			byte* ptr = &a.Byte0;
			for (int i = 0; i <= 15; i++)
			{
				if ((ptr[i] & 0x80u) != 0)
				{
					num |= 1 << i;
				}
			}
			return num;
		}

		[DebuggerStepThrough]
		public unsafe static v128 shuffle_epi32(v128 a, int imm8)
		{
			v128 result = default(v128);
			uint* num = &result.UInt0;
			uint* ptr = &a.UInt0;
			*num = ptr[imm8 & 3];
			num[1] = ptr[(imm8 >> 2) & 3];
			num[2] = ptr[(imm8 >> 4) & 3];
			num[3] = ptr[(imm8 >> 6) & 3];
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 shufflehi_epi16(v128 a, int imm8)
		{
			v128 result = a;
			short* num = &result.SShort0;
			short* ptr = &a.SShort0;
			num[4] = ptr[4 + (imm8 & 3)];
			num[5] = ptr[4 + ((imm8 >> 2) & 3)];
			num[6] = ptr[4 + ((imm8 >> 4) & 3)];
			num[7] = ptr[4 + ((imm8 >> 6) & 3)];
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 shufflelo_epi16(v128 a, int imm8)
		{
			v128 result = a;
			short* num = &result.SShort0;
			short* ptr = &a.SShort0;
			*num = ptr[imm8 & 3];
			num[1] = ptr[(imm8 >> 2) & 3];
			num[2] = ptr[(imm8 >> 4) & 3];
			num[3] = ptr[(imm8 >> 6) & 3];
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 unpackhi_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			byte* ptr3 = &b.Byte0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[2 * i] = ptr2[i + 8];
				ptr[2 * i + 1] = ptr3[i + 8];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 unpackhi_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			ushort* ptr = &result.UShort0;
			ushort* ptr2 = &a.UShort0;
			ushort* ptr3 = &b.UShort0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[2 * i] = ptr2[i + 4];
				ptr[2 * i + 1] = ptr3[i + 4];
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 unpackhi_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = a.UInt2;
			result.UInt1 = b.UInt2;
			result.UInt2 = a.UInt3;
			result.UInt3 = b.UInt3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 unpackhi_epi64(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = a.ULong1;
			result.ULong1 = b.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 unpacklo_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			byte* ptr3 = &b.Byte0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[2 * i] = ptr2[i];
				ptr[2 * i + 1] = ptr3[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 unpacklo_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			ushort* ptr = &result.UShort0;
			ushort* ptr2 = &a.UShort0;
			ushort* ptr3 = &b.UShort0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[2 * i] = ptr2[i];
				ptr[2 * i + 1] = ptr3[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 unpacklo_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UInt0 = a.UInt0;
			result.UInt1 = b.UInt0;
			result.UInt2 = a.UInt1;
			result.UInt3 = b.UInt1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 unpacklo_epi64(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = a.ULong0;
			result.ULong1 = b.ULong0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 add_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 + b.Double0;
			result.Double1 = a.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 add_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 + b.Double0;
			result.Double1 = a.Double1 + b.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 div_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 / b.Double0;
			result.Double1 = a.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 div_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 / b.Double0;
			result.Double1 = a.Double1 / b.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 max_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = Math.Max(a.Double0, b.Double0);
			result.Double1 = a.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 max_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = Math.Max(a.Double0, b.Double0);
			result.Double1 = Math.Max(a.Double1, b.Double1);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 min_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = Math.Min(a.Double0, b.Double0);
			result.Double1 = a.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 min_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = Math.Min(a.Double0, b.Double0);
			result.Double1 = Math.Min(a.Double1, b.Double1);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 mul_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 * b.Double0;
			result.Double1 = a.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 mul_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 * b.Double0;
			result.Double1 = a.Double1 * b.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 sqrt_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = Math.Sqrt(b.Double0);
			result.Double1 = a.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 sqrt_pd(v128 a)
		{
			v128 result = default(v128);
			result.Double0 = Math.Sqrt(a.Double0);
			result.Double1 = Math.Sqrt(a.Double1);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 sub_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 - b.Double0;
			result.Double1 = a.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 sub_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 - b.Double0;
			result.Double1 = a.Double1 - b.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 and_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = a.ULong0 & b.ULong0;
			result.ULong1 = a.ULong1 & b.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 andnot_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = ~a.ULong0 & b.ULong0;
			result.ULong1 = ~a.ULong1 & b.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 or_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = a.ULong0 | b.ULong0;
			result.ULong1 = a.ULong1 | b.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 xor_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = a.ULong0 ^ b.ULong0;
			result.ULong1 = a.ULong1 ^ b.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpeq_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((a.Double0 == b.Double0) ? (-1) : 0);
			result.ULong1 = a.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmplt_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((a.Double0 < b.Double0) ? (-1) : 0);
			result.ULong1 = a.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmple_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((a.Double0 <= b.Double0) ? (-1) : 0);
			result.ULong1 = a.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpgt_sd(v128 a, v128 b)
		{
			return cmple_sd(b, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpge_sd(v128 a, v128 b)
		{
			return cmplt_sd(b, a);
		}

		[DebuggerStepThrough]
		public static v128 cmpord_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((IsNaN(a.ULong0) || IsNaN(b.ULong0)) ? 0 : (-1));
			result.ULong1 = a.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpunord_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((IsNaN(a.ULong0) || IsNaN(b.ULong0)) ? (-1) : 0);
			result.ULong1 = a.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpneq_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((a.Double0 != b.Double0) ? (-1) : 0);
			result.ULong1 = a.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpnlt_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((!(a.Double0 < b.Double0)) ? (-1) : 0);
			result.ULong1 = a.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpnle_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((!(a.Double0 <= b.Double0)) ? (-1) : 0);
			result.ULong1 = a.ULong1;
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpngt_sd(v128 a, v128 b)
		{
			return cmpnlt_sd(b, a);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 cmpnge_sd(v128 a, v128 b)
		{
			return cmpnle_sd(b, a);
		}

		[DebuggerStepThrough]
		public static v128 cmpeq_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((a.Double0 == b.Double0) ? (-1) : 0);
			result.ULong1 = (ulong)((a.Double1 == b.Double1) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmplt_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((a.Double0 < b.Double0) ? (-1) : 0);
			result.ULong1 = (ulong)((a.Double1 < b.Double1) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmple_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((a.Double0 <= b.Double0) ? (-1) : 0);
			result.ULong1 = (ulong)((a.Double1 <= b.Double1) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpgt_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((a.Double0 > b.Double0) ? (-1) : 0);
			result.ULong1 = (ulong)((a.Double1 > b.Double1) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpge_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((a.Double0 >= b.Double0) ? (-1) : 0);
			result.ULong1 = (ulong)((a.Double1 >= b.Double1) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpord_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((IsNaN(a.ULong0) || IsNaN(b.ULong0)) ? 0 : (-1));
			result.ULong1 = (ulong)((IsNaN(a.ULong1) || IsNaN(b.ULong1)) ? 0 : (-1));
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpunord_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((IsNaN(a.ULong0) || IsNaN(b.ULong0)) ? (-1) : 0);
			result.ULong1 = (ulong)((IsNaN(a.ULong1) || IsNaN(b.ULong1)) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpneq_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((a.Double0 != b.Double0) ? (-1) : 0);
			result.ULong1 = (ulong)((a.Double1 != b.Double1) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpnlt_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((!(a.Double0 < b.Double0)) ? (-1) : 0);
			result.ULong1 = (ulong)((!(a.Double1 < b.Double1)) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpnle_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((!(a.Double0 <= b.Double0)) ? (-1) : 0);
			result.ULong1 = (ulong)((!(a.Double1 <= b.Double1)) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpngt_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((!(a.Double0 > b.Double0)) ? (-1) : 0);
			result.ULong1 = (ulong)((!(a.Double1 > b.Double1)) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpnge_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.ULong0 = (ulong)((!(a.Double0 >= b.Double0)) ? (-1) : 0);
			result.ULong1 = (ulong)((!(a.Double1 >= b.Double1)) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static int comieq_sd(v128 a, v128 b)
		{
			if (a.Double0 != b.Double0)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int comilt_sd(v128 a, v128 b)
		{
			if (!(a.Double0 < b.Double0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int comile_sd(v128 a, v128 b)
		{
			if (!(a.Double0 <= b.Double0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int comigt_sd(v128 a, v128 b)
		{
			if (!(a.Double0 > b.Double0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int comige_sd(v128 a, v128 b)
		{
			if (!(a.Double0 >= b.Double0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int comineq_sd(v128 a, v128 b)
		{
			if (a.Double0 == b.Double0)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomieq_sd(v128 a, v128 b)
		{
			if (a.Double0 != b.Double0)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomilt_sd(v128 a, v128 b)
		{
			if (!(a.Double0 < b.Double0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomile_sd(v128 a, v128 b)
		{
			if (!(a.Double0 <= b.Double0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomigt_sd(v128 a, v128 b)
		{
			if (!(a.Double0 > b.Double0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomige_sd(v128 a, v128 b)
		{
			if (!(a.Double0 >= b.Double0))
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int ucomineq_sd(v128 a, v128 b)
		{
			if (a.Double0 == b.Double0)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static v128 cvtpd_ps(v128 a)
		{
			v128 result = default(v128);
			result.Float0 = (float)a.Double0;
			result.Float1 = (float)a.Double1;
			result.Float2 = 0f;
			result.Float3 = 0f;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cvtps_pd(v128 a)
		{
			v128 result = default(v128);
			result.Double0 = a.Float0;
			result.Double1 = a.Float1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cvtpd_epi32(v128 a)
		{
			v128 result = default(v128);
			result.SInt0 = (int)Math.Round(a.Double0);
			result.SInt1 = (int)Math.Round(a.Double1);
			return result;
		}

		[DebuggerStepThrough]
		public static int cvtsd_si32(v128 a)
		{
			return (int)Math.Round(a.Double0);
		}

		[DebuggerStepThrough]
		public static long cvtsd_si64(v128 a)
		{
			return (long)Math.Round(a.Double0);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static long cvtsd_si64x(v128 a)
		{
			return cvtsd_si64(a);
		}

		[DebuggerStepThrough]
		public static v128 cvtsd_ss(v128 a, v128 b)
		{
			v128 result = a;
			result.Float0 = (float)b.Double0;
			return result;
		}

		[DebuggerStepThrough]
		public static double cvtsd_f64(v128 a)
		{
			return a.Double0;
		}

		[DebuggerStepThrough]
		public static v128 cvtss_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = b.Float0;
			result.Double1 = a.Float0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cvttpd_epi32(v128 a)
		{
			v128 result = default(v128);
			result.SInt0 = (int)a.Double0;
			result.SInt1 = (int)a.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static int cvttsd_si32(v128 a)
		{
			return (int)a.Double0;
		}

		[DebuggerStepThrough]
		public static long cvttsd_si64(v128 a)
		{
			return (long)a.Double0;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static long cvttsd_si64x(v128 a)
		{
			return cvttsd_si64(a);
		}

		[DebuggerStepThrough]
		public static v128 cvtps_epi32(v128 a)
		{
			v128 result = default(v128);
			result.SInt0 = (int)Math.Round(a.Float0);
			result.SInt1 = (int)Math.Round(a.Float1);
			result.SInt2 = (int)Math.Round(a.Float2);
			result.SInt3 = (int)Math.Round(a.Float3);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cvttps_epi32(v128 a)
		{
			v128 result = default(v128);
			result.SInt0 = (int)a.Float0;
			result.SInt1 = (int)a.Float1;
			result.SInt2 = (int)a.Float2;
			result.SInt3 = (int)a.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 set_sd(double a)
		{
			v128 result = default(v128);
			result.Double0 = a;
			result.Double1 = 0.0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 set1_pd(double a)
		{
			v128 result = default(v128);
			result.Double0 = (result.Double1 = a);
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public static v128 set_pd1(double a)
		{
			return set1_pd(a);
		}

		[DebuggerStepThrough]
		public static v128 set_pd(double e1, double e0)
		{
			v128 result = default(v128);
			result.Double0 = e0;
			result.Double1 = e1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 setr_pd(double e1, double e0)
		{
			v128 result = default(v128);
			result.Double0 = e1;
			result.Double1 = e0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 unpackhi_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double1;
			result.Double1 = b.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 unpacklo_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0;
			result.Double1 = b.Double0;
			return result;
		}

		[DebuggerStepThrough]
		public static int movemask_pd(v128 a)
		{
			int num = 0;
			if ((a.ULong0 & 0x8000000000000000uL) != 0L)
			{
				num |= 1;
			}
			if ((a.ULong1 & 0x8000000000000000uL) != 0L)
			{
				num |= 2;
			}
			return num;
		}

		[DebuggerStepThrough]
		public unsafe static v128 shuffle_pd(v128 a, v128 b, int imm8)
		{
			v128 result = default(v128);
			double* ptr = &a.Double0;
			double* ptr2 = &b.Double0;
			result.Double0 = ptr[imm8 & 1];
			result.Double1 = ptr2[(imm8 >> 1) & 1];
			return result;
		}

		[DebuggerStepThrough]
		public static v128 move_sd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = b.Double0;
			result.Double1 = a.Double1;
			return result;
		}

		public unsafe static v128 loadu_si32(void* mem_addr)
		{
			return new v128(*(int*)mem_addr, 0, 0, 0);
		}

		public unsafe static void storeu_si32(void* mem_addr, v128 a)
		{
			*(int*)mem_addr = a.SInt0;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public unsafe static v128 load_si128(void* ptr)
		{
			return GenericCSharpLoad(ptr);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public unsafe static v128 loadu_si128(void* ptr)
		{
			return GenericCSharpLoad(ptr);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public unsafe static void store_si128(void* ptr, v128 val)
		{
			GenericCSharpStore(ptr, val);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		public unsafe static void storeu_si128(void* ptr, v128 val)
		{
			GenericCSharpStore(ptr, val);
		}

		[DebuggerStepThrough]
		public unsafe static void clflush(void* ptr)
		{
		}
	}

	public static class Sse3
	{
		public static bool IsSse3Supported => false;

		[DebuggerStepThrough]
		public static v128 addsub_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Float0 = a.Float0 - b.Float0;
			result.Float1 = a.Float1 + b.Float1;
			result.Float2 = a.Float2 - b.Float2;
			result.Float3 = a.Float3 + b.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 addsub_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 - b.Double0;
			result.Double1 = a.Double1 + b.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 hadd_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 + a.Double1;
			result.Double1 = b.Double0 + b.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 hadd_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Float0 = a.Float0 + a.Float1;
			result.Float1 = a.Float2 + a.Float3;
			result.Float2 = b.Float0 + b.Float1;
			result.Float3 = b.Float2 + b.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 hsub_pd(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0 - a.Double1;
			result.Double1 = b.Double0 - b.Double1;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 hsub_ps(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.Float0 = a.Float0 - a.Float1;
			result.Float1 = a.Float2 - a.Float3;
			result.Float2 = b.Float0 - b.Float1;
			result.Float3 = b.Float2 - b.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 movedup_pd(v128 a)
		{
			v128 result = default(v128);
			result.Double0 = a.Double0;
			result.Double1 = a.Double0;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 movehdup_ps(v128 a)
		{
			v128 result = default(v128);
			result.Float0 = a.Float1;
			result.Float1 = a.Float1;
			result.Float2 = a.Float3;
			result.Float3 = a.Float3;
			return result;
		}

		[DebuggerStepThrough]
		public static v128 moveldup_ps(v128 a)
		{
			v128 result = default(v128);
			result.Float0 = a.Float0;
			result.Float1 = a.Float0;
			result.Float2 = a.Float2;
			result.Float3 = a.Float2;
			return result;
		}
	}

	public static class Sse4_1
	{
		public static bool IsSse41Supported => false;

		[DebuggerStepThrough]
		public unsafe static v128 stream_load_si128(void* mem_addr)
		{
			return GenericCSharpLoad(mem_addr);
		}

		[DebuggerStepThrough]
		public unsafe static v128 blend_pd(v128 a, v128 b, int imm8)
		{
			v128 result = default(v128);
			double* ptr = &result.Double0;
			double* ptr2 = &a.Double0;
			double* ptr3 = &b.Double0;
			for (int i = 0; i <= 1; i++)
			{
				if ((imm8 & (1 << i)) != 0)
				{
					ptr[i] = ptr3[i];
				}
				else
				{
					ptr[i] = ptr2[i];
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 blend_ps(v128 a, v128 b, int imm8)
		{
			v128 result = default(v128);
			uint* ptr = &result.UInt0;
			uint* ptr2 = &a.UInt0;
			uint* ptr3 = &b.UInt0;
			for (int i = 0; i <= 3; i++)
			{
				if ((imm8 & (1 << i)) != 0)
				{
					ptr[i] = ptr3[i];
				}
				else
				{
					ptr[i] = ptr2[i];
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 blendv_pd(v128 a, v128 b, v128 mask)
		{
			v128 result = default(v128);
			double* ptr = &result.Double0;
			double* ptr2 = &a.Double0;
			double* ptr3 = &b.Double0;
			long* ptr4 = &mask.SLong0;
			for (int i = 0; i <= 1; i++)
			{
				if (ptr4[i] < 0)
				{
					ptr[i] = ptr3[i];
				}
				else
				{
					ptr[i] = ptr2[i];
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 blendv_ps(v128 a, v128 b, v128 mask)
		{
			v128 result = default(v128);
			uint* ptr = &result.UInt0;
			uint* ptr2 = &a.UInt0;
			uint* ptr3 = &b.UInt0;
			int* ptr4 = &mask.SInt0;
			for (int i = 0; i <= 3; i++)
			{
				if (ptr4[i] < 0)
				{
					ptr[i] = ptr3[i];
				}
				else
				{
					ptr[i] = ptr2[i];
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 blendv_epi8(v128 a, v128 b, v128 mask)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			byte* ptr3 = &b.Byte0;
			sbyte* ptr4 = &mask.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				if (ptr4[i] < 0)
				{
					ptr[i] = ptr3[i];
				}
				else
				{
					ptr[i] = ptr2[i];
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 blend_epi16(v128 a, v128 b, int imm8)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				if (((uint)(imm8 >> i) & (true ? 1u : 0u)) != 0)
				{
					ptr[i] = ptr3[i];
				}
				else
				{
					ptr[i] = ptr2[i];
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 dp_pd(v128 a, v128 b, int imm8)
		{
			double num = ((((uint)imm8 & 0x10u) != 0) ? (a.Double0 * b.Double0) : 0.0);
			double num2 = ((((uint)imm8 & 0x20u) != 0) ? (a.Double1 * b.Double1) : 0.0);
			double num3 = num + num2;
			v128 result = default(v128);
			result.Double0 = ((((uint)imm8 & (true ? 1u : 0u)) != 0) ? num3 : 0.0);
			result.Double1 = ((((uint)imm8 & 2u) != 0) ? num3 : 0.0);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 dp_ps(v128 a, v128 b, int imm8)
		{
			float num = ((((uint)imm8 & 0x10u) != 0) ? (a.Float0 * b.Float0) : 0f);
			float num2 = ((((uint)imm8 & 0x20u) != 0) ? (a.Float1 * b.Float1) : 0f);
			float num3 = ((((uint)imm8 & 0x40u) != 0) ? (a.Float2 * b.Float2) : 0f);
			float num4 = ((((uint)imm8 & 0x80u) != 0) ? (a.Float3 * b.Float3) : 0f);
			float num5 = num + num2 + num3 + num4;
			v128 result = default(v128);
			result.Float0 = ((((uint)imm8 & (true ? 1u : 0u)) != 0) ? num5 : 0f);
			result.Float1 = ((((uint)imm8 & 2u) != 0) ? num5 : 0f);
			result.Float2 = ((((uint)imm8 & 4u) != 0) ? num5 : 0f);
			result.Float3 = ((((uint)imm8 & 8u) != 0) ? num5 : 0f);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static int extract_ps(v128 a, int imm8)
		{
			return (&a.SInt0)[imm8 & 3];
		}

		[DebuggerStepThrough]
		public unsafe static float extractf_ps(v128 a, int imm8)
		{
			return (&a.Float0)[imm8 & 3];
		}

		[DebuggerStepThrough]
		public unsafe static byte extract_epi8(v128 a, int imm8)
		{
			return (&a.Byte0)[imm8 & 0xF];
		}

		[DebuggerStepThrough]
		public unsafe static int extract_epi32(v128 a, int imm8)
		{
			return (&a.SInt0)[imm8 & 3];
		}

		[DebuggerStepThrough]
		public unsafe static long extract_epi64(v128 a, int imm8)
		{
			return (&a.SLong0)[imm8 & 1];
		}

		[DebuggerStepThrough]
		public unsafe static v128 insert_ps(v128 a, v128 b, int imm8)
		{
			v128 result = a;
			(&result.Float0)[(imm8 >> 4) & 3] = (&b.Float0)[(imm8 >> 6) & 3];
			for (int i = 0; i < 4; i++)
			{
				if ((imm8 & (1 << i)) != 0)
				{
					(&result.Float0)[i] = 0f;
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 insert_epi8(v128 a, byte i, int imm8)
		{
			v128 result = a;
			(&result.Byte0)[imm8 & 0xF] = i;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 insert_epi32(v128 a, int i, int imm8)
		{
			v128 result = a;
			(&result.SInt0)[imm8 & 3] = i;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 insert_epi64(v128 a, long i, int imm8)
		{
			v128 result = a;
			(&result.SLong0)[imm8 & 1] = i;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 max_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			sbyte* ptr = &result.SByte0;
			sbyte* ptr2 = &a.SByte0;
			sbyte* ptr3 = &b.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = Math.Max(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 max_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			int* ptr = &result.SInt0;
			int* ptr2 = &a.SInt0;
			int* ptr3 = &b.SInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = Math.Max(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 max_epu32(v128 a, v128 b)
		{
			v128 result = default(v128);
			uint* ptr = &result.UInt0;
			uint* ptr2 = &a.UInt0;
			uint* ptr3 = &b.UInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = Math.Max(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 max_epu16(v128 a, v128 b)
		{
			v128 result = default(v128);
			ushort* ptr = &result.UShort0;
			ushort* ptr2 = &a.UShort0;
			ushort* ptr3 = &b.UShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = Math.Max(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 min_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			sbyte* ptr = &result.SByte0;
			sbyte* ptr2 = &a.SByte0;
			sbyte* ptr3 = &b.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = Math.Min(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 min_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			int* ptr = &result.SInt0;
			int* ptr2 = &a.SInt0;
			int* ptr3 = &b.SInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = Math.Min(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 min_epu32(v128 a, v128 b)
		{
			v128 result = default(v128);
			uint* ptr = &result.UInt0;
			uint* ptr2 = &a.UInt0;
			uint* ptr3 = &b.UInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = Math.Min(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 min_epu16(v128 a, v128 b)
		{
			v128 result = default(v128);
			ushort* ptr = &result.UShort0;
			ushort* ptr2 = &a.UShort0;
			ushort* ptr3 = &b.UShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = Math.Min(ptr2[i], ptr3[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 packus_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.UShort0 = Saturate_To_UnsignedInt16(a.SInt0);
			result.UShort1 = Saturate_To_UnsignedInt16(a.SInt1);
			result.UShort2 = Saturate_To_UnsignedInt16(a.SInt2);
			result.UShort3 = Saturate_To_UnsignedInt16(a.SInt3);
			result.UShort4 = Saturate_To_UnsignedInt16(b.SInt0);
			result.UShort5 = Saturate_To_UnsignedInt16(b.SInt1);
			result.UShort6 = Saturate_To_UnsignedInt16(b.SInt2);
			result.UShort7 = Saturate_To_UnsignedInt16(b.SInt3);
			return result;
		}

		[DebuggerStepThrough]
		public static v128 cmpeq_epi64(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.SLong0 = ((a.SLong0 == b.SLong0) ? (-1) : 0);
			result.SLong1 = ((a.SLong1 == b.SLong1) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepi8_epi16(v128 a)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			sbyte* ptr2 = &a.SByte0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepi8_epi32(v128 a)
		{
			v128 result = default(v128);
			int* ptr = &result.SInt0;
			sbyte* ptr2 = &a.SByte0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepi8_epi64(v128 a)
		{
			v128 result = default(v128);
			long* ptr = &result.SLong0;
			sbyte* ptr2 = &a.SByte0;
			for (int i = 0; i <= 1; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepi16_epi32(v128 a)
		{
			v128 result = default(v128);
			int* ptr = &result.SInt0;
			short* ptr2 = &a.SShort0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepi16_epi64(v128 a)
		{
			v128 result = default(v128);
			long* ptr = &result.SLong0;
			short* ptr2 = &a.SShort0;
			for (int i = 0; i <= 1; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepi32_epi64(v128 a)
		{
			v128 result = default(v128);
			long* ptr = &result.SLong0;
			int* ptr2 = &a.SInt0;
			for (int i = 0; i <= 1; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepu8_epi16(v128 a)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			byte* ptr2 = &a.Byte0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepu8_epi32(v128 a)
		{
			v128 result = default(v128);
			int* ptr = &result.SInt0;
			byte* ptr2 = &a.Byte0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepu8_epi64(v128 a)
		{
			v128 result = default(v128);
			long* ptr = &result.SLong0;
			byte* ptr2 = &a.Byte0;
			for (int i = 0; i <= 1; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepu16_epi32(v128 a)
		{
			v128 result = default(v128);
			int* ptr = &result.SInt0;
			ushort* ptr2 = &a.UShort0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepu16_epi64(v128 a)
		{
			v128 result = default(v128);
			long* ptr = &result.SLong0;
			ushort* ptr2 = &a.UShort0;
			for (int i = 0; i <= 1; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cvtepu32_epi64(v128 a)
		{
			v128 result = default(v128);
			long* ptr = &result.SLong0;
			uint* ptr2 = &a.UInt0;
			for (int i = 0; i <= 1; i++)
			{
				ptr[i] = ptr2[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 mul_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.SLong0 = (long)a.SInt0 * (long)b.SInt0;
			result.SLong1 = (long)a.SInt2 * (long)b.SInt2;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mullo_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			int* ptr = &result.SInt0;
			int* ptr2 = &a.SInt0;
			int* ptr3 = &b.SInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = ptr2[i] * ptr3[i];
			}
			return result;
		}

		[DebuggerStepThrough]
		public static int testz_si128(v128 a, v128 b)
		{
			if ((a.SLong0 & b.SLong0) != 0L || (a.SLong1 & b.SLong1) != 0L)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int testc_si128(v128 a, v128 b)
		{
			if ((~a.SLong0 & b.SLong0) != 0L || (~a.SLong1 & b.SLong1) != 0L)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int testnzc_si128(v128 a, v128 b)
		{
			int num = (((a.SLong0 & b.SLong0) == 0L && (a.SLong1 & b.SLong1) == 0L) ? 1 : 0);
			int num2 = (((~a.SLong0 & b.SLong0) == 0L && (~a.SLong1 & b.SLong1) == 0L) ? 1 : 0);
			return 1 - (num | num2);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static int test_all_zeros(v128 a, v128 mask)
		{
			return testz_si128(a, mask);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static int test_mix_ones_zeroes(v128 a, v128 mask)
		{
			return testnzc_si128(a, mask);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static int test_all_ones(v128 a)
		{
			return testc_si128(a, Sse2.cmpeq_epi32(a, a));
		}

		private static double RoundDImpl(double d, int roundingMode)
		{
			switch (roundingMode & 7)
			{
			case 0:
				return Math.Round(d);
			case 1:
				return Math.Floor(d);
			case 2:
			{
				double num = Math.Ceiling(d);
				if (num == 0.0 && d < 0.0)
				{
					return new v128(9223372036854775808uL).Double0;
				}
				return num;
			}
			case 3:
				return Math.Truncate(d);
			default:
				return (MXCSR & MXCSRBits.RoundingControlMask) switch
				{
					MXCSRBits.RoundToNearest => Math.Round(d), 
					MXCSRBits.RoundDown => Math.Floor(d), 
					MXCSRBits.RoundUp => Math.Ceiling(d), 
					_ => Math.Truncate(d), 
				};
			}
		}

		[DebuggerStepThrough]
		public static v128 round_pd(v128 a, int rounding)
		{
			v128 result = default(v128);
			result.Double0 = RoundDImpl(a.Double0, rounding);
			result.Double1 = RoundDImpl(a.Double1, rounding);
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static v128 floor_pd(v128 a)
		{
			return round_pd(a, 1);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static v128 ceil_pd(v128 a)
		{
			return round_pd(a, 2);
		}

		[DebuggerStepThrough]
		public static v128 round_ps(v128 a, int rounding)
		{
			v128 result = default(v128);
			result.Float0 = (float)RoundDImpl(a.Float0, rounding);
			result.Float1 = (float)RoundDImpl(a.Float1, rounding);
			result.Float2 = (float)RoundDImpl(a.Float2, rounding);
			result.Float3 = (float)RoundDImpl(a.Float3, rounding);
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static v128 floor_ps(v128 a)
		{
			return round_ps(a, 1);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static v128 ceil_ps(v128 a)
		{
			return round_ps(a, 2);
		}

		[DebuggerStepThrough]
		public static v128 round_sd(v128 a, v128 b, int rounding)
		{
			v128 result = default(v128);
			result.Double0 = RoundDImpl(b.Double0, rounding);
			result.Double1 = a.Double1;
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static v128 floor_sd(v128 a, v128 b)
		{
			return round_sd(a, b, 1);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static v128 ceil_sd(v128 a, v128 b)
		{
			return round_sd(a, b, 2);
		}

		[DebuggerStepThrough]
		public static v128 round_ss(v128 a, v128 b, int rounding)
		{
			v128 result = a;
			result.Float0 = (float)RoundDImpl(b.Float0, rounding);
			return result;
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static v128 floor_ss(v128 a, v128 b)
		{
			return round_ss(a, b, 1);
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.X64_SSE4)]
		public static v128 ceil_ss(v128 a, v128 b)
		{
			return round_ss(a, b, 2);
		}

		[DebuggerStepThrough]
		public unsafe static v128 minpos_epu16(v128 a)
		{
			int num = 0;
			ushort num2 = a.UShort0;
			ushort* ptr = &a.UShort0;
			for (int i = 1; i <= 7; i++)
			{
				if (ptr[i] < num2)
				{
					num = i;
					num2 = ptr[i];
				}
			}
			v128 result = default(v128);
			result.UShort0 = num2;
			result.UShort1 = (ushort)num;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mpsadbw_epu8(v128 a, v128 b, int imm8)
		{
			v128 result = default(v128);
			ushort* ptr = &result.UShort0;
			byte* ptr2 = &a.Byte0 + ((imm8 >> 2) & 1) * 4;
			byte* num = &b.Byte0 + (imm8 & 3) * 4;
			byte b2 = *num;
			byte b3 = num[1];
			byte b4 = num[2];
			byte b5 = num[3];
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = (ushort)(Math.Abs(ptr2[i] - b2) + Math.Abs(ptr2[i + 1] - b3) + Math.Abs(ptr2[i + 2] - b4) + Math.Abs(ptr2[i + 3] - b5));
			}
			return result;
		}

		[DebuggerStepThrough]
		public static int MK_INSERTPS_NDX(int srcField, int dstField, int zeroMask)
		{
			return (srcField << 6) | (dstField << 4) | zeroMask;
		}
	}

	public static class Sse4_2
	{
		[Flags]
		public enum SIDD
		{
			UBYTE_OPS = 0,
			UWORD_OPS = 1,
			SBYTE_OPS = 2,
			SWORD_OPS = 3,
			CMP_EQUAL_ANY = 0,
			CMP_RANGES = 4,
			CMP_EQUAL_EACH = 8,
			CMP_EQUAL_ORDERED = 0xC,
			POSITIVE_POLARITY = 0,
			NEGATIVE_POLARITY = 0x10,
			MASKED_POSITIVE_POLARITY = 0x20,
			MASKED_NEGATIVE_POLARITY = 0x30,
			LEAST_SIGNIFICANT = 0,
			MOST_SIGNIFICANT = 0x40,
			BIT_MASK = 0,
			UNIT_MASK = 0x40
		}

		private struct StrBoolArray
		{
			public unsafe fixed ushort Bits[16];

			public unsafe void SetBit(int aindex, int bindex, bool val)
			{
				fixed (ushort* ptr = Bits)
				{
					if (val)
					{
						ushort* num = ptr + aindex;
						*num |= (ushort)(1 << bindex);
					}
					else
					{
						ushort* num2 = ptr + aindex;
						*num2 &= (ushort)(~(1 << bindex));
					}
				}
			}

			public unsafe bool GetBit(int aindex, int bindex)
			{
				fixed (ushort* ptr = Bits)
				{
					return (ptr[aindex] & (1 << bindex)) != 0;
				}
			}
		}

		private static readonly uint[] crctab = new uint[256]
		{
			0u, 4067132163u, 3778769143u, 324072436u, 3348797215u, 904991772u, 648144872u, 3570033899u, 2329499855u, 2024987596u,
			1809983544u, 2575936315u, 1296289744u, 3207089363u, 2893594407u, 1578318884u, 274646895u, 3795141740u, 4049975192u, 51262619u,
			3619967088u, 632279923u, 922689671u, 3298075524u, 2592579488u, 1760304291u, 2075979607u, 2312596564u, 1562183871u, 2943781820u,
			3156637768u, 1313733451u, 549293790u, 3537243613u, 3246849577u, 871202090u, 3878099393u, 357341890u, 102525238u, 4101499445u,
			2858735121u, 1477399826u, 1264559846u, 3107202533u, 1845379342u, 2677391885u, 2361733625u, 2125378298u, 820201905u, 3263744690u,
			3520608582u, 598981189u, 4151959214u, 85089709u, 373468761u, 3827903834u, 3124367742u, 1213305469u, 1526817161u, 2842354314u,
			2107672161u, 2412447074u, 2627466902u, 1861252501u, 1098587580u, 3004210879u, 2688576843u, 1378610760u, 2262928035u, 1955203488u,
			1742404180u, 2511436119u, 3416409459u, 969524848u, 714683780u, 3639785095u, 205050476u, 4266873199u, 3976438427u, 526918040u,
			1361435347u, 2739821008u, 2954799652u, 1114974503u, 2529119692u, 1691668175u, 2005155131u, 2247081528u, 3690758684u, 697762079u,
			986182379u, 3366744552u, 476452099u, 3993867776u, 4250756596u, 255256311u, 1640403810u, 2477592673u, 2164122517u, 1922457750u,
			2791048317u, 1412925310u, 1197962378u, 3037525897u, 3944729517u, 427051182u, 170179418u, 4165941337u, 746937522u, 3740196785u,
			3451792453u, 1070968646u, 1905808397u, 2213795598u, 2426610938u, 1657317369u, 3053634322u, 1147748369u, 1463399397u, 2773627110u,
			4215344322u, 153784257u, 444234805u, 3893493558u, 1021025245u, 3467647198u, 3722505002u, 797665321u, 2197175160u, 1889384571u,
			1674398607u, 2443626636u, 1164749927u, 3070701412u, 2757221520u, 1446797203u, 137323447u, 4198817972u, 3910406976u, 461344835u,
			3484808360u, 1037989803u, 781091935u, 3705997148u, 2460548119u, 1623424788u, 1939049696u, 2180517859u, 1429367560u, 2807687179u,
			3020495871u, 1180866812u, 410100952u, 3927582683u, 4182430767u, 186734380u, 3756733383u, 763408580u, 1053836080u, 3434856499u,
			2722870694u, 1344288421u, 1131464017u, 2971354706u, 1708204729u, 2545590714u, 2229949006u, 1988219213u, 680717673u, 3673779818u,
			3383336350u, 1002577565u, 4010310262u, 493091189u, 238226049u, 4233660802u, 2987750089u, 1082061258u, 1395524158u, 2705686845u,
			1972364758u, 2279892693u, 2494862625u, 1725896226u, 952904198u, 3399985413u, 3656866545u, 731699698u, 4283874585u, 222117402u,
			510512622u, 3959836397u, 3280807620u, 837199303u, 582374963u, 3504198960u, 68661723u, 4135334616u, 3844915500u, 390545967u,
			1230274059u, 3141532936u, 2825850620u, 1510247935u, 2395924756u, 2091215383u, 1878366691u, 2644384480u, 3553878443u, 565732008u,
			854102364u, 3229815391u, 340358836u, 3861050807u, 4117890627u, 119113024u, 1493875044u, 2875275879u, 3090270611u, 1247431312u,
			2660249211u, 1828433272u, 2141937292u, 2378227087u, 3811616794u, 291187481u, 34330861u, 4032846830u, 615137029u, 3603020806u,
			3314634738u, 939183345u, 1776939221u, 2609017814u, 2295496738u, 2058945313u, 2926798794u, 1545135305u, 1330124605u, 3173225534u,
			4084100981u, 17165430u, 307568514u, 3762199681u, 888469610u, 3332340585u, 3587147933u, 665062302u, 2042050490u, 2346497209u,
			2559330125u, 1793573966u, 3190661285u, 1279665062u, 1595330642u, 2910671697u
		};

		public static bool IsSse42Supported => false;

		private unsafe static v128 cmpistrm_emulation<T>(T* a, T* b, int len, int imm8, int allOnes, T allOnesT) where T : unmanaged, IComparable<T>, IEquatable<T>
		{
			int intRes = ComputeStrCmpIntRes2(a, ComputeStringLength(a, len), b, ComputeStringLength(b, len), len, imm8, allOnes);
			return ComputeStrmOutput(len, imm8, allOnesT, intRes);
		}

		private unsafe static v128 cmpestrm_emulation<T>(T* a, int alen, T* b, int blen, int len, int imm8, int allOnes, T allOnesT) where T : unmanaged, IComparable<T>, IEquatable<T>
		{
			int intRes = ComputeStrCmpIntRes2(a, alen, b, blen, len, imm8, allOnes);
			return ComputeStrmOutput(len, imm8, allOnesT, intRes);
		}

		private unsafe static v128 ComputeStrmOutput<T>(int len, int imm8, T allOnesT, int intRes2) where T : unmanaged, IComparable<T>, IEquatable<T>
		{
			v128 result = default(v128);
			if (((uint)imm8 & 0x40u) != 0)
			{
				T* ptr = (T*)(&result.Byte0);
				for (int i = 0; i < len; i++)
				{
					if ((intRes2 & (1 << i)) != 0)
					{
						ptr[i] = allOnesT;
					}
					else
					{
						ptr[i] = default(T);
					}
				}
			}
			else
			{
				result.SInt0 = intRes2;
			}
			return result;
		}

		private unsafe static int cmpistri_emulation<T>(T* a, T* b, int len, int imm8, int allOnes, T allOnesT) where T : unmanaged, IComparable<T>, IEquatable<T>
		{
			int intRes = ComputeStrCmpIntRes2(a, ComputeStringLength(a, len), b, ComputeStringLength(b, len), len, imm8, allOnes);
			return ComputeStriOutput(len, imm8, intRes);
		}

		private unsafe static int cmpestri_emulation<T>(T* a, int alen, T* b, int blen, int len, int imm8, int allOnes, T allOnesT) where T : unmanaged, IComparable<T>, IEquatable<T>
		{
			int intRes = ComputeStrCmpIntRes2(a, alen, b, blen, len, imm8, allOnes);
			return ComputeStriOutput(len, imm8, intRes);
		}

		private static int ComputeStriOutput(int len, int imm8, int intRes2)
		{
			if ((imm8 & 0x40) == 0)
			{
				for (int i = 0; i < len; i++)
				{
					if ((intRes2 & (1 << i)) != 0)
					{
						return i;
					}
				}
			}
			else
			{
				for (int num = len - 1; num >= 0; num--)
				{
					if ((intRes2 & (1 << num)) != 0)
					{
						return num;
					}
				}
			}
			return len;
		}

		private unsafe static int ComputeStringLength<T>(T* ptr, int max) where T : unmanaged, IEquatable<T>
		{
			for (int i = 0; i < max; i++)
			{
				if (EqualityComparer<T>.Default.Equals(ptr[i], default(T)))
				{
					return i;
				}
			}
			return max;
		}

		private unsafe static int ComputeStrCmpIntRes2<T>(T* a, int alen, T* b, int blen, int len, int imm8, int allOnes) where T : unmanaged, IComparable<T>, IEquatable<T>
		{
			bool flag = false;
			bool flag2 = false;
			StrBoolArray strBoolArray = default(StrBoolArray);
			for (int i = 0; i < len; i++)
			{
				T val = a[i];
				if (i == alen)
				{
					flag = true;
				}
				flag2 = false;
				for (int j = 0; j < len; j++)
				{
					T val2 = b[j];
					if (j == blen)
					{
						flag2 = true;
					}
					bool val3;
					switch ((imm8 >> 2) & 3)
					{
					case 0:
						val3 = EqualityComparer<T>.Default.Equals(val, val2);
						if (!flag && flag2)
						{
							val3 = false;
						}
						else if (flag && !flag2)
						{
							val3 = false;
						}
						else if (flag && flag2)
						{
							val3 = false;
						}
						break;
					case 1:
						val3 = ((((uint)i & (true ? 1u : 0u)) != 0) ? (Comparer<T>.Default.Compare(val2, val) <= 0) : (Comparer<T>.Default.Compare(val2, val) >= 0));
						if (!flag && flag2)
						{
							val3 = false;
						}
						else if (flag && !flag2)
						{
							val3 = false;
						}
						else if (flag && flag2)
						{
							val3 = false;
						}
						break;
					case 2:
						val3 = EqualityComparer<T>.Default.Equals(val, val2);
						if (!flag && flag2)
						{
							val3 = false;
						}
						else if (flag && !flag2)
						{
							val3 = false;
						}
						else if (flag && flag2)
						{
							val3 = true;
						}
						break;
					default:
						val3 = EqualityComparer<T>.Default.Equals(val, val2);
						if (!flag && flag2)
						{
							val3 = false;
						}
						else if (flag && !flag2)
						{
							val3 = true;
						}
						else if (flag && flag2)
						{
							val3 = true;
						}
						break;
					}
					strBoolArray.SetBit(i, j, val3);
				}
			}
			int num = 0;
			switch ((imm8 >> 2) & 3)
			{
			case 0:
			{
				for (int i = 0; i < len; i++)
				{
					for (int j = 0; j < len; j++)
					{
						num |= (strBoolArray.GetBit(j, i) ? 1 : 0) << i;
					}
				}
				break;
			}
			case 1:
			{
				for (int i = 0; i < len; i++)
				{
					for (int j = 0; j < len; j += 2)
					{
						num |= ((strBoolArray.GetBit(j, i) && strBoolArray.GetBit(j + 1, i)) ? 1 : 0) << i;
					}
				}
				break;
			}
			case 2:
			{
				for (int i = 0; i < len; i++)
				{
					num |= (strBoolArray.GetBit(i, i) ? 1 : 0) << i;
				}
				break;
			}
			case 3:
			{
				num = allOnes;
				for (int i = 0; i < len; i++)
				{
					int num2 = i;
					for (int j = 0; j < len - i; j++)
					{
						if (!strBoolArray.GetBit(j, num2))
						{
							num &= ~(1 << i);
						}
						num2++;
					}
				}
				break;
			}
			}
			int num3 = 0;
			flag2 = false;
			for (int i = 0; i < len; i++)
			{
				if (((uint)imm8 & 0x10u) != 0)
				{
					if (((uint)imm8 & 0x20u) != 0)
					{
						if (EqualityComparer<T>.Default.Equals(b[i], default(T)))
						{
							flag2 = true;
						}
						num3 = ((!flag2) ? (num3 | (~num & (1 << i))) : (num3 | (num & (1 << i))));
					}
					else
					{
						num3 |= ~num & (1 << i);
					}
				}
				else
				{
					num3 |= num & (1 << i);
				}
			}
			return num3;
		}

		[DebuggerStepThrough]
		public unsafe static v128 cmpistrm(v128 a, v128 b, int imm8)
		{
			if ((imm8 & 1) == 0)
			{
				if ((imm8 & 2) == 0)
				{
					return cmpistrm_emulation(&a.Byte0, &b.Byte0, 16, imm8, 65535, byte.MaxValue);
				}
				return cmpistrm_emulation<sbyte>(&a.SByte0, &b.SByte0, 16, imm8, 65535, -1);
			}
			if ((imm8 & 2) == 0)
			{
				return cmpistrm_emulation(&a.UShort0, &b.UShort0, 8, imm8, 255, ushort.MaxValue);
			}
			return cmpistrm_emulation<short>(&a.SShort0, &b.SShort0, 8, imm8, 255, -1);
		}

		[DebuggerStepThrough]
		public unsafe static int cmpistri(v128 a, v128 b, int imm8)
		{
			if ((imm8 & 1) == 0)
			{
				if ((imm8 & 2) == 0)
				{
					return cmpistri_emulation(&a.Byte0, &b.Byte0, 16, imm8, 65535, byte.MaxValue);
				}
				return cmpistri_emulation<sbyte>(&a.SByte0, &b.SByte0, 16, imm8, 65535, -1);
			}
			if ((imm8 & 2) == 0)
			{
				return cmpistri_emulation(&a.UShort0, &b.UShort0, 8, imm8, 255, ushort.MaxValue);
			}
			return cmpistri_emulation<short>(&a.SShort0, &b.SShort0, 8, imm8, 255, -1);
		}

		[DebuggerStepThrough]
		public unsafe static v128 cmpestrm(v128 a, int la, v128 b, int lb, int imm8)
		{
			if ((imm8 & 1) == 0)
			{
				if ((imm8 & 2) == 0)
				{
					return cmpestrm_emulation(&a.Byte0, la, &b.Byte0, lb, 16, imm8, 65535, byte.MaxValue);
				}
				return cmpestrm_emulation<sbyte>(&a.SByte0, la, &b.SByte0, lb, 16, imm8, 65535, -1);
			}
			if ((imm8 & 2) == 0)
			{
				return cmpestrm_emulation(&a.UShort0, la, &b.UShort0, lb, 8, imm8, 255, ushort.MaxValue);
			}
			return cmpestrm_emulation<short>(&a.SShort0, la, &b.SShort0, lb, 8, imm8, 255, -1);
		}

		[DebuggerStepThrough]
		public unsafe static int cmpestri(v128 a, int la, v128 b, int lb, int imm8)
		{
			if ((imm8 & 1) == 0)
			{
				if ((imm8 & 2) == 0)
				{
					return cmpestri_emulation(&a.Byte0, la, &b.Byte0, lb, 16, imm8, 65535, byte.MaxValue);
				}
				return cmpestri_emulation<sbyte>(&a.SByte0, la, &b.SByte0, lb, 16, imm8, 65535, -1);
			}
			if ((imm8 & 2) == 0)
			{
				return cmpestri_emulation(&a.UShort0, la, &b.UShort0, lb, 8, imm8, 255, ushort.MaxValue);
			}
			return cmpestri_emulation<short>(&a.SShort0, la, &b.SShort0, lb, 8, imm8, 255, -1);
		}

		[DebuggerStepThrough]
		public unsafe static int cmpistrz(v128 a, v128 b, int imm8)
		{
			if ((imm8 & 1) == 0)
			{
				if (ComputeStringLength(&b.Byte0, 16) >= 16)
				{
					return 0;
				}
				return 1;
			}
			if (ComputeStringLength(&b.UShort0, 8) >= 8)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int cmpistrc(v128 a, v128 b, int imm8)
		{
			v128 v257 = cmpistrm(a, b, imm8);
			if (v257.SInt0 != 0 || v257.SInt1 != 0 || v257.SInt2 != 0 || v257.SInt3 != 0)
			{
				return 1;
			}
			return 0;
		}

		[DebuggerStepThrough]
		public unsafe static int cmpistrs(v128 a, v128 b, int imm8)
		{
			if ((imm8 & 1) == 0)
			{
				if (ComputeStringLength(&a.Byte0, 16) >= 16)
				{
					return 0;
				}
				return 1;
			}
			if (ComputeStringLength(&a.UShort0, 8) >= 8)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public unsafe static int cmpistro(v128 a, v128 b, int imm8)
		{
			int num;
			if ((imm8 & 1) == 0)
			{
				int alen = ComputeStringLength(&a.Byte0, 16);
				int blen = ComputeStringLength(&b.Byte0, 16);
				num = ((((uint)imm8 & 2u) != 0) ? ComputeStrCmpIntRes2(&a.SByte0, alen, &b.SByte0, blen, 16, imm8, 65535) : ComputeStrCmpIntRes2(&a.Byte0, alen, &b.Byte0, blen, 16, imm8, 65535));
			}
			else
			{
				int alen2 = ComputeStringLength(&a.UShort0, 8);
				int blen2 = ComputeStringLength(&b.UShort0, 8);
				num = ((((uint)imm8 & 2u) != 0) ? ComputeStrCmpIntRes2(&a.SShort0, alen2, &b.SShort0, blen2, 8, imm8, 255) : ComputeStrCmpIntRes2(&a.UShort0, alen2, &b.UShort0, blen2, 8, imm8, 255));
			}
			return num & 1;
		}

		[DebuggerStepThrough]
		public static int cmpistra(v128 a, v128 b, int imm8)
		{
			return ~cmpistrc(a, b, imm8) & ~cmpistrz(a, b, imm8) & 1;
		}

		[DebuggerStepThrough]
		public static int cmpestrz(v128 a, int la, v128 b, int lb, int imm8)
		{
			int num = (((imm8 & 1) == 1) ? 16 : 8);
			int num2 = 128 / num - 1;
			if (lb > num2)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public unsafe static int cmpestrc(v128 a, int la, v128 b, int lb, int imm8)
		{
			if ((((imm8 & 1) == 0) ? ((((uint)imm8 & 2u) != 0) ? ComputeStrCmpIntRes2(&a.SByte0, la, &b.SByte0, lb, 16, imm8, 65535) : ComputeStrCmpIntRes2(&a.Byte0, la, &b.Byte0, lb, 16, imm8, 65535)) : ((((uint)imm8 & 2u) != 0) ? ComputeStrCmpIntRes2(&a.SShort0, la, &b.SShort0, lb, 8, imm8, 255) : ComputeStrCmpIntRes2(&a.UShort0, la, &b.UShort0, lb, 8, imm8, 255))) == 0)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public static int cmpestrs(v128 a, int la, v128 b, int lb, int imm8)
		{
			int num = (((imm8 & 1) == 1) ? 16 : 8);
			int num2 = 128 / num - 1;
			if (la > num2)
			{
				return 0;
			}
			return 1;
		}

		[DebuggerStepThrough]
		public unsafe static int cmpestro(v128 a, int la, v128 b, int lb, int imm8)
		{
			int num = (((imm8 & 1) == 0) ? ((((uint)imm8 & 2u) != 0) ? ComputeStrCmpIntRes2(&a.SByte0, la, &b.SByte0, lb, 16, imm8, 65535) : ComputeStrCmpIntRes2(&a.Byte0, la, &b.Byte0, lb, 16, imm8, 65535)) : ((((uint)imm8 & 2u) != 0) ? ComputeStrCmpIntRes2(&a.SShort0, la, &b.SShort0, lb, 8, imm8, 255) : ComputeStrCmpIntRes2(&a.UShort0, la, &b.UShort0, lb, 8, imm8, 255)));
			return num & 1;
		}

		[DebuggerStepThrough]
		public static int cmpestra(v128 a, int la, v128 b, int lb, int imm8)
		{
			return ~cmpestrc(a, la, b, lb, imm8) & ~cmpestrz(a, la, b, lb, imm8) & 1;
		}

		[DebuggerStepThrough]
		public static v128 cmpgt_epi64(v128 val1, v128 val2)
		{
			v128 result = default(v128);
			result.SLong0 = ((val1.SLong0 > val2.SLong0) ? (-1) : 0);
			result.SLong1 = ((val1.SLong1 > val2.SLong1) ? (-1) : 0);
			return result;
		}

		[DebuggerStepThrough]
		public static uint crc32_u32(uint crc, uint v)
		{
			crc = crc32_u8(crc, (byte)v);
			v >>= 8;
			crc = crc32_u8(crc, (byte)v);
			v >>= 8;
			crc = crc32_u8(crc, (byte)v);
			v >>= 8;
			crc = crc32_u8(crc, (byte)v);
			return crc;
		}

		[DebuggerStepThrough]
		public static uint crc32_u8(uint crc, byte v)
		{
			crc = (crc >> 8) ^ crctab[(crc ^ v) & 0xFF];
			return crc;
		}

		[DebuggerStepThrough]
		public static uint crc32_u16(uint crc, ushort v)
		{
			crc = crc32_u8(crc, (byte)v);
			v >>= 8;
			crc = crc32_u8(crc, (byte)v);
			return crc;
		}

		[DebuggerStepThrough]
		[Obsolete("Use the ulong version of this intrinsic instead.")]
		public static ulong crc32_u64(ulong crc_ul, long v)
		{
			return crc32_u64(crc_ul, (ulong)v);
		}

		[DebuggerStepThrough]
		public static ulong crc32_u64(ulong crc_ul, ulong v)
		{
			uint crc = crc32_u8((uint)crc_ul, (byte)v);
			v >>= 8;
			uint crc2 = crc32_u8(crc, (byte)v);
			v >>= 8;
			uint crc3 = crc32_u8(crc2, (byte)v);
			v >>= 8;
			uint crc4 = crc32_u8(crc3, (byte)v);
			v >>= 8;
			uint crc5 = crc32_u8(crc4, (byte)v);
			v >>= 8;
			uint crc6 = crc32_u8(crc5, (byte)v);
			v >>= 8;
			uint crc7 = crc32_u8(crc6, (byte)v);
			v >>= 8;
			return crc32_u8(crc7, (byte)v);
		}
	}

	public static class Ssse3
	{
		public static bool IsSsse3Supported => false;

		[DebuggerStepThrough]
		public unsafe static v128 abs_epi8(v128 a)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			sbyte* ptr2 = &a.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				ptr[i] = (byte)Math.Abs((int)ptr2[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 abs_epi16(v128 a)
		{
			v128 result = default(v128);
			ushort* ptr = &result.UShort0;
			short* ptr2 = &a.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				ptr[i] = (ushort)Math.Abs((int)ptr2[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 abs_epi32(v128 a)
		{
			v128 result = default(v128);
			uint* ptr = &result.UInt0;
			int* ptr2 = &a.SInt0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = (uint)Math.Abs((long)ptr2[i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 shuffle_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0;
			byte* ptr3 = &b.Byte0;
			for (int i = 0; i <= 15; i++)
			{
				if ((ptr3[i] & 0x80u) != 0)
				{
					ptr[i] = 0;
				}
				else
				{
					ptr[i] = ptr2[ptr3[i] & 0xF];
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 alignr_epi8(v128 a, v128 b, int count)
		{
			v128 result = default(v128);
			byte* ptr = &result.Byte0;
			byte* ptr2 = &a.Byte0 + count;
			byte* ptr3 = &b.Byte0;
			int i;
			for (i = 0; i < 16 - count; i++)
			{
				*(ptr++) = *(ptr2++);
			}
			for (; i < 16; i++)
			{
				*(ptr++) = *(ptr3++);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 hadd_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = (short)(ptr2[2 * i + 1] + ptr2[2 * i]);
				ptr[i + 4] = (short)(ptr3[2 * i + 1] + ptr3[2 * i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 hadds_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = Saturate_To_Int16(ptr2[2 * i + 1] + ptr2[2 * i]);
				ptr[i + 4] = Saturate_To_Int16(ptr3[2 * i + 1] + ptr3[2 * i]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 hadd_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.SInt0 = a.SInt1 + a.SInt0;
			result.SInt1 = a.SInt3 + a.SInt2;
			result.SInt2 = b.SInt1 + b.SInt0;
			result.SInt3 = b.SInt3 + b.SInt2;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 hsub_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = (short)(ptr2[2 * i] - ptr2[2 * i + 1]);
				ptr[i + 4] = (short)(ptr3[2 * i] - ptr3[2 * i + 1]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 hsubs_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 3; i++)
			{
				ptr[i] = Saturate_To_Int16(ptr2[2 * i] - ptr2[2 * i + 1]);
				ptr[i + 4] = Saturate_To_Int16(ptr3[2 * i] - ptr3[2 * i + 1]);
			}
			return result;
		}

		[DebuggerStepThrough]
		public static v128 hsub_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			result.SInt0 = a.SInt0 - a.SInt1;
			result.SInt1 = a.SInt2 - a.SInt3;
			result.SInt2 = b.SInt0 - b.SInt1;
			result.SInt3 = b.SInt2 - b.SInt3;
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 maddubs_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			byte* ptr2 = &a.Byte0;
			sbyte* ptr3 = &b.SByte0;
			for (int i = 0; i <= 7; i++)
			{
				int val = ptr2[2 * i + 1] * ptr3[2 * i + 1] + ptr2[2 * i] * ptr3[2 * i];
				ptr[i] = Saturate_To_Int16(val);
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 mulhrs_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				int num = ptr2[i] * ptr3[i];
				num >>= 14;
				num++;
				num >>= 1;
				ptr[i] = (short)num;
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sign_epi8(v128 a, v128 b)
		{
			v128 result = default(v128);
			sbyte* ptr = &result.SByte0;
			sbyte* ptr2 = &a.SByte0;
			sbyte* ptr3 = &b.SByte0;
			for (int i = 0; i <= 15; i++)
			{
				if (ptr3[i] < 0)
				{
					ptr[i] = (sbyte)(-ptr2[i]);
				}
				else if (ptr3[i] == 0)
				{
					ptr[i] = 0;
				}
				else
				{
					ptr[i] = ptr2[i];
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sign_epi16(v128 a, v128 b)
		{
			v128 result = default(v128);
			short* ptr = &result.SShort0;
			short* ptr2 = &a.SShort0;
			short* ptr3 = &b.SShort0;
			for (int i = 0; i <= 7; i++)
			{
				if (ptr3[i] < 0)
				{
					ptr[i] = (short)(-ptr2[i]);
				}
				else if (ptr3[i] == 0)
				{
					ptr[i] = 0;
				}
				else
				{
					ptr[i] = ptr2[i];
				}
			}
			return result;
		}

		[DebuggerStepThrough]
		public unsafe static v128 sign_epi32(v128 a, v128 b)
		{
			v128 result = default(v128);
			int* ptr = &result.SInt0;
			int* ptr2 = &a.SInt0;
			int* ptr3 = &b.SInt0;
			for (int i = 0; i <= 3; i++)
			{
				if (ptr3[i] < 0)
				{
					ptr[i] = -ptr2[i];
				}
				else if (ptr3[i] == 0)
				{
					ptr[i] = 0;
				}
				else
				{
					ptr[i] = ptr2[i];
				}
			}
			return result;
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void DoSetCSRTrampoline_00000129_0024PostfixBurstDelegate(int bits);

	internal static class DoSetCSRTrampoline_00000129_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<DoSetCSRTrampoline_00000129_0024PostfixBurstDelegate>(DoSetCSRTrampoline).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(int bits)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<int, void>)functionPointer)(bits);
					return;
				}
			}
			DoSetCSRTrampoline_0024BurstManaged(bits);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int DoGetCSRTrampoline_0000012A_0024PostfixBurstDelegate();

	internal static class DoGetCSRTrampoline_0000012A_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<DoGetCSRTrampoline_0000012A_0024PostfixBurstDelegate>(DoGetCSRTrampoline).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static int Invoke()
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<int>)functionPointer)();
				}
			}
			return DoGetCSRTrampoline_0024BurstManaged();
		}
	}

	public static MXCSRBits MXCSR
	{
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		get
		{
			return (MXCSRBits)getcsr_raw();
		}
		[BurstTargetCpu(BurstTargetCpu.X64_SSE2)]
		set
		{
			setcsr_raw((int)value);
		}
	}

	private unsafe static v128 GenericCSharpLoad(void* ptr)
	{
		return *(v128*)ptr;
	}

	private unsafe static void GenericCSharpStore(void* ptr, v128 val)
	{
		*(v128*)ptr = val;
	}

	private static sbyte Saturate_To_Int8(int val)
	{
		if (val > 127)
		{
			return sbyte.MaxValue;
		}
		if (val < -128)
		{
			return sbyte.MinValue;
		}
		return (sbyte)val;
	}

	private static byte Saturate_To_UnsignedInt8(int val)
	{
		if (val > 255)
		{
			return byte.MaxValue;
		}
		if (val < 0)
		{
			return 0;
		}
		return (byte)val;
	}

	private static short Saturate_To_Int16(int val)
	{
		if (val > 32767)
		{
			return short.MaxValue;
		}
		if (val < -32768)
		{
			return short.MinValue;
		}
		return (short)val;
	}

	private static ushort Saturate_To_UnsignedInt16(int val)
	{
		if (val > 65535)
		{
			return ushort.MaxValue;
		}
		if (val < 0)
		{
			return 0;
		}
		return (ushort)val;
	}

	private static bool IsNaN(uint v)
	{
		return (v & 0x7FFFFFFF) > 2139095040;
	}

	private static bool IsNaN(ulong v)
	{
		return (v & 0x7FFFFFFFFFFFFFFFL) > 9218868437227405312L;
	}

	private static void BurstIntrinsicSetCSRFromManaged(int _)
	{
	}

	private static int BurstIntrinsicGetCSRFromManaged()
	{
		return 0;
	}

	internal static int getcsr_raw()
	{
		return DoGetCSRTrampoline();
	}

	internal static void setcsr_raw(int bits)
	{
		DoSetCSRTrampoline(bits);
	}

	[BurstCompile(CompileSynchronously = true)]
	[MonoPInvokeCallback(typeof(Unity_002EBurst_002EIntrinsics_002EDoSetCSRTrampoline_00000129_0024PostfixBurstDelegate))]
	private static void DoSetCSRTrampoline(int bits)
	{
		DoSetCSRTrampoline_00000129_0024BurstDirectCall.Invoke(bits);
	}

	[BurstCompile(CompileSynchronously = true)]
	[MonoPInvokeCallback(typeof(Unity_002EBurst_002EIntrinsics_002EDoGetCSRTrampoline_0000012A_0024PostfixBurstDelegate))]
	private static int DoGetCSRTrampoline()
	{
		return DoGetCSRTrampoline_0000012A_0024BurstDirectCall.Invoke();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	internal static void DoSetCSRTrampoline_0024BurstManaged(int bits)
	{
		if (Sse.IsSseSupported)
		{
			BurstIntrinsicSetCSRFromManaged(bits);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(CompileSynchronously = true)]
	internal static int DoGetCSRTrampoline_0024BurstManaged()
	{
		if (Sse.IsSseSupported)
		{
			return BurstIntrinsicGetCSRFromManaged();
		}
		return 0;
	}
}
