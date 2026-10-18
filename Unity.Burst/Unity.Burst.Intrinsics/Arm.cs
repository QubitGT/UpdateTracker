using System;
using System.Diagnostics;

namespace Unity.Burst.Intrinsics;

public static class Arm
{
	public class Neon
	{
		public static bool IsNeonSupported => false;

		public static bool IsNeonArmv82FeaturesSupported => false;

		public static bool IsNeonCryptoSupported => false;

		public static bool IsNeonDotProdSupported => false;

		public static bool IsNeonRDMASupported => false;

		[DebuggerStepThrough]
		public static v64 vadd_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vadd_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vadd_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vadd_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vadd_u8(v64 a0, v64 a1)
		{
			return vadd_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vaddq_u8(v128 a0, v128 a1)
		{
			return vaddq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vadd_u16(v64 a0, v64 a1)
		{
			return vadd_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vaddq_u16(v128 a0, v128 a1)
		{
			return vaddq_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vadd_u32(v64 a0, v64 a1)
		{
			return vadd_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vaddq_u32(v128 a0, v128 a1)
		{
			return vaddq_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vadd_u64(v64 a0, v64 a1)
		{
			return vadd_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vaddq_u64(v128 a0, v128 a1)
		{
			return vaddq_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vadd_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_s8(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_s16(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_s32(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_u8(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_u16(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_u32(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhadd_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhaddq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhadd_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhaddq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhadd_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhaddq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhadd_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhaddq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhadd_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhaddq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhadd_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhaddq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrhadd_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrhaddq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrhadd_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrhaddq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrhadd_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrhaddq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrhadd_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrhaddq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrhadd_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrhaddq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrhadd_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrhaddq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqadd_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqaddq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqadd_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqaddq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqadd_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqaddq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqadd_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqaddq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqadd_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqaddq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqadd_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqaddq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqadd_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqaddq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqadd_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqaddq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vaddhn_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vaddhn_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vaddhn_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vaddhn_u16(v128 a0, v128 a1)
		{
			return vaddhn_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vaddhn_u32(v128 a0, v128 a1)
		{
			return vaddhn_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vaddhn_u64(v128 a0, v128 a1)
		{
			return vaddhn_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vraddhn_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vraddhn_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vraddhn_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vraddhn_u16(v128 a0, v128 a1)
		{
			return vraddhn_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vraddhn_u32(v128 a0, v128 a1)
		{
			return vraddhn_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vraddhn_u64(v128 a0, v128 a1)
		{
			return vraddhn_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vmul_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_u8(v64 a0, v64 a1)
		{
			return vmul_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vmulq_u8(v128 a0, v128 a1)
		{
			return vmulq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vmul_u16(v64 a0, v64 a1)
		{
			return vmul_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vmulq_u16(v128 a0, v128 a1)
		{
			return vmulq_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vmul_u32(v64 a0, v64 a1)
		{
			return vmul_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vmulq_u32(v128 a0, v128 a1)
		{
			return vmulq_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vmul_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_s8(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_s8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_s16(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_s16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_s32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_u8(v64 a0, v64 a1, v64 a2)
		{
			return vmla_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_u8(v128 a0, v128 a1, v128 a2)
		{
			return vmlaq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vmla_u16(v64 a0, v64 a1, v64 a2)
		{
			return vmla_s16(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_u16(v128 a0, v128 a1, v128 a2)
		{
			return vmlaq_s16(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vmla_u32(v64 a0, v64 a1, v64 a2)
		{
			return vmla_s32(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_u32(v128 a0, v128 a1, v128 a2)
		{
			return vmlaq_s32(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vmla_f32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_f32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_s8(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_s16(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_s32(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_u8(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_u16(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_u32(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_s8(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_s8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_s16(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_s16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_s32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_u8(v64 a0, v64 a1, v64 a2)
		{
			return vmls_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_u8(v128 a0, v128 a1, v128 a2)
		{
			return vmlsq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vmls_u16(v64 a0, v64 a1, v64 a2)
		{
			return vmls_s16(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_u16(v128 a0, v128 a1, v128 a2)
		{
			return vmlsq_s16(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vmls_u32(v64 a0, v64 a1, v64 a2)
		{
			return vmls_s32(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_u32(v128 a0, v128 a1, v128 a2)
		{
			return vmlsq_s32(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vmls_f32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_f32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_s8(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_s16(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_s32(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_u8(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_u16(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_u32(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfma_f32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmaq_f32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfms_f32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmsq_f32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqdmulh_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmulhq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqdmulh_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmulhq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmulh_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmulhq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmulh_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmulhq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_s16(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_s32(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_s16(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_s32(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsub_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsub_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsub_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsub_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsub_u8(v64 a0, v64 a1)
		{
			return vsub_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vsubq_u8(v128 a0, v128 a1)
		{
			return vsubq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vsub_u16(v64 a0, v64 a1)
		{
			return vsub_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vsubq_u16(v128 a0, v128 a1)
		{
			return vsubq_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vsub_u32(v64 a0, v64 a1)
		{
			return vsub_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vsubq_u32(v128 a0, v128 a1)
		{
			return vsubq_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vsub_u64(v64 a0, v64 a1)
		{
			return vsub_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vsubq_u64(v128 a0, v128 a1)
		{
			return vsubq_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vsub_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_s8(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_s16(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_s32(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_u8(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_u16(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_u32(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhsub_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhsubq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhsub_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhsubq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhsub_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhsubq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhsub_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhsubq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhsub_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhsubq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vhsub_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vhsubq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqsub_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqsubq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqsub_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqsubq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqsub_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqsubq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqsub_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqsubq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqsub_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqsubq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqsub_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqsubq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqsub_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqsubq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqsub_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqsubq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsubhn_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsubhn_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsubhn_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsubhn_u16(v128 a0, v128 a1)
		{
			return vsubhn_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vsubhn_u32(v128 a0, v128 a1)
		{
			return vsubhn_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vsubhn_u64(v128 a0, v128 a1)
		{
			return vsubhn_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vrsubhn_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrsubhn_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrsubhn_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrsubhn_u16(v128 a0, v128 a1)
		{
			return vrsubhn_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vrsubhn_u32(v128 a0, v128 a1)
		{
			return vrsubhn_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vrsubhn_u64(v128 a0, v128 a1)
		{
			return vrsubhn_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vceq_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vceq_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vceq_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vceq_u8(v64 a0, v64 a1)
		{
			return vceq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vceqq_u8(v128 a0, v128 a1)
		{
			return vceqq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vceq_u16(v64 a0, v64 a1)
		{
			return vceq_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vceqq_u16(v128 a0, v128 a1)
		{
			return vceqq_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vceq_u32(v64 a0, v64 a1)
		{
			return vceq_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vceqq_u32(v128 a0, v128 a1)
		{
			return vceqq_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vceq_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcge_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgeq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcge_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgeq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcge_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgeq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcge_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgeq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcge_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgeq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcge_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgeq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcge_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgeq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcle_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcleq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcle_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcleq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcle_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcleq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcle_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcleq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcle_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcleq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcle_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcleq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcle_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcleq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgt_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgt_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgt_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgt_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgt_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgt_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgt_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclt_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclt_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclt_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclt_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclt_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclt_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclt_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcage_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcageq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcale_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcaleq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcagt_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcagtq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcalt_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcaltq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtst_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtstq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtst_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtstq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtst_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtstq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtst_u8(v64 a0, v64 a1)
		{
			return vtst_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtstq_u8(v128 a0, v128 a1)
		{
			return vtstq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtst_u16(v64 a0, v64 a1)
		{
			return vtst_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtstq_u16(v128 a0, v128 a1)
		{
			return vtstq_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtst_u32(v64 a0, v64 a1)
		{
			return vtst_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtstq_u32(v128 a0, v128 a1)
		{
			return vtstq_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vabd_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabd_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabd_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabd_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabd_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabd_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabd_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vaba_s8(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabaq_s8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vaba_s16(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabaq_s16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vaba_s32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabaq_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vaba_u8(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabaq_u8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vaba_u16(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabaq_u16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vaba_u32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabaq_u32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_s8(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_s16(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_s32(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_u8(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_u16(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_u32(v128 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmax_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmaxq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmax_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmaxq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmax_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmaxq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmax_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmaxq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmax_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmaxq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmax_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmaxq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmax_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmaxq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmin_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vminq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmin_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vminq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmin_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vminq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmin_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vminq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmin_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vminq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmin_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vminq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmin_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vminq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshl_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshlq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshl_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshlq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshl_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshlq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshl_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshlq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshl_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshlq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshl_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshlq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshl_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshlq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshl_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshlq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshl_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshlq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshl_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshlq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshl_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshlq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshl_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshlq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshl_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshlq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshl_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshlq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshl_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshlq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshl_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshlq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshl_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshlq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshl_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshlq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshl_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshlq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshl_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshlq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshl_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshlq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshl_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshlq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshl_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshlq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshl_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshlq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshr_n_s8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrq_n_s8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshr_n_s16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrq_n_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshr_n_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrq_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshr_n_s64(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrq_n_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshr_n_u8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrq_n_u8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshr_n_u16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrq_n_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshr_n_u32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrq_n_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshr_n_u64(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrq_n_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_n_s8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_n_s8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_n_s16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_n_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_n_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_n_s64(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_n_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_n_u8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_n_u8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_n_u16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_n_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_n_u32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_n_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshl_n_u64(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshlq_n_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrshr_n_s8(v64 a0, int a1)
		{
			return vrshl_s8(a0, new v64((sbyte)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrshrq_n_s8(v128 a0, int a1)
		{
			return vrshlq_s8(a0, new v128((sbyte)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrshr_n_s16(v64 a0, int a1)
		{
			return vrshl_s16(a0, new v64((short)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrshrq_n_s16(v128 a0, int a1)
		{
			return vrshlq_s16(a0, new v128((short)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrshr_n_s32(v64 a0, int a1)
		{
			return vrshl_s32(a0, new v64(-a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrshrq_n_s32(v128 a0, int a1)
		{
			return vrshlq_s32(a0, new v128(-a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrshr_n_s64(v64 a0, int a1)
		{
			return vrshl_s64(a0, new v64((long)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrshrq_n_s64(v128 a0, int a1)
		{
			return vrshlq_s64(a0, new v128((long)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrshr_n_u8(v64 a0, int a1)
		{
			return vrshl_u8(a0, new v64((byte)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrshrq_n_u8(v128 a0, int a1)
		{
			return vrshlq_u8(a0, new v128((byte)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrshr_n_u16(v64 a0, int a1)
		{
			return vrshl_u16(a0, new v64((ushort)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrshrq_n_u16(v128 a0, int a1)
		{
			return vrshlq_u16(a0, new v128((ushort)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrshr_n_u32(v64 a0, int a1)
		{
			return vrshl_u32(a0, new v64(-a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrshrq_n_u32(v128 a0, int a1)
		{
			return vrshlq_u32(a0, new v128(-a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrshr_n_u64(v64 a0, int a1)
		{
			return vrshl_u64(a0, new v64((ulong)(-a1)));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrshrq_n_u64(v128 a0, int a1)
		{
			return vrshlq_u64(a0, new v128((ulong)(-a1)));
		}

		[DebuggerStepThrough]
		public static v64 vsra_n_s8(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsraq_n_s8(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsra_n_s16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsraq_n_s16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsra_n_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsraq_n_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsra_n_s64(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsraq_n_s64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsra_n_u8(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsraq_n_u8(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsra_n_u16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsraq_n_u16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsra_n_u32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsraq_n_u32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsra_n_u64(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsraq_n_u64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrsra_n_s8(v64 a0, v64 a1, int a2)
		{
			return vadd_s8(a0, vrshr_n_s8(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrsraq_n_s8(v128 a0, v128 a1, int a2)
		{
			return vaddq_s8(a0, vrshrq_n_s8(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrsra_n_s16(v64 a0, v64 a1, int a2)
		{
			return vadd_s16(a0, vrshr_n_s16(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrsraq_n_s16(v128 a0, v128 a1, int a2)
		{
			return vaddq_s16(a0, vrshrq_n_s16(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrsra_n_s32(v64 a0, v64 a1, int a2)
		{
			return vadd_s32(a0, vrshr_n_s32(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrsraq_n_s32(v128 a0, v128 a1, int a2)
		{
			return vaddq_s32(a0, vrshrq_n_s32(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrsra_n_s64(v64 a0, v64 a1, int a2)
		{
			return vadd_s64(a0, vrshr_n_s64(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrsraq_n_s64(v128 a0, v128 a1, int a2)
		{
			return vaddq_s64(a0, vrshrq_n_s64(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrsra_n_u8(v64 a0, v64 a1, int a2)
		{
			return vadd_u8(a0, vrshr_n_u8(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrsraq_n_u8(v128 a0, v128 a1, int a2)
		{
			return vaddq_u8(a0, vrshrq_n_u8(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrsra_n_u16(v64 a0, v64 a1, int a2)
		{
			return vadd_u16(a0, vrshr_n_u16(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrsraq_n_u16(v128 a0, v128 a1, int a2)
		{
			return vaddq_u16(a0, vrshrq_n_u16(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrsra_n_u32(v64 a0, v64 a1, int a2)
		{
			return vadd_u32(a0, vrshr_n_u32(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrsraq_n_u32(v128 a0, v128 a1, int a2)
		{
			return vaddq_u32(a0, vrshrq_n_u32(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vrsra_n_u64(v64 a0, v64 a1, int a2)
		{
			return vadd_u64(a0, vrshr_n_u64(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vrsraq_n_u64(v128 a0, v128 a1, int a2)
		{
			return vaddq_u64(a0, vrshrq_n_u64(a1, a2));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vqshl_n_s8(v64 a0, int a1)
		{
			return vqshl_s8(a0, new v64((sbyte)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vqshlq_n_s8(v128 a0, int a1)
		{
			return vqshlq_s8(a0, new v128((sbyte)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vqshl_n_s16(v64 a0, int a1)
		{
			return vqshl_s16(a0, new v64((short)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vqshlq_n_s16(v128 a0, int a1)
		{
			return vqshlq_s16(a0, new v128((short)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vqshl_n_s32(v64 a0, int a1)
		{
			return vqshl_s32(a0, new v64(a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vqshlq_n_s32(v128 a0, int a1)
		{
			return vqshlq_s32(a0, new v128(a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vqshl_n_s64(v64 a0, int a1)
		{
			return vqshl_s64(a0, new v64((long)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vqshlq_n_s64(v128 a0, int a1)
		{
			return vqshlq_s64(a0, new v128((long)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vqshl_n_u8(v64 a0, int a1)
		{
			return vqshl_u8(a0, new v64((byte)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vqshlq_n_u8(v128 a0, int a1)
		{
			return vqshlq_u8(a0, new v128((byte)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vqshl_n_u16(v64 a0, int a1)
		{
			return vqshl_u16(a0, new v64((ushort)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vqshlq_n_u16(v128 a0, int a1)
		{
			return vqshlq_u16(a0, new v128((ushort)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vqshl_n_u32(v64 a0, int a1)
		{
			return vqshl_u32(a0, new v64((uint)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vqshlq_n_u32(v128 a0, int a1)
		{
			return vqshlq_u32(a0, new v128((uint)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v64 vqshl_n_u64(v64 a0, int a1)
		{
			return vqshl_u64(a0, new v64((ulong)a1));
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV7A_NEON32)]
		public static v128 vqshlq_n_u64(v128 a0, int a1)
		{
			return vqshlq_u64(a0, new v128((ulong)a1));
		}

		[DebuggerStepThrough]
		public static v64 vqshlu_n_s8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshluq_n_s8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshlu_n_s16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshluq_n_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshlu_n_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshluq_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshlu_n_s64(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshluq_n_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshrn_n_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshrn_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshrn_n_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshrn_n_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshrn_n_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vshrn_n_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshrun_n_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshrun_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshrun_n_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshrun_n_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshrun_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshrun_n_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshrn_n_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshrn_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshrn_n_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshrn_n_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshrn_n_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqshrn_n_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshrn_n_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshrn_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshrn_n_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshrn_n_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshrn_n_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrshrn_n_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshrn_n_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshrn_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshrn_n_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshrn_n_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshrn_n_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrshrn_n_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_n_s8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_n_s16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_n_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_n_u8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_n_u16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_n_u32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsri_n_s8(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsriq_n_s8(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsri_n_s16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsriq_n_s16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsri_n_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsriq_n_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsri_n_s64(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsriq_n_s64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsri_n_u8(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsriq_n_u8(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsri_n_u16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsriq_n_u16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsri_n_u32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsriq_n_u32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsri_n_u64(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsriq_n_u64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsli_n_s8(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsliq_n_s8(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsli_n_s16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsliq_n_s16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsli_n_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsliq_n_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsli_n_s64(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsliq_n_s64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsli_n_u8(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsliq_n_u8(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsli_n_u16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsliq_n_u16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsli_n_u32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsliq_n_u32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsli_n_u64(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsliq_n_u64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_s32_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_s32_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_u32_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_u32_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_n_s32_f32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_n_s32_f32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_n_u32_f32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_n_u32_f32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_f32_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_f32_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_f32_u32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_f32_u32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_n_f32_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_n_f32_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_n_f32_u32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_n_f32_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmovn_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmovn_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmovn_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmovn_u16(v128 a0)
		{
			return vmovn_s16(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmovn_u32(v128 a0)
		{
			return vmovn_s32(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmovn_u64(v128 a0)
		{
			return vmovn_s64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovn_high_s16(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovn_high_s32(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovn_high_s64(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovn_high_u16(v64 a0, v128 a1)
		{
			return vmovn_high_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vmovn_high_u32(v64 a0, v128 a1)
		{
			return vmovn_high_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vmovn_high_u64(v64 a0, v128 a1)
		{
			return vmovn_high_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vmovl_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_u8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_u16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_u32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqmovn_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqmovn_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqmovn_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqmovn_u16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqmovn_u32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqmovn_u64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqmovun_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqmovun_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqmovun_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_lane_s16(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_lane_s16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_lane_s32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_lane_s32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_lane_u16(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_lane_u16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_lane_u32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_lane_u32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_lane_f32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_lane_f32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_lane_s16(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_lane_s32(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_lane_u16(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_lane_u32(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_lane_s16(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_lane_s32(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_lane_s16(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_lane_s16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_lane_s32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_lane_s32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_lane_u16(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_lane_u16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_lane_u32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_lane_u32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_lane_f32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_lane_f32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_lane_s16(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_lane_s32(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_lane_u16(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_lane_u32(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_lane_s16(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_lane_s32(v128 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_n_s16(v64 a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_n_s16(v128 a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_n_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_n_u16(v64 a0, ushort a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_n_u16(v128 a0, ushort a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_n_u32(v64 a0, uint a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_n_u32(v128 a0, uint a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_n_f32(v64 a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_n_f32(v128 a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_lane_s16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_lane_s16(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_lane_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_lane_s32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_lane_u16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_lane_u16(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_lane_u32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_lane_u32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_lane_f32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_lane_f32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_n_s16(v64 a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_n_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_n_u16(v64 a0, ushort a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_n_u32(v64 a0, uint a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_lane_s16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_lane_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_lane_u16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_lane_u32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_n_s16(v64 a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_n_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_lane_s16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_lane_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqdmulh_n_s16(v64 a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmulhq_n_s16(v128 a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqdmulh_n_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmulhq_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqdmulh_lane_s16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmulhq_lane_s16(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqdmulh_lane_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmulhq_lane_s32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmulh_n_s16(v64 a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmulhq_n_s16(v128 a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmulh_n_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmulhq_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmulh_lane_s16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmulhq_lane_s16(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmulh_lane_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmulhq_lane_s32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_n_s16(v64 a0, v64 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_n_s16(v128 a0, v128 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_n_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_n_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_n_u16(v64 a0, v64 a1, ushort a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_n_u16(v128 a0, v128 a1, ushort a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_n_u32(v64 a0, v64 a1, uint a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_n_u32(v128 a0, v128 a1, uint a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_n_f32(v64 a0, v64 a1, float a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_n_f32(v128 a0, v128 a1, float a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_n_s16(v128 a0, v64 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_n_s32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_n_u16(v128 a0, v64 a1, ushort a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_n_u32(v128 a0, v64 a1, uint a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_n_s16(v128 a0, v64 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_n_s32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_n_s16(v64 a0, v64 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_n_s16(v128 a0, v128 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_n_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_n_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_n_u16(v64 a0, v64 a1, ushort a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_n_u16(v128 a0, v128 a1, ushort a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_n_u32(v64 a0, v64 a1, uint a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_n_u32(v128 a0, v128 a1, uint a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_n_f32(v64 a0, v64 a1, float a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_n_f32(v128 a0, v128 a1, float a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_n_s16(v128 a0, v64 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_n_s32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_n_u16(v128 a0, v64 a1, ushort a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_n_u32(v128 a0, v64 a1, uint a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_n_s16(v128 a0, v64 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_n_s32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabs_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabsq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabs_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabsq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabs_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabsq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabs_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabsq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqabs_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqabsq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqabs_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqabsq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqabs_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqabsq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vneg_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vnegq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vneg_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vnegq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vneg_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vnegq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vneg_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vnegq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqneg_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqnegq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqneg_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqnegq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqneg_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqnegq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcls_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclsq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcls_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclsq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcls_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclsq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclz_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclzq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclz_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclzq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclz_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclzq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclz_u8(v64 a0)
		{
			return vclz_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vclzq_u8(v128 a0)
		{
			return vclzq_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vclz_u16(v64 a0)
		{
			return vclz_s16(a0);
		}

		[DebuggerStepThrough]
		public static v128 vclzq_u16(v128 a0)
		{
			return vclzq_s16(a0);
		}

		[DebuggerStepThrough]
		public static v64 vclz_u32(v64 a0)
		{
			return vclz_s32(a0);
		}

		[DebuggerStepThrough]
		public static v128 vclzq_u32(v128 a0)
		{
			return vclzq_s32(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcnt_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcntq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcnt_u8(v64 a0)
		{
			return vcnt_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vcntq_u8(v128 a0)
		{
			return vcntq_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vrecpe_u32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrecpeq_u32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrecpe_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrecpeq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrecps_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrecpsq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrsqrte_u32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrsqrteq_u32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrsqrte_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrsqrteq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrsqrts_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrsqrtsq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmvn_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmvnq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmvn_s16(v64 a0)
		{
			return vmvn_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmvnq_s16(v128 a0)
		{
			return vmvnq_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmvn_s32(v64 a0)
		{
			return vmvn_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmvnq_s32(v128 a0)
		{
			return vmvnq_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmvn_u8(v64 a0)
		{
			return vmvn_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmvnq_u8(v128 a0)
		{
			return vmvnq_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmvn_u16(v64 a0)
		{
			return vmvn_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmvnq_u16(v128 a0)
		{
			return vmvnq_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmvn_u32(v64 a0)
		{
			return vmvn_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmvnq_u32(v128 a0)
		{
			return vmvnq_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vand_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vandq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vand_s16(v64 a0, v64 a1)
		{
			return vand_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vandq_s16(v128 a0, v128 a1)
		{
			return vandq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vand_s32(v64 a0, v64 a1)
		{
			return vand_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vandq_s32(v128 a0, v128 a1)
		{
			return vandq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vand_s64(v64 a0, v64 a1)
		{
			return vand_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vandq_s64(v128 a0, v128 a1)
		{
			return vandq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vand_u8(v64 a0, v64 a1)
		{
			return vand_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vandq_u8(v128 a0, v128 a1)
		{
			return vandq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vand_u16(v64 a0, v64 a1)
		{
			return vand_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vandq_u16(v128 a0, v128 a1)
		{
			return vandq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vand_u32(v64 a0, v64 a1)
		{
			return vand_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vandq_u32(v128 a0, v128 a1)
		{
			return vandq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vand_u64(v64 a0, v64 a1)
		{
			return vand_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vandq_u64(v128 a0, v128 a1)
		{
			return vandq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorr_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vorrq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vorr_s16(v64 a0, v64 a1)
		{
			return vorr_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vorrq_s16(v128 a0, v128 a1)
		{
			return vorrq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorr_s32(v64 a0, v64 a1)
		{
			return vorr_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vorrq_s32(v128 a0, v128 a1)
		{
			return vorrq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorr_s64(v64 a0, v64 a1)
		{
			return vorr_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vorrq_s64(v128 a0, v128 a1)
		{
			return vorrq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorr_u8(v64 a0, v64 a1)
		{
			return vorr_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vorrq_u8(v128 a0, v128 a1)
		{
			return vorrq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorr_u16(v64 a0, v64 a1)
		{
			return vorr_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vorrq_u16(v128 a0, v128 a1)
		{
			return vorrq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorr_u32(v64 a0, v64 a1)
		{
			return vorr_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vorrq_u32(v128 a0, v128 a1)
		{
			return vorrq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorr_u64(v64 a0, v64 a1)
		{
			return vorr_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vorrq_u64(v128 a0, v128 a1)
		{
			return vorrq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 veor_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 veorq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 veor_s16(v64 a0, v64 a1)
		{
			return veor_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 veorq_s16(v128 a0, v128 a1)
		{
			return veorq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 veor_s32(v64 a0, v64 a1)
		{
			return veor_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 veorq_s32(v128 a0, v128 a1)
		{
			return veorq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 veor_s64(v64 a0, v64 a1)
		{
			return veor_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 veorq_s64(v128 a0, v128 a1)
		{
			return veorq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 veor_u8(v64 a0, v64 a1)
		{
			return veor_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 veorq_u8(v128 a0, v128 a1)
		{
			return veorq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 veor_u16(v64 a0, v64 a1)
		{
			return veor_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 veorq_u16(v128 a0, v128 a1)
		{
			return veorq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 veor_u32(v64 a0, v64 a1)
		{
			return veor_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 veorq_u32(v128 a0, v128 a1)
		{
			return veorq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 veor_u64(v64 a0, v64 a1)
		{
			return veor_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 veorq_u64(v128 a0, v128 a1)
		{
			return veorq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vbic_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vbicq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vbic_s16(v64 a0, v64 a1)
		{
			return vbic_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vbicq_s16(v128 a0, v128 a1)
		{
			return vbicq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vbic_s32(v64 a0, v64 a1)
		{
			return vbic_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vbicq_s32(v128 a0, v128 a1)
		{
			return vbicq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vbic_s64(v64 a0, v64 a1)
		{
			return vbic_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vbicq_s64(v128 a0, v128 a1)
		{
			return vbicq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vbic_u8(v64 a0, v64 a1)
		{
			return vbic_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vbicq_u8(v128 a0, v128 a1)
		{
			return vbicq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vbic_u16(v64 a0, v64 a1)
		{
			return vbic_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vbicq_u16(v128 a0, v128 a1)
		{
			return vbicq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vbic_u32(v64 a0, v64 a1)
		{
			return vbic_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vbicq_u32(v128 a0, v128 a1)
		{
			return vbicq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vbic_u64(v64 a0, v64 a1)
		{
			return vbic_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vbicq_u64(v128 a0, v128 a1)
		{
			return vbicq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorn_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vornq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vorn_s16(v64 a0, v64 a1)
		{
			return vorn_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vornq_s16(v128 a0, v128 a1)
		{
			return vornq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorn_s32(v64 a0, v64 a1)
		{
			return vorn_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vornq_s32(v128 a0, v128 a1)
		{
			return vornq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorn_s64(v64 a0, v64 a1)
		{
			return vorn_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vornq_s64(v128 a0, v128 a1)
		{
			return vornq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorn_u8(v64 a0, v64 a1)
		{
			return vorn_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vornq_u8(v128 a0, v128 a1)
		{
			return vornq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorn_u16(v64 a0, v64 a1)
		{
			return vorn_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vornq_u16(v128 a0, v128 a1)
		{
			return vornq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorn_u32(v64 a0, v64 a1)
		{
			return vorn_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vornq_u32(v128 a0, v128 a1)
		{
			return vornq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vorn_u64(v64 a0, v64 a1)
		{
			return vorn_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vornq_u64(v128 a0, v128 a1)
		{
			return vornq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vbsl_s8(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vbslq_s8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vbsl_s16(v64 a0, v64 a1, v64 a2)
		{
			return vbsl_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vbslq_s16(v128 a0, v128 a1, v128 a2)
		{
			return vbslq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vbsl_s32(v64 a0, v64 a1, v64 a2)
		{
			return vbsl_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vbslq_s32(v128 a0, v128 a1, v128 a2)
		{
			return vbslq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vbsl_s64(v64 a0, v64 a1, v64 a2)
		{
			return vbsl_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vbslq_s64(v128 a0, v128 a1, v128 a2)
		{
			return vbslq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vbsl_u8(v64 a0, v64 a1, v64 a2)
		{
			return vbsl_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vbslq_u8(v128 a0, v128 a1, v128 a2)
		{
			return vbslq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vbsl_u16(v64 a0, v64 a1, v64 a2)
		{
			return vbsl_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vbslq_u16(v128 a0, v128 a1, v128 a2)
		{
			return vbslq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vbsl_u32(v64 a0, v64 a1, v64 a2)
		{
			return vbsl_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vbslq_u32(v128 a0, v128 a1, v128 a2)
		{
			return vbslq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vbsl_u64(v64 a0, v64 a1, v64 a2)
		{
			return vbsl_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vbslq_u64(v128 a0, v128 a1, v128 a2)
		{
			return vbslq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vbsl_f32(v64 a0, v64 a1, v64 a2)
		{
			return vbsl_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vbslq_f32(v128 a0, v128 a1, v128 a2)
		{
			return vbslq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vdup_lane_s8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_lane_s8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_lane_s16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_lane_s16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_lane_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_lane_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_lane_s64(v64 a0, int a1)
		{
			return a0;
		}

		[DebuggerStepThrough]
		public static v128 vdupq_lane_s64(v64 a0, int a1)
		{
			return new v128(a0, a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_lane_u8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_lane_u8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_lane_u16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_lane_u16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_lane_u32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_lane_u32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_lane_u64(v64 a0, int a1)
		{
			return a0;
		}

		[DebuggerStepThrough]
		public static v128 vdupq_lane_u64(v64 a0, int a1)
		{
			return new v128(a0, a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_lane_f32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_lane_f32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpadd_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpadd_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpadd_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpadd_u8(v64 a0, v64 a1)
		{
			return vpadd_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vpadd_u16(v64 a0, v64 a1)
		{
			return vpadd_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vpadd_u32(v64 a0, v64 a1)
		{
			return vpadd_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vpadd_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpaddl_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddlq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpaddl_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddlq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpaddl_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddlq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpaddl_u8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddlq_u8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpaddl_u16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddlq_u16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpaddl_u32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddlq_u32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpadal_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpadalq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpadal_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpadalq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpadal_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpadalq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpadal_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpadalq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpadal_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpadalq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpadal_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpadalq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmax_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmax_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmax_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmax_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmax_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmax_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmax_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmin_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmin_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmin_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmin_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmin_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmin_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmin_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vext_s8(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vextq_s8(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vext_s16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vextq_s16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vext_s32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vextq_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vext_s64(v64 a0, v64 a1, int a2)
		{
			return a0;
		}

		[DebuggerStepThrough]
		public static v128 vextq_s64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vext_u8(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vextq_u8(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vext_u16(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vextq_u16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vext_u32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vextq_u32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vext_u64(v64 a0, v64 a1, int a2)
		{
			return a0;
		}

		[DebuggerStepThrough]
		public static v128 vextq_u64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vext_f32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vextq_f32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrev64_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrev64q_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrev64_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrev64q_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrev64_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrev64q_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrev64_u8(v64 a0)
		{
			return vrev64_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vrev64q_u8(v128 a0)
		{
			return vrev64q_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vrev64_u16(v64 a0)
		{
			return vrev64_s16(a0);
		}

		[DebuggerStepThrough]
		public static v128 vrev64q_u16(v128 a0)
		{
			return vrev64q_s16(a0);
		}

		[DebuggerStepThrough]
		public static v64 vrev64_u32(v64 a0)
		{
			return vrev64_s32(a0);
		}

		[DebuggerStepThrough]
		public static v128 vrev64q_u32(v128 a0)
		{
			return vrev64q_s32(a0);
		}

		[DebuggerStepThrough]
		public static v64 vrev64_f32(v64 a0)
		{
			return vrev64_s32(a0);
		}

		[DebuggerStepThrough]
		public static v128 vrev64q_f32(v128 a0)
		{
			return vrev64q_s32(a0);
		}

		[DebuggerStepThrough]
		public static v64 vrev32_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrev32q_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrev32_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrev32q_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrev32_u8(v64 a0)
		{
			return vrev32_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vrev32q_u8(v128 a0)
		{
			return vrev32q_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vrev32_u16(v64 a0)
		{
			return vrev32_s16(a0);
		}

		[DebuggerStepThrough]
		public static v128 vrev32q_u16(v128 a0)
		{
			return vrev32q_s16(a0);
		}

		[DebuggerStepThrough]
		public static v64 vrev16_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrev16q_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrev16_u8(v64 a0)
		{
			return vrev16_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vrev16q_u8(v128 a0)
		{
			return vrev16q_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vtbl1_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtbl1_u8(v64 a0, v64 a1)
		{
			return vtbl1_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtbx1_s8(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtbx1_u8(v64 a0, v64 a1, v64 a2)
		{
			return vtbx1_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static byte vget_lane_u8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vget_lane_u16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vget_lane_u32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vget_lane_u64(v64 a0, int a1)
		{
			return a0.ULong0;
		}

		[DebuggerStepThrough]
		public static sbyte vget_lane_s8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vget_lane_s16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vget_lane_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vget_lane_s64(v64 a0, int a1)
		{
			return a0.SLong0;
		}

		[DebuggerStepThrough]
		public static float vget_lane_f32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vgetq_lane_u8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vgetq_lane_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vgetq_lane_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vgetq_lane_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vgetq_lane_s8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vgetq_lane_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vgetq_lane_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vgetq_lane_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vgetq_lane_f32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vset_lane_u8(byte a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vset_lane_u16(ushort a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vset_lane_u32(uint a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vset_lane_u64(ulong a0, v64 a1, int a2)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vset_lane_s8(sbyte a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vset_lane_s16(short a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vset_lane_s32(int a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vset_lane_s64(long a0, v64 a1, int a2)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vset_lane_f32(float a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsetq_lane_u8(byte a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsetq_lane_u16(ushort a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsetq_lane_u32(uint a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsetq_lane_u64(ulong a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsetq_lane_s8(sbyte a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsetq_lane_s16(short a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsetq_lane_s32(int a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsetq_lane_s64(long a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsetq_lane_f32(float a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfma_n_f32(v64 a0, v64 a1, float a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmaq_n_f32(v128 a0, v128 a1, float a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vadd_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vaddd_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vaddd_u64(ulong a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_high_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_high_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_high_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_high_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_high_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddl_high_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_high_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_high_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_high_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_high_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_high_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddw_high_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vqaddb_s8(sbyte a0, sbyte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqaddh_s16(short a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqadds_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqaddd_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqaddb_u8(byte a0, byte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqaddh_u16(ushort a0, ushort a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqadds_u32(uint a0, uint a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vqaddd_u64(ulong a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vuqadd_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuqaddq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vuqadd_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuqaddq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vuqadd_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuqaddq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vuqadd_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuqaddq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vuqaddb_s8(sbyte a0, byte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vuqaddh_s16(short a0, ushort a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vuqadds_s32(int a0, uint a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vuqaddd_s64(long a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsqadd_u8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsqaddq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsqadd_u16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsqaddq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsqadd_u32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsqaddq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsqadd_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsqaddq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vsqaddb_u8(byte a0, sbyte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vsqaddh_u16(ushort a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vsqadds_u32(uint a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vsqaddd_u64(ulong a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddhn_high_s16(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddhn_high_s32(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddhn_high_s64(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaddhn_high_u16(v64 a0, v128 a1, v128 a2)
		{
			return vaddhn_high_s16(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vaddhn_high_u32(v64 a0, v128 a1, v128 a2)
		{
			return vaddhn_high_s32(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vaddhn_high_u64(v64 a0, v128 a1, v128 a2)
		{
			return vaddhn_high_s64(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vraddhn_high_s16(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vraddhn_high_s32(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vraddhn_high_s64(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vraddhn_high_u16(v64 a0, v128 a1, v128 a2)
		{
			return vraddhn_high_s16(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vraddhn_high_u32(v64 a0, v128 a1, v128 a2)
		{
			return vraddhn_high_s32(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vraddhn_high_u64(v64 a0, v128 a1, v128 a2)
		{
			return vraddhn_high_s64(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vmul_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmulx_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulxq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmulx_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulxq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vmulxs_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vmulxd_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmulx_lane_f32(v64 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulxq_lane_f32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmulx_lane_f64(v64 a0, v64 a1, int a2)
		{
			return vmulx_f64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vmulxq_lane_f64(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vmulxs_lane_f32(float a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vmulxd_lane_f64(double a0, v64 a1, int a2)
		{
			return vmulxd_f64(a0, a1.Double0);
		}

		[DebuggerStepThrough]
		public static v64 vmulx_laneq_f32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulxq_laneq_f32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmulx_laneq_f64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulxq_laneq_f64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vmulxs_laneq_f32(float a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vmulxd_laneq_f64(double a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdiv_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdivq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdiv_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdivq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_f64(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_f64(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_s8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_s16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_u8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_u16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_u32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_f64(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_f64(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_s8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_s16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_u8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_u16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_u32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfma_f64(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmaq_f64(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfma_lane_f32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmaq_lane_f32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV8A_AARCH64)]
		public static v64 vfma_lane_f64(v64 a0, v64 a1, v64 a2, int a3)
		{
			return vfma_f64(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vfmaq_lane_f64(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vfmas_lane_f32(float a0, float a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV8A_AARCH64)]
		public static double vfmad_lane_f64(double a0, double a1, v64 a2, int a3)
		{
			return vfma_f64(new v64(a0), new v64(a1), a2).Double0;
		}

		[DebuggerStepThrough]
		public static v64 vfma_laneq_f32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmaq_laneq_f32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfma_laneq_f64(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmaq_laneq_f64(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vfmas_laneq_f32(float a0, float a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vfmad_laneq_f64(double a0, double a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfms_f64(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmsq_f64(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfms_lane_f32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmsq_lane_f32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV8A_AARCH64)]
		public static v64 vfms_lane_f64(v64 a0, v64 a1, v64 a2, int a3)
		{
			return vfms_f64(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vfmsq_lane_f64(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vfmss_lane_f32(float a0, float a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV8A_AARCH64)]
		public static double vfmsd_lane_f64(double a0, double a1, v64 a2, int a3)
		{
			return vfms_f64(new v64(a0), new v64(a1), a2).Double0;
		}

		[DebuggerStepThrough]
		public static v64 vfms_laneq_f32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmsq_laneq_f32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfms_laneq_f64(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmsq_laneq_f64(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vfmss_laneq_f32(float a0, float a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vfmsd_laneq_f64(double a0, double a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqdmulhh_s16(short a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmulhs_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrdmulhh_s16(short a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqrdmulhs_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmlalh_s16(int a0, short a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqdmlals_s32(long a0, int a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_high_s16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_high_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmlslh_s16(int a0, short a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqdmlsls_s32(long a0, int a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_high_s16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_high_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmullh_s16(short a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqdmulls_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_high_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_high_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsub_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vsubd_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vsubd_u64(ulong a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_high_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_high_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_high_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_high_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_high_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubl_high_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_high_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_high_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_high_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_high_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_high_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubw_high_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vqsubb_s8(sbyte a0, sbyte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqsubh_s16(short a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqsubs_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqsubd_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqsubb_u8(byte a0, byte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqsubh_u16(ushort a0, ushort a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqsubs_u32(uint a0, uint a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vqsubd_u64(ulong a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubhn_high_s16(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubhn_high_s32(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubhn_high_s64(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsubhn_high_u16(v64 a0, v128 a1, v128 a2)
		{
			return vsubhn_high_s16(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vsubhn_high_u32(v64 a0, v128 a1, v128 a2)
		{
			return vsubhn_high_s32(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vsubhn_high_u64(v64 a0, v128 a1, v128 a2)
		{
			return vsubhn_high_s64(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vrsubhn_high_s16(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrsubhn_high_s32(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrsubhn_high_s64(v64 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrsubhn_high_u16(v64 a0, v128 a1, v128 a2)
		{
			return vrsubhn_high_s16(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vrsubhn_high_u32(v64 a0, v128 a1, v128 a2)
		{
			return vrsubhn_high_s32(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vrsubhn_high_u64(v64 a0, v128 a1, v128 a2)
		{
			return vrsubhn_high_s64(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vceq_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vceq_u64(v64 a0, v64 a1)
		{
			return vceq_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vceqq_u64(v128 a0, v128 a1)
		{
			return vceqq_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vceq_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vceqd_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vceqd_u64(ulong a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vceqs_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vceqd_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vceqz_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqzq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vceqz_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqzq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vceqz_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqzq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vceqz_u8(v64 a0)
		{
			return vceqz_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vceqzq_u8(v128 a0)
		{
			return vceqzq_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vceqz_u16(v64 a0)
		{
			return vceqz_s16(a0);
		}

		[DebuggerStepThrough]
		public static v128 vceqzq_u16(v128 a0)
		{
			return vceqzq_s16(a0);
		}

		[DebuggerStepThrough]
		public static v64 vceqz_u32(v64 a0)
		{
			return vceqz_s32(a0);
		}

		[DebuggerStepThrough]
		public static v128 vceqzq_u32(v128 a0)
		{
			return vceqzq_s32(a0);
		}

		[DebuggerStepThrough]
		public static v64 vceqz_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqzq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vceqz_s64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqzq_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vceqz_u64(v64 a0)
		{
			return vceqz_s64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vceqzq_u64(v128 a0)
		{
			return vceqzq_s64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vceqz_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vceqzq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vceqzd_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vceqzd_u64(ulong a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vceqzs_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vceqzd_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcge_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgeq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcge_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgeq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcge_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgeq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcged_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcged_u64(ulong a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcges_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcged_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgez_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgezq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgez_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgezq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgez_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgezq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgez_s64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgezq_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgez_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgezq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgez_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgezq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcgezd_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcgezs_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcgezd_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcle_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcleq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcle_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcleq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcle_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcleq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcled_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcled_u64(ulong a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcles_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcled_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclez_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclezq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclez_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclezq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclez_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclezq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclez_s64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclezq_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclez_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclezq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclez_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vclezq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vclezd_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vclezs_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vclezd_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgt_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgt_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgt_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcgtd_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcgtd_u64(ulong a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcgts_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcgtd_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgtz_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtzq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgtz_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtzq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgtz_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtzq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgtz_s64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtzq_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgtz_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtzq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcgtz_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcgtzq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcgtzd_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcgtzs_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcgtzd_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclt_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclt_u64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltq_u64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vclt_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcltd_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcltd_u64(ulong a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vclts_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcltd_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcltz_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltzq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcltz_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltzq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcltz_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltzq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcltz_s64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltzq_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcltz_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltzq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcltz_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcltzq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcltzd_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcltzs_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcltzd_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcage_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcageq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcages_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcaged_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcale_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcaleq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcales_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcaled_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcagt_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcagtq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcagts_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcagtd_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcalt_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcaltq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcalts_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcaltd_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtst_s64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtstq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtst_u64(v64 a0, v64 a1)
		{
			return vtst_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtstq_u64(v128 a0, v128 a1)
		{
			return vtstq_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static ulong vtstd_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vtstd_u64(ulong a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabd_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vabds_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vabdd_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_high_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_high_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_high_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_high_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_high_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabdl_high_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_high_s8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_high_s16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_high_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_high_u8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_high_u16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabal_high_u32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmax_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmaxq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmin_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vminq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmaxnm_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmaxnmq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmaxnm_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmaxnmq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vminnm_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vminnmq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vminnm_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vminnmq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vshld_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vshld_u64(ulong a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vqshlb_s8(sbyte a0, sbyte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqshlh_s16(short a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqshls_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqshld_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqshlb_u8(byte a0, sbyte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqshlh_u16(ushort a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqshls_u32(uint a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vqshld_u64(ulong a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vrshld_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vrshld_u64(ulong a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vqrshlb_s8(sbyte a0, sbyte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrshlh_s16(short a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqrshls_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqrshld_s64(long a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqrshlb_u8(byte a0, sbyte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqrshlh_u16(ushort a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqrshls_u32(uint a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vqrshld_u64(ulong a0, long a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vshrd_n_s64(long a0, int a1)
		{
			return a0 >> a1;
		}

		[DebuggerStepThrough]
		public static ulong vshrd_n_u64(ulong a0, int a1)
		{
			return a0 >> a1;
		}

		[DebuggerStepThrough]
		public static long vshld_n_s64(long a0, int a1)
		{
			return a0 << a1;
		}

		[DebuggerStepThrough]
		public static ulong vshld_n_u64(ulong a0, int a1)
		{
			return a0 << a1;
		}

		[DebuggerStepThrough]
		public static long vrshrd_n_s64(long a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vrshrd_n_u64(ulong a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vsrad_n_s64(long a0, long a1, int a2)
		{
			return a0 + (a1 >> a2);
		}

		[DebuggerStepThrough]
		public static ulong vsrad_n_u64(ulong a0, ulong a1, int a2)
		{
			return a0 + (a1 >> a2);
		}

		[DebuggerStepThrough]
		public static long vrsrad_n_s64(long a0, long a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vrsrad_n_u64(ulong a0, ulong a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vqshlb_n_s8(sbyte a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqshlh_n_s16(short a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqshls_n_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqshld_n_s64(long a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqshlb_n_u8(byte a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqshlh_n_u16(ushort a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqshls_n_u32(uint a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vqshld_n_u64(ulong a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqshlub_n_s8(sbyte a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqshluh_n_s16(short a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqshlus_n_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vqshlud_n_s64(long a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrn_high_n_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrn_high_n_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrn_high_n_s64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrn_high_n_u16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrn_high_n_u32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshrn_high_n_u64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqshrunh_n_s16(short a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqshruns_n_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqshrund_n_s64(long a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshrun_high_n_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshrun_high_n_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshrun_high_n_s64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqrshrunh_n_s16(short a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqrshruns_n_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqrshrund_n_s64(long a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshrun_high_n_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshrun_high_n_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshrun_high_n_s64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vqshrnh_n_s16(short a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqshrns_n_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqshrnd_n_s64(long a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqshrnh_n_u16(ushort a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqshrns_n_u32(uint a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqshrnd_n_u64(ulong a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshrn_high_n_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshrn_high_n_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshrn_high_n_s64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshrn_high_n_u16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshrn_high_n_u32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqshrn_high_n_u64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshrn_high_n_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshrn_high_n_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshrn_high_n_s64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshrn_high_n_u16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshrn_high_n_u32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrshrn_high_n_u64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vqrshrnh_n_s16(short a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrshrns_n_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqrshrnd_n_s64(long a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqrshrnh_n_u16(ushort a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqrshrns_n_u32(uint a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqrshrnd_n_u64(ulong a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshrn_high_n_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshrn_high_n_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshrn_high_n_s64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshrn_high_n_u16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshrn_high_n_u32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrshrn_high_n_u64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_high_n_s8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_high_n_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_high_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_high_n_u8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_high_n_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vshll_high_n_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vsrid_n_s64(long a0, long a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vsrid_n_u64(ulong a0, ulong a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vslid_n_s64(long a0, long a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vslid_n_u64(ulong a0, ulong a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtn_s32_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtnq_s32_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtn_u32_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtnq_u32_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtm_s32_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtmq_s32_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtm_u32_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtmq_u32_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtp_s32_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtpq_s32_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtp_u32_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtpq_u32_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvta_s32_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtaq_s32_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvta_u32_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtaq_u32_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vcvts_s32_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcvts_u32_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vcvtns_s32_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcvtns_u32_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vcvtms_s32_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcvtms_u32_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vcvtps_s32_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcvtps_u32_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vcvtas_s32_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcvtas_u32_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_s64_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_s64_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_u64_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_u64_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtn_s64_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtnq_s64_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtn_u64_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtnq_u64_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtm_s64_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtmq_s64_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtm_u64_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtmq_u64_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtp_s64_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtpq_s64_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtp_u64_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtpq_u64_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvta_s64_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtaq_s64_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvta_u64_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtaq_u64_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vcvtd_s64_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcvtd_u64_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vcvtnd_s64_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcvtnd_u64_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vcvtmd_s64_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcvtmd_u64_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vcvtpd_s64_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcvtpd_u64_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vcvtad_s64_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcvtad_u64_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vcvts_n_s32_f32(float a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vcvts_n_u32_f32(float a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_n_s64_f64(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_n_s64_f64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_n_u64_f64(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_n_u64_f64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vcvtd_n_s64_f64(double a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vcvtd_n_u64_f64(double a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vcvts_f32_s32(int a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vcvts_f32_u32(uint a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_f64_s64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_f64_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_f64_u64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_f64_u64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vcvtd_f64_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vcvtd_f64_u64(ulong a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vcvts_n_f32_s32(int a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vcvts_n_f32_u32(uint a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_n_f64_s64(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_n_f64_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_n_f64_u64(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtq_n_f64_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vcvtd_n_f64_s64(long a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vcvtd_n_f64_u64(ulong a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvt_f32_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvt_high_f32_f64(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvt_f64_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvt_high_f64_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcvtx_f32_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vcvtxd_f32_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcvtx_high_f32_f64(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrnd_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrnd_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrndn_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndnq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrndn_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndnq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vrndns_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrndm_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndmq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrndm_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndmq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrndp_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndpq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrndp_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndpq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrnda_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndaq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrnda_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndaq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrndi_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndiq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrndi_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndiq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrndx_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndxq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrndx_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrndxq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_high_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_high_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_high_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_high_u8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_high_u16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmovl_high_u32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vqmovnh_s16(short a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqmovns_s32(int a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqmovnd_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqmovnh_u16(ushort a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqmovns_u32(uint a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqmovnd_u64(ulong a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqmovn_high_s16(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqmovn_high_s32(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqmovn_high_s64(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqmovn_high_u16(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqmovn_high_u32(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqmovn_high_u64(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vqmovunh_s16(short a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vqmovuns_s32(int a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vqmovund_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqmovun_high_s16(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqmovun_high_s32(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqmovun_high_s64(v64 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_laneq_s16(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_laneq_s16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_laneq_s32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_laneq_s32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_laneq_u16(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_laneq_u16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_laneq_u32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_laneq_u32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmla_laneq_f32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlaq_laneq_f32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_lane_s16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_lane_s32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_lane_u16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_lane_u32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_laneq_s16(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_laneq_s32(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_laneq_u16(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_laneq_u32(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_laneq_s16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_laneq_s32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_laneq_u16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_laneq_u32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmlalh_lane_s16(int a0, short a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqdmlals_lane_s32(long a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_high_lane_s16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_high_lane_s32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_laneq_s16(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_laneq_s32(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmlalh_laneq_s16(int a0, short a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqdmlals_laneq_s32(long a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_high_laneq_s16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_high_laneq_s32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_laneq_s16(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_laneq_s16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_laneq_s32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_laneq_s32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_laneq_u16(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_laneq_u16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_laneq_u32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_laneq_u32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmls_laneq_f32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsq_laneq_f32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_lane_s16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_lane_s32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_lane_u16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_lane_u32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_laneq_s16(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_laneq_s32(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_laneq_u16(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_laneq_u32(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_laneq_s16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_laneq_s32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_laneq_u16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_laneq_u32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmlslh_lane_s16(int a0, short a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqdmlsls_lane_s32(long a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_high_lane_s16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_high_lane_s32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_laneq_s16(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_laneq_s32(v128 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmlslh_laneq_s16(int a0, short a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqdmlsls_laneq_s32(long a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_high_laneq_s16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_high_laneq_s32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_n_f64(v64 a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_n_f64(v128 a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		[BurstTargetCpu(BurstTargetCpu.ARMV8A_AARCH64)]
		public static v64 vmul_lane_f64(v64 a0, v64 a1, int a2)
		{
			return vmul_f64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vmulq_lane_f64(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vmuls_lane_f32(float a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vmuld_lane_f64(double a0, v64 a1, int a2)
		{
			return a0 * a1.Double0;
		}

		[DebuggerStepThrough]
		public static v64 vmul_laneq_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_laneq_s16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_laneq_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_laneq_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_laneq_u16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_laneq_u16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_laneq_u32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_laneq_u32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_laneq_f32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_laneq_f32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vmul_laneq_f64(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmulq_laneq_f64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vmuls_laneq_f32(float a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vmuld_laneq_f64(double a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_n_s16(v128 a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_n_u16(v128 a0, ushort a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_n_u32(v128 a0, uint a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_lane_s16(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_lane_s32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_lane_u16(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_lane_u32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_laneq_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_laneq_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_laneq_u16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_laneq_u32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_laneq_s16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_laneq_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_laneq_u16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmull_high_laneq_u32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_high_n_s16(v128 a0, short a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_high_n_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmullh_lane_s16(short a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqdmulls_lane_s32(int a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_high_lane_s16(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_high_lane_s32(v128 a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_laneq_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_laneq_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmullh_laneq_s16(short a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqdmulls_laneq_s32(int a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_high_laneq_s16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmull_high_laneq_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqdmulhh_lane_s16(short a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmulhs_lane_s32(int a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqdmulh_laneq_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmulhq_laneq_s16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqdmulh_laneq_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmulhq_laneq_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqdmulhh_laneq_s16(short a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqdmulhs_laneq_s32(int a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrdmulhh_lane_s16(short a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqrdmulhs_lane_s32(int a0, v64 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmulh_laneq_s16(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmulhq_laneq_s16(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmulh_laneq_s32(v64 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmulhq_laneq_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrdmulhh_laneq_s16(short a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqrdmulhs_laneq_s32(int a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_n_s16(v128 a0, v128 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_n_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_n_u16(v128 a0, v128 a1, ushort a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlal_high_n_u32(v128 a0, v128 a1, uint a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_high_n_s16(v128 a0, v128 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlal_high_n_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_n_s16(v128 a0, v128 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_n_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_n_u16(v128 a0, v128 a1, ushort a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vmlsl_high_n_u32(v128 a0, v128 a1, uint a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_high_n_s16(v128 a0, v128 a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqdmlsl_high_n_s32(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabs_s64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vabsd_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabsq_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vabs_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vabsq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqabs_s64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqabsq_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vqabsb_s8(sbyte a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqabsh_s16(short a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqabss_s32(int a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqabsd_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vneg_s64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vnegd_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vnegq_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vneg_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vnegq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqneg_s64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqnegq_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vqnegb_s8(sbyte a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqnegh_s16(short a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqnegs_s32(int a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vqnegd_s64(long a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrecpe_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrecpeq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vrecpes_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vrecped_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrecps_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrecpsq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vrecpss_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vrecpsd_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsqrt_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsqrtq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vsqrt_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsqrtq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrsqrte_f64(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrsqrteq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vrsqrtes_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vrsqrted_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrsqrts_f64(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrsqrtsq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vrsqrtss_f32(float a0, float a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vrsqrtsd_f64(double a0, double a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vbsl_f64(v64 a0, v64 a1, v64 a2)
		{
			return vbsl_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vbslq_f64(v128 a0, v128 a1, v128 a2)
		{
			return vbslq_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v64 vcopy_lane_s8(v64 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_lane_s8(v128 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_lane_s16(v64 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_lane_s16(v128 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_lane_s32(v64 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_lane_s32(v128 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_lane_s64(v64 a0, int a1, v64 a2, int a3)
		{
			return a2;
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_lane_s64(v128 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_lane_u8(v64 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_lane_u8(v128 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_lane_u16(v64 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_lane_u16(v128 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_lane_u32(v64 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_lane_u32(v128 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_lane_u64(v64 a0, int a1, v64 a2, int a3)
		{
			return a2;
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_lane_u64(v128 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_lane_f32(v64 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_lane_f32(v128 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_lane_f64(v64 a0, int a1, v64 a2, int a3)
		{
			return a2;
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_lane_f64(v128 a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_laneq_s8(v64 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_laneq_s8(v128 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_laneq_s16(v64 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_laneq_s16(v128 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_laneq_s32(v64 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_laneq_s32(v128 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_laneq_s64(v64 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_laneq_s64(v128 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_laneq_u8(v64 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_laneq_u8(v128 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_laneq_u16(v64 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_laneq_u16(v128 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_laneq_u32(v64 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_laneq_u32(v128 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_laneq_u64(v64 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_laneq_u64(v128 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_laneq_f32(v64 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_laneq_f32(v128 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcopy_laneq_f64(v64 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vcopyq_laneq_f64(v128 a0, int a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrbit_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vrbitq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vrbit_u8(v64 a0)
		{
			return vrbit_s8(a0);
		}

		[DebuggerStepThrough]
		public static v128 vrbitq_u8(v128 a0)
		{
			return vrbitq_s8(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_lane_f64(v64 a0, int a1)
		{
			return a0;
		}

		[DebuggerStepThrough]
		public static v128 vdupq_lane_f64(v64 a0, int a1)
		{
			return new v128(a0, a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_laneq_s8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_laneq_s8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_laneq_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_laneq_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_laneq_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_laneq_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_laneq_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_laneq_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_laneq_u8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_laneq_u8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_laneq_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_laneq_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_laneq_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_laneq_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_laneq_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_laneq_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_laneq_f32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_laneq_f32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdup_laneq_f64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdupq_laneq_f64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vdupb_lane_s8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vduph_lane_s16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vdups_lane_s32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vdupd_lane_s64(v64 a0, int a1)
		{
			return a0.SLong0;
		}

		[DebuggerStepThrough]
		public static byte vdupb_lane_u8(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vduph_lane_u16(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vdups_lane_u32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vdupd_lane_u64(v64 a0, int a1)
		{
			return a0.ULong0;
		}

		[DebuggerStepThrough]
		public static float vdups_lane_f32(v64 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vdupd_lane_f64(v64 a0, int a1)
		{
			return a0.Double0;
		}

		[DebuggerStepThrough]
		public static sbyte vdupb_laneq_s8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vduph_laneq_s16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vdups_laneq_s32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vdupd_laneq_s64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vdupb_laneq_u8(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vduph_laneq_u16(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vdups_laneq_u32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vdupd_laneq_u64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vdups_laneq_f32(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vdupd_laneq_f64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddq_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddq_u8(v128 a0, v128 a1)
		{
			return vpaddq_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vpaddq_u16(v128 a0, v128 a1)
		{
			return vpaddq_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vpaddq_u32(v128 a0, v128 a1)
		{
			return vpaddq_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vpaddq_u64(v128 a0, v128 a1)
		{
			return vpaddq_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vpaddq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpaddq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpmaxq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpmaxq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpmaxq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpmaxq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpmaxq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpmaxq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpmaxq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpmaxq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpminq_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpminq_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpminq_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpminq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpminq_u16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpminq_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpminq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpminq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpmaxnm_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpmaxnmq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpmaxnmq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vpminnm_f32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpminnmq_f32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vpminnmq_f64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vpaddd_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vpaddd_u64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vpadds_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vpaddd_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vpmaxs_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vpmaxqd_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vpmins_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vpminqd_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vpmaxnms_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vpmaxnmqd_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vpminnms_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vpminnmqd_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vaddv_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vaddvq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vaddv_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vaddvq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vaddv_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vaddvq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vaddvq_s64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vaddv_u8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vaddvq_u8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vaddv_u16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vaddvq_u16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vaddv_u32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vaddvq_u32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vaddvq_u64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vaddv_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vaddvq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vaddvq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vaddlv_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vaddlvq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vaddlv_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vaddlvq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vaddlv_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static long vaddlvq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vaddlv_u8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vaddlvq_u8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vaddlv_u16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vaddlvq_u16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vaddlv_u32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ulong vaddlvq_u32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vmaxv_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vmaxvq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vmaxv_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vmaxvq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vmaxv_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vmaxvq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vmaxv_u8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vmaxvq_u8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vmaxv_u16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vmaxvq_u16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vmaxv_u32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vmaxvq_u32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vmaxv_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vmaxvq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vmaxvq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vminv_s8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static sbyte vminvq_s8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vminv_s16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vminvq_s16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vminv_s32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vminvq_s32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vminv_u8(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static byte vminvq_u8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vminv_u16(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static ushort vminvq_u16(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vminv_u32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vminvq_u32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vminv_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vminvq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vminvq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vmaxnmv_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vmaxnmvq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vmaxnmvq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vminnmv_f32(v64 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vminnmvq_f32(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vminnmvq_f64(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vext_f64(v64 a0, v64 a1, int a2)
		{
			return a0;
		}

		[DebuggerStepThrough]
		public static v128 vextq_f64(v128 a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vzip1_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vzip1q_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vzip1_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vzip1q_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vzip1_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vzip1q_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vzip1q_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vzip1_u8(v64 a0, v64 a1)
		{
			return vzip1_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip1q_u8(v128 a0, v128 a1)
		{
			return vzip1q_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vzip1_u16(v64 a0, v64 a1)
		{
			return vzip1_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip1q_u16(v128 a0, v128 a1)
		{
			return vzip1q_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vzip1_u32(v64 a0, v64 a1)
		{
			return vzip1_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip1q_u32(v128 a0, v128 a1)
		{
			return vzip1q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip1q_u64(v128 a0, v128 a1)
		{
			return vzip1q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vzip1_f32(v64 a0, v64 a1)
		{
			return vzip1_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip1q_f32(v128 a0, v128 a1)
		{
			return vzip1q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip1q_f64(v128 a0, v128 a1)
		{
			return vzip1q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vzip2_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vzip2q_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vzip2_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vzip2q_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vzip2_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vzip2q_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vzip2q_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vzip2_u8(v64 a0, v64 a1)
		{
			return vzip2_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip2q_u8(v128 a0, v128 a1)
		{
			return vzip2q_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vzip2_u16(v64 a0, v64 a1)
		{
			return vzip2_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip2q_u16(v128 a0, v128 a1)
		{
			return vzip2q_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vzip2_u32(v64 a0, v64 a1)
		{
			return vzip2_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip2q_u32(v128 a0, v128 a1)
		{
			return vzip2q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip2q_u64(v128 a0, v128 a1)
		{
			return vzip2q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vzip2_f32(v64 a0, v64 a1)
		{
			return vzip2_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip2q_f32(v128 a0, v128 a1)
		{
			return vzip2q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vzip2q_f64(v128 a0, v128 a1)
		{
			return vzip2q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vuzp1_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuzp1q_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vuzp1_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuzp1q_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vuzp1_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuzp1q_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuzp1q_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vuzp1_u8(v64 a0, v64 a1)
		{
			return vuzp1_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp1q_u8(v128 a0, v128 a1)
		{
			return vuzp1q_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vuzp1_u16(v64 a0, v64 a1)
		{
			return vuzp1_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp1q_u16(v128 a0, v128 a1)
		{
			return vuzp1q_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vuzp1_u32(v64 a0, v64 a1)
		{
			return vuzp1_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp1q_u32(v128 a0, v128 a1)
		{
			return vuzp1q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp1q_u64(v128 a0, v128 a1)
		{
			return vuzp1q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vuzp1_f32(v64 a0, v64 a1)
		{
			return vuzp1_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp1q_f32(v128 a0, v128 a1)
		{
			return vuzp1q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp1q_f64(v128 a0, v128 a1)
		{
			return vuzp1q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vuzp2_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuzp2q_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vuzp2_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuzp2q_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vuzp2_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuzp2q_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vuzp2q_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vuzp2_u8(v64 a0, v64 a1)
		{
			return vuzp2_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp2q_u8(v128 a0, v128 a1)
		{
			return vuzp2q_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vuzp2_u16(v64 a0, v64 a1)
		{
			return vuzp2_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp2q_u16(v128 a0, v128 a1)
		{
			return vuzp2q_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vuzp2_u32(v64 a0, v64 a1)
		{
			return vuzp2_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp2q_u32(v128 a0, v128 a1)
		{
			return vuzp2q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp2q_u64(v128 a0, v128 a1)
		{
			return vuzp2q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vuzp2_f32(v64 a0, v64 a1)
		{
			return vuzp2_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp2q_f32(v128 a0, v128 a1)
		{
			return vuzp2q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vuzp2q_f64(v128 a0, v128 a1)
		{
			return vuzp2q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtrn1_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtrn1q_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtrn1_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtrn1q_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtrn1_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtrn1q_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtrn1q_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtrn1_u8(v64 a0, v64 a1)
		{
			return vtrn1_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn1q_u8(v128 a0, v128 a1)
		{
			return vtrn1q_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtrn1_u16(v64 a0, v64 a1)
		{
			return vtrn1_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn1q_u16(v128 a0, v128 a1)
		{
			return vtrn1q_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtrn1_u32(v64 a0, v64 a1)
		{
			return vtrn1_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn1q_u32(v128 a0, v128 a1)
		{
			return vtrn1q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn1q_u64(v128 a0, v128 a1)
		{
			return vtrn1q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtrn1_f32(v64 a0, v64 a1)
		{
			return vtrn1_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn1q_f32(v128 a0, v128 a1)
		{
			return vtrn1q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn1q_f64(v128 a0, v128 a1)
		{
			return vtrn1q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtrn2_s8(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtrn2q_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtrn2_s16(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtrn2q_s16(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtrn2_s32(v64 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtrn2q_s32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vtrn2q_s64(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vtrn2_u8(v64 a0, v64 a1)
		{
			return vtrn2_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn2q_u8(v128 a0, v128 a1)
		{
			return vtrn2q_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtrn2_u16(v64 a0, v64 a1)
		{
			return vtrn2_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn2q_u16(v128 a0, v128 a1)
		{
			return vtrn2q_s16(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtrn2_u32(v64 a0, v64 a1)
		{
			return vtrn2_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn2q_u32(v128 a0, v128 a1)
		{
			return vtrn2q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn2q_u64(v128 a0, v128 a1)
		{
			return vtrn2q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vtrn2_f32(v64 a0, v64 a1)
		{
			return vtrn2_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn2q_f32(v128 a0, v128 a1)
		{
			return vtrn2q_s32(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vtrn2q_f64(v128 a0, v128 a1)
		{
			return vtrn2q_s64(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vqtbl1_s8(v128 a0, v64 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqtbl1q_s8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqtbl1_u8(v128 a0, v64 a1)
		{
			return vqtbl1_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vqtbl1q_u8(v128 a0, v128 a1)
		{
			return vqtbl1q_s8(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vqtbx1_s8(v64 a0, v128 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqtbx1q_s8(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqtbx1_u8(v64 a0, v128 a1, v64 a2)
		{
			return vqtbx1_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static v128 vqtbx1q_u8(v128 a0, v128 a1, v128 a2)
		{
			return vqtbx1q_s8(a0, a1, a2);
		}

		[DebuggerStepThrough]
		public static double vget_lane_f64(v64 a0, int a1)
		{
			return a0.Double0;
		}

		[DebuggerStepThrough]
		public static double vgetq_lane_f64(v128 a0, int a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vset_lane_f64(double a0, v64 a1, int a2)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vsetq_lane_f64(double a0, v128 a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static float vrecpxs_f32(float a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static double vrecpxd_f64(double a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfms_n_f32(v64 a0, v64 a1, float a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmsq_n_f32(v128 a0, v128 a1, float a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfma_n_f64(v64 a0, v64 a1, double a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmaq_n_f64(v128 a0, v128 a1, double a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vfms_n_f64(v64 a0, v64 a1, double a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vfmsq_n_f64(v128 a0, v128 a1, double a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsha1cq_u32(v128 a0, uint a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsha1pq_u32(v128 a0, uint a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsha1mq_u32(v128 a0, uint a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint vsha1h_u32(uint a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsha1su0q_u32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsha1su1q_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsha256hq_u32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsha256h2q_u32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsha256su0q_u32(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vsha256su1q_u32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint __crc32b(uint a0, byte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint __crc32h(uint a0, ushort a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint __crc32w(uint a0, uint a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint __crc32d(uint a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint __crc32cb(uint a0, byte a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint __crc32ch(uint a0, ushort a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint __crc32cw(uint a0, uint a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static uint __crc32cd(uint a0, ulong a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaeseq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaesdq_u8(v128 a0, v128 a1)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaesmcq_u8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vaesimcq_u8(v128 a0)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdot_u32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdot_s32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdotq_u32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdotq_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdot_lane_u32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdot_lane_s32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdotq_laneq_u32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdotq_laneq_s32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdot_laneq_u32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vdot_laneq_s32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdotq_lane_u32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vdotq_lane_s32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlah_s16(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlah_s32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlahq_s16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlahq_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlsh_s16(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlsh_s32(v64 a0, v64 a1, v64 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlshq_s16(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlshq_s32(v128 a0, v128 a1, v128 a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlah_lane_s16(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlahq_lane_s16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlah_laneq_s16(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlahq_laneq_s16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlah_lane_s32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlahq_lane_s32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlah_laneq_s32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlahq_laneq_s32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlsh_lane_s16(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlshq_lane_s16(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlsh_laneq_s16(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlshq_laneq_s16(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlsh_lane_s32(v64 a0, v64 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlshq_lane_s32(v128 a0, v128 a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vqrdmlsh_laneq_s32(v64 a0, v64 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v128 vqrdmlshq_laneq_s32(v128 a0, v128 a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrdmlahh_s16(short a0, short a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqrdmlahs_s32(int a0, int a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrdmlshh_s16(short a0, short a1, short a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqrdmlshs_s32(int a0, int a1, int a2)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrdmlahh_lane_s16(short a0, short a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrdmlahh_laneq_s16(short a0, short a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqrdmlahs_lane_s32(int a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrdmlshh_lane_s16(short a0, short a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static short vqrdmlshh_laneq_s16(short a0, short a1, v128 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static int vqrdmlshs_lane_s32(int a0, int a1, v64 a2, int a3)
		{
			throw new NotImplementedException();
		}

		[DebuggerStepThrough]
		public static v64 vcreate_s8(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcreate_s16(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcreate_s32(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcreate_s64(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcreate_u8(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcreate_u16(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcreate_u32(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcreate_u64(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcreate_f16(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcreate_f32(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vcreate_f64(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_n_s8(sbyte a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vdupq_n_s8(sbyte a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_n_s16(short a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vdupq_n_s16(short a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_n_s32(int a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vdupq_n_s32(int a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_n_s64(long a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vdupq_n_s64(long a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_n_u8(byte a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vdupq_n_u8(byte a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_n_u16(ushort a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vdupq_n_u16(ushort a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_n_u32(uint a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vdupq_n_u32(uint a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_n_u64(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vdupq_n_u64(ulong a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_n_f32(float a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vdupq_n_f32(float a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vdup_n_f64(double a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vdupq_n_f64(double a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmov_n_s8(sbyte a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovq_n_s8(sbyte a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmov_n_s16(short a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovq_n_s16(short a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmov_n_s32(int a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovq_n_s32(int a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmov_n_s64(long a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovq_n_s64(long a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmov_n_u8(byte a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovq_n_u8(byte a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmov_n_u16(ushort a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovq_n_u16(ushort a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmov_n_u32(uint a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovq_n_u32(uint a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmov_n_u64(ulong a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovq_n_u64(ulong a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmov_n_f32(float a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovq_n_f32(float a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v64 vmov_n_f64(double a0)
		{
			return new v64(a0);
		}

		[DebuggerStepThrough]
		public static v128 vmovq_n_f64(double a0)
		{
			return new v128(a0);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_s8(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_s16(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_s32(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_s64(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_u8(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_u16(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_u32(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_u64(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_f16(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_f32(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v128 vcombine_f64(v64 a0, v64 a1)
		{
			return new v128(a0, a1);
		}

		[DebuggerStepThrough]
		public static v64 vget_high_s8(v128 a0)
		{
			return a0.Hi64;
		}

		[DebuggerStepThrough]
		public static v64 vget_high_s16(v128 a0)
		{
			return a0.Hi64;
		}

		[DebuggerStepThrough]
		public static v64 vget_high_s32(v128 a0)
		{
			return a0.Hi64;
		}

		[DebuggerStepThrough]
		public static v64 vget_high_s64(v128 a0)
		{
			return a0.Hi64;
		}

		[DebuggerStepThrough]
		public static v64 vget_high_u8(v128 a0)
		{
			return a0.Hi64;
		}

		[DebuggerStepThrough]
		public static v64 vget_high_u16(v128 a0)
		{
			return a0.Hi64;
		}

		[DebuggerStepThrough]
		public static v64 vget_high_u32(v128 a0)
		{
			return a0.Hi64;
		}

		[DebuggerStepThrough]
		public static v64 vget_high_u64(v128 a0)
		{
			return a0.Hi64;
		}

		[DebuggerStepThrough]
		public static v64 vget_high_f32(v128 a0)
		{
			return a0.Hi64;
		}

		[DebuggerStepThrough]
		public static v64 vget_high_f64(v128 a0)
		{
			return a0.Hi64;
		}

		[DebuggerStepThrough]
		public static v64 vget_low_s8(v128 a0)
		{
			return a0.Lo64;
		}

		[DebuggerStepThrough]
		public static v64 vget_low_s16(v128 a0)
		{
			return a0.Lo64;
		}

		[DebuggerStepThrough]
		public static v64 vget_low_s32(v128 a0)
		{
			return a0.Lo64;
		}

		[DebuggerStepThrough]
		public static v64 vget_low_s64(v128 a0)
		{
			return a0.Lo64;
		}

		[DebuggerStepThrough]
		public static v64 vget_low_u8(v128 a0)
		{
			return a0.Lo64;
		}

		[DebuggerStepThrough]
		public static v64 vget_low_u16(v128 a0)
		{
			return a0.Lo64;
		}

		[DebuggerStepThrough]
		public static v64 vget_low_u32(v128 a0)
		{
			return a0.Lo64;
		}

		[DebuggerStepThrough]
		public static v64 vget_low_u64(v128 a0)
		{
			return a0.Lo64;
		}

		[DebuggerStepThrough]
		public static v64 vget_low_f32(v128 a0)
		{
			return a0.Lo64;
		}

		[DebuggerStepThrough]
		public static v64 vget_low_f64(v128 a0)
		{
			return a0.Lo64;
		}

		[DebuggerStepThrough]
		public unsafe static v64 vld1_s8(sbyte* a0)
		{
			return *(v64*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 vld1q_s8(sbyte* a0)
		{
			return *(v128*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v64 vld1_s16(short* a0)
		{
			return *(v64*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 vld1q_s16(short* a0)
		{
			return *(v128*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v64 vld1_s32(int* a0)
		{
			return *(v64*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 vld1q_s32(int* a0)
		{
			return *(v128*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v64 vld1_s64(long* a0)
		{
			return *(v64*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 vld1q_s64(long* a0)
		{
			return *(v128*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v64 vld1_u8(byte* a0)
		{
			return *(v64*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 vld1q_u8(byte* a0)
		{
			return *(v128*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v64 vld1_u16(ushort* a0)
		{
			return *(v64*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 vld1q_u16(ushort* a0)
		{
			return *(v128*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v64 vld1_u32(uint* a0)
		{
			return *(v64*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 vld1q_u32(uint* a0)
		{
			return *(v128*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v64 vld1_u64(ulong* a0)
		{
			return *(v64*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 vld1q_u64(ulong* a0)
		{
			return *(v128*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v64 vld1_f32(float* a0)
		{
			return *(v64*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 vld1q_f32(float* a0)
		{
			return *(v128*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v64 vld1_f64(double* a0)
		{
			return *(v64*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static v128 vld1q_f64(double* a0)
		{
			return *(v128*)a0;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1_s8(sbyte* a0, v64 a1)
		{
			*(v64*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1q_s8(sbyte* a0, v128 a1)
		{
			*(v128*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1_s16(short* a0, v64 a1)
		{
			*(v64*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1q_s16(short* a0, v128 a1)
		{
			*(v128*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1_s32(int* a0, v64 a1)
		{
			*(v64*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1q_s32(int* a0, v128 a1)
		{
			*(v128*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1_s64(long* a0, v64 a1)
		{
			*(v64*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1q_s64(long* a0, v128 a1)
		{
			*(v128*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1_u8(byte* a0, v64 a1)
		{
			*(v64*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1q_u8(byte* a0, v128 a1)
		{
			*(v128*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1_u16(ushort* a0, v64 a1)
		{
			*(v64*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1q_u16(ushort* a0, v128 a1)
		{
			*(v128*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1_u32(uint* a0, v64 a1)
		{
			*(v64*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1q_u32(uint* a0, v128 a1)
		{
			*(v128*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1_u64(ulong* a0, v64 a1)
		{
			*(v64*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1q_u64(ulong* a0, v128 a1)
		{
			*(v128*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1_f32(float* a0, v64 a1)
		{
			*(v64*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1q_f32(float* a0, v128 a1)
		{
			*(v128*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1_f64(double* a0, v64 a1)
		{
			*(v64*)a0 = a1;
		}

		[DebuggerStepThrough]
		public unsafe static void vst1q_f64(double* a0, v128 a1)
		{
			*(v128*)a0 = a1;
		}
	}
}
