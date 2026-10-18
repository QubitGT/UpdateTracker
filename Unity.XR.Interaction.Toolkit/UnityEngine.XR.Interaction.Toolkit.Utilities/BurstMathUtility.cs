using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

[BurstCompile]
public static class BurstMathUtility
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OrthogonalUpVector_0000034D_0024PostfixBurstDelegate(in Vector3 forward, in Vector3 referenceUp, out Vector3 orthogonalUp);

	internal static class OrthogonalUpVector_0000034D_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<OrthogonalUpVector_0000034D_0024PostfixBurstDelegate>(OrthogonalUpVector).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 forward, in Vector3 referenceUp, out Vector3 orthogonalUp)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, ref Vector3, void>)functionPointer)(ref forward, ref referenceUp, ref orthogonalUp);
					return;
				}
			}
			OrthogonalUpVector_0024BurstManaged(in forward, in referenceUp, out orthogonalUp);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OrthogonalUpVector_0000034E_0024PostfixBurstDelegate(in float3 forward, in float3 referenceUp, out float3 orthogonalUp);

	internal static class OrthogonalUpVector_0000034E_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<OrthogonalUpVector_0000034E_0024PostfixBurstDelegate>(OrthogonalUpVector).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 forward, in float3 referenceUp, out float3 orthogonalUp)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, void>)functionPointer)(ref forward, ref referenceUp, ref orthogonalUp);
					return;
				}
			}
			OrthogonalUpVector_0024BurstManaged(in forward, in referenceUp, out orthogonalUp);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OrthogonalLookRotation_0000034F_0024PostfixBurstDelegate(in Vector3 forward, in Vector3 referenceUp, out Quaternion lookRotation);

	internal static class OrthogonalLookRotation_0000034F_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<OrthogonalLookRotation_0000034F_0024PostfixBurstDelegate>(OrthogonalLookRotation).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 forward, in Vector3 referenceUp, out Quaternion lookRotation)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, ref Quaternion, void>)functionPointer)(ref forward, ref referenceUp, ref lookRotation);
					return;
				}
			}
			OrthogonalLookRotation_0024BurstManaged(in forward, in referenceUp, out lookRotation);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void OrthogonalLookRotation_00000350_0024PostfixBurstDelegate(in float3 forward, in float3 referenceUp, out quaternion lookRotation);

	internal static class OrthogonalLookRotation_00000350_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<OrthogonalLookRotation_00000350_0024PostfixBurstDelegate>(OrthogonalLookRotation).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 forward, in float3 referenceUp, out quaternion lookRotation)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref quaternion, void>)functionPointer)(ref forward, ref referenceUp, ref lookRotation);
					return;
				}
			}
			OrthogonalLookRotation_0024BurstManaged(in forward, in referenceUp, out lookRotation);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ProjectOnPlane_00000351_0024PostfixBurstDelegate(in float3 vector, in float3 planeNormal, out float3 projectedVector);

	internal static class ProjectOnPlane_00000351_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ProjectOnPlane_00000351_0024PostfixBurstDelegate>(ProjectOnPlane).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 vector, in float3 planeNormal, out float3 projectedVector)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, void>)functionPointer)(ref vector, ref planeNormal, ref projectedVector);
					return;
				}
			}
			ProjectOnPlane_0024BurstManaged(in vector, in planeNormal, out projectedVector);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ProjectOnPlane_00000352_0024PostfixBurstDelegate(in Vector3 vector, in Vector3 planeNormal, out Vector3 projectedVector);

	internal static class ProjectOnPlane_00000352_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ProjectOnPlane_00000352_0024PostfixBurstDelegate>(ProjectOnPlane).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 vector, in Vector3 planeNormal, out Vector3 projectedVector)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, ref Vector3, void>)functionPointer)(ref vector, ref planeNormal, ref projectedVector);
					return;
				}
			}
			ProjectOnPlane_0024BurstManaged(in vector, in planeNormal, out projectedVector);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void LookRotationWithForwardProjectedOnPlane_00000353_0024PostfixBurstDelegate(in float3 forward, in float3 planeNormal, out quaternion lookRotation);

	internal static class LookRotationWithForwardProjectedOnPlane_00000353_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<LookRotationWithForwardProjectedOnPlane_00000353_0024PostfixBurstDelegate>(LookRotationWithForwardProjectedOnPlane).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 forward, in float3 planeNormal, out quaternion lookRotation)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref quaternion, void>)functionPointer)(ref forward, ref planeNormal, ref lookRotation);
					return;
				}
			}
			LookRotationWithForwardProjectedOnPlane_0024BurstManaged(in forward, in planeNormal, out lookRotation);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void LookRotationWithForwardProjectedOnPlane_00000354_0024PostfixBurstDelegate(in Vector3 forward, in Vector3 planeNormal, out Quaternion lookRotation);

	internal static class LookRotationWithForwardProjectedOnPlane_00000354_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<LookRotationWithForwardProjectedOnPlane_00000354_0024PostfixBurstDelegate>(LookRotationWithForwardProjectedOnPlane).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 forward, in Vector3 planeNormal, out Quaternion lookRotation)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, ref Quaternion, void>)functionPointer)(ref forward, ref planeNormal, ref lookRotation);
					return;
				}
			}
			LookRotationWithForwardProjectedOnPlane_0024BurstManaged(in forward, in planeNormal, out lookRotation);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void Angle_00000355_0024PostfixBurstDelegate(in quaternion a, in quaternion b, out float angle);

	internal static class Angle_00000355_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Angle_00000355_0024PostfixBurstDelegate>(Angle).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in quaternion a, in quaternion b, out float angle)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref quaternion, ref quaternion, ref float, void>)functionPointer)(ref a, ref b, ref angle);
					return;
				}
			}
			Angle_0024BurstManaged(in a, in b, out angle);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void Angle_00000356_0024PostfixBurstDelegate(in Vector3 a, in Vector3 b, out float angle);

	internal static class Angle_00000356_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Angle_00000356_0024PostfixBurstDelegate>(BurstMathUtility.Angle).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 a, in Vector3 b, out float angle)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, ref float, void>)functionPointer)(ref a, ref b, ref angle);
					return;
				}
			}
			Angle_0024BurstManaged(in a, in b, out angle);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool FastVectorEquals_00000357_0024PostfixBurstDelegate(in float3 a, in float3 b, float tolerance = 0.0001f);

	internal static class FastVectorEquals_00000357_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FastVectorEquals_00000357_0024PostfixBurstDelegate>(FastVectorEquals).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(in float3 a, in float3 b, float tolerance = 0.0001f)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref float3, ref float3, float, bool>)functionPointer)(ref a, ref b, tolerance);
				}
			}
			return FastVectorEquals_0024BurstManaged(in a, in b, tolerance);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool FastVectorEquals_00000358_0024PostfixBurstDelegate(in Vector3 a, in Vector3 b, float tolerance = 0.0001f);

	internal static class FastVectorEquals_00000358_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FastVectorEquals_00000358_0024PostfixBurstDelegate>(BurstMathUtility.FastVectorEquals).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(in Vector3 a, in Vector3 b, float tolerance = 0.0001f)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, float, bool>)functionPointer)(ref a, ref b, tolerance);
				}
			}
			return FastVectorEquals_0024BurstManaged(in a, in b, tolerance);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void FastSafeDivide_00000359_0024PostfixBurstDelegate(in Vector3 a, in Vector3 b, out Vector3 result, float tolerance = 1E-06f);

	internal static class FastSafeDivide_00000359_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FastSafeDivide_00000359_0024PostfixBurstDelegate>(FastSafeDivide).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 a, in Vector3 b, out Vector3 result, float tolerance = 1E-06f)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, ref Vector3, float, void>)functionPointer)(ref a, ref b, ref result, tolerance);
					return;
				}
			}
			FastSafeDivide_0024BurstManaged(in a, in b, out result, tolerance);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void FastSafeDivide_0000035A_0024PostfixBurstDelegate(in float3 a, in float3 b, out float3 result, float tolerance = 1E-06f);

	internal static class FastSafeDivide_0000035A_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FastSafeDivide_0000035A_0024PostfixBurstDelegate>(FastSafeDivide).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 a, in float3 b, out float3 result, float tolerance = 1E-06f)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, float, void>)functionPointer)(ref a, ref b, ref result, tolerance);
					return;
				}
			}
			FastSafeDivide_0024BurstManaged(in a, in b, out result, tolerance);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void Scale_0000035B_0024PostfixBurstDelegate(in float3 a, in float3 b, out float3 result);

	internal static class Scale_0000035B_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Scale_0000035B_0024PostfixBurstDelegate>(Scale).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 a, in float3 b, out float3 result)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, void>)functionPointer)(ref a, ref b, ref result);
					return;
				}
			}
			Scale_0024BurstManaged(in a, in b, out result);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void Scale_0000035C_0024PostfixBurstDelegate(in Vector3 a, in Vector3 b, out Vector3 result);

	internal static class Scale_0000035C_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Scale_0000035C_0024PostfixBurstDelegate>(Scale).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 a, in Vector3 b, out Vector3 result)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, ref Vector3, void>)functionPointer)(ref a, ref b, ref result);
					return;
				}
			}
			Scale_0024BurstManaged(in a, in b, out result);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void Orthogonal_0000035E_0024PostfixBurstDelegate(in float3 input, out float3 result);

	internal static class Orthogonal_0000035E_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Orthogonal_0000035E_0024PostfixBurstDelegate>(Orthogonal).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 input, out float3 result)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, void>)functionPointer)(ref input, ref result);
					return;
				}
			}
			Orthogonal_0024BurstManaged(in input, out result);
		}
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EOrthogonalUpVector_0000034D_0024PostfixBurstDelegate))]
	public static void OrthogonalUpVector(in Vector3 forward, in Vector3 referenceUp, out Vector3 orthogonalUp)
	{
		OrthogonalUpVector_0000034D_0024BurstDirectCall.Invoke(in forward, in referenceUp, out orthogonalUp);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EOrthogonalUpVector_0000034E_0024PostfixBurstDelegate))]
	public static void OrthogonalUpVector(in float3 forward, in float3 referenceUp, out float3 orthogonalUp)
	{
		OrthogonalUpVector_0000034E_0024BurstDirectCall.Invoke(in forward, in referenceUp, out orthogonalUp);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EOrthogonalLookRotation_0000034F_0024PostfixBurstDelegate))]
	public static void OrthogonalLookRotation(in Vector3 forward, in Vector3 referenceUp, out Quaternion lookRotation)
	{
		OrthogonalLookRotation_0000034F_0024BurstDirectCall.Invoke(in forward, in referenceUp, out lookRotation);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EOrthogonalLookRotation_00000350_0024PostfixBurstDelegate))]
	public static void OrthogonalLookRotation(in float3 forward, in float3 referenceUp, out quaternion lookRotation)
	{
		OrthogonalLookRotation_00000350_0024BurstDirectCall.Invoke(in forward, in referenceUp, out lookRotation);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EProjectOnPlane_00000351_0024PostfixBurstDelegate))]
	public static void ProjectOnPlane(in float3 vector, in float3 planeNormal, out float3 projectedVector)
	{
		ProjectOnPlane_00000351_0024BurstDirectCall.Invoke(in vector, in planeNormal, out projectedVector);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EProjectOnPlane_00000352_0024PostfixBurstDelegate))]
	public static void ProjectOnPlane(in Vector3 vector, in Vector3 planeNormal, out Vector3 projectedVector)
	{
		ProjectOnPlane_00000352_0024BurstDirectCall.Invoke(in vector, in planeNormal, out projectedVector);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ELookRotationWithForwardProjectedOnPlane_00000353_0024PostfixBurstDelegate))]
	public static void LookRotationWithForwardProjectedOnPlane(in float3 forward, in float3 planeNormal, out quaternion lookRotation)
	{
		LookRotationWithForwardProjectedOnPlane_00000353_0024BurstDirectCall.Invoke(in forward, in planeNormal, out lookRotation);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002ELookRotationWithForwardProjectedOnPlane_00000354_0024PostfixBurstDelegate))]
	public static void LookRotationWithForwardProjectedOnPlane(in Vector3 forward, in Vector3 planeNormal, out Quaternion lookRotation)
	{
		LookRotationWithForwardProjectedOnPlane_00000354_0024BurstDirectCall.Invoke(in forward, in planeNormal, out lookRotation);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EAngle_00000355_0024PostfixBurstDelegate))]
	public static void Angle(in quaternion a, in quaternion b, out float angle)
	{
		Angle_00000355_0024BurstDirectCall.Invoke(in a, in b, out angle);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EAngle_00000356_0024PostfixBurstDelegate))]
	public static void Angle(in Vector3 a, in Vector3 b, out float angle)
	{
		Angle_00000356_0024BurstDirectCall.Invoke(in a, in b, out angle);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EFastVectorEquals_00000357_0024PostfixBurstDelegate))]
	public static bool FastVectorEquals(in float3 a, in float3 b, float tolerance = 0.0001f)
	{
		return FastVectorEquals_00000357_0024BurstDirectCall.Invoke(in a, in b, tolerance);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EFastVectorEquals_00000358_0024PostfixBurstDelegate))]
	public static bool FastVectorEquals(in Vector3 a, in Vector3 b, float tolerance = 0.0001f)
	{
		return FastVectorEquals_00000358_0024BurstDirectCall.Invoke(in a, in b, tolerance);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EFastSafeDivide_00000359_0024PostfixBurstDelegate))]
	public static void FastSafeDivide(in Vector3 a, in Vector3 b, out Vector3 result, float tolerance = 1E-06f)
	{
		FastSafeDivide_00000359_0024BurstDirectCall.Invoke(in a, in b, out result, tolerance);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EFastSafeDivide_0000035A_0024PostfixBurstDelegate))]
	public static void FastSafeDivide(in float3 a, in float3 b, out float3 result, float tolerance = 1E-06f)
	{
		FastSafeDivide_0000035A_0024BurstDirectCall.Invoke(in a, in b, out result, tolerance);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EScale_0000035B_0024PostfixBurstDelegate))]
	public static void Scale(in float3 a, in float3 b, out float3 result)
	{
		Scale_0000035B_0024BurstDirectCall.Invoke(in a, in b, out result);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EScale_0000035C_0024PostfixBurstDelegate))]
	public static void Scale(in Vector3 a, in Vector3 b, out Vector3 result)
	{
		Scale_0000035C_0024BurstDirectCall.Invoke(in a, in b, out result);
	}

	public static Vector3 Orthogonal(Vector3 input)
	{
		float3 input2 = input;
		Orthogonal(in input2, out var result);
		return result;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EUtilities_002EOrthogonal_0000035E_0024PostfixBurstDelegate))]
	public static void Orthogonal(in float3 input, out float3 result)
	{
		Orthogonal_0000035E_0024BurstDirectCall.Invoke(in input, out result);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void OrthogonalUpVector_0024BurstManaged(in Vector3 forward, in Vector3 referenceUp, out Vector3 orthogonalUp)
	{
		float3 forward2 = forward;
		float3 referenceUp2 = referenceUp;
		OrthogonalUpVector(in forward2, in referenceUp2, out var orthogonalUp2);
		orthogonalUp = orthogonalUp2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void OrthogonalUpVector_0024BurstManaged(in float3 forward, in float3 referenceUp, out float3 orthogonalUp)
	{
		float3 y = -math.cross(forward, referenceUp);
		orthogonalUp = math.cross(forward, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void OrthogonalLookRotation_0024BurstManaged(in Vector3 forward, in Vector3 referenceUp, out Quaternion lookRotation)
	{
		float3 forward2 = forward;
		float3 referenceUp2 = referenceUp;
		OrthogonalLookRotation(in forward2, in referenceUp2, out var lookRotation2);
		lookRotation = lookRotation2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void OrthogonalLookRotation_0024BurstManaged(in float3 forward, in float3 referenceUp, out quaternion lookRotation)
	{
		OrthogonalUpVector(in forward, in referenceUp, out var orthogonalUp);
		lookRotation = quaternion.LookRotation(forward, orthogonalUp);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ProjectOnPlane_0024BurstManaged(in float3 vector, in float3 planeNormal, out float3 projectedVector)
	{
		float num = math.dot(planeNormal, planeNormal);
		if (num < 1.1920929E-07f)
		{
			projectedVector = vector;
			return;
		}
		float num2 = math.dot(vector, planeNormal);
		projectedVector = new float3(vector.x - planeNormal.x * num2 / num, vector.y - planeNormal.y * num2 / num, vector.z - planeNormal.z * num2 / num);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void ProjectOnPlane_0024BurstManaged(in Vector3 vector, in Vector3 planeNormal, out Vector3 projectedVector)
	{
		float3 vector2 = vector;
		float3 planeNormal2 = planeNormal;
		ProjectOnPlane(in vector2, in planeNormal2, out var projectedVector2);
		projectedVector = projectedVector2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void LookRotationWithForwardProjectedOnPlane_0024BurstManaged(in float3 forward, in float3 planeNormal, out quaternion lookRotation)
	{
		ProjectOnPlane(in forward, in planeNormal, out var projectedVector);
		lookRotation = quaternion.LookRotation(projectedVector, planeNormal);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void LookRotationWithForwardProjectedOnPlane_0024BurstManaged(in Vector3 forward, in Vector3 planeNormal, out Quaternion lookRotation)
	{
		float3 forward2 = forward;
		float3 planeNormal2 = planeNormal;
		LookRotationWithForwardProjectedOnPlane(in forward2, in planeNormal2, out var lookRotation2);
		lookRotation = lookRotation2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Angle_0024BurstManaged(in quaternion a, in quaternion b, out float angle)
	{
		float num = math.min(math.abs(math.dot(a, b)), 1f);
		angle = ((num > 0.999999f) ? 0f : (math.acos(num) * 2f * 57.29578f));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Angle_0024BurstManaged(in Vector3 a, in Vector3 b, out float angle)
	{
		float num = math.sqrt(a.sqrMagnitude * b.sqrMagnitude);
		if (num < 1E-15f)
		{
			angle = 0f;
			return;
		}
		float x = math.clamp(math.dot(a, b) / num, -1f, 1f);
		angle = math.acos(x) * 57.29578f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool FastVectorEquals_0024BurstManaged(in float3 a, in float3 b, float tolerance = 0.0001f)
	{
		if (math.abs(a.x - b.x) < tolerance && math.abs(a.y - b.y) < tolerance)
		{
			return math.abs(a.z - b.z) < tolerance;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool FastVectorEquals_0024BurstManaged(in Vector3 a, in Vector3 b, float tolerance = 0.0001f)
	{
		if (math.abs(a.x - b.x) < tolerance && math.abs(a.y - b.y) < tolerance)
		{
			return math.abs(a.z - b.z) < tolerance;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void FastSafeDivide_0024BurstManaged(in Vector3 a, in Vector3 b, out Vector3 result, float tolerance = 1E-06f)
	{
		float3 a2 = a;
		float3 b2 = b;
		FastSafeDivide(in a2, in b2, out var result2, tolerance);
		result = result2;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void FastSafeDivide_0024BurstManaged(in float3 a, in float3 b, out float3 result, float tolerance = 1E-06f)
	{
		result = default(float3);
		if (math.abs(a.x - b.x) > tolerance)
		{
			result.x = a.x / b.x;
		}
		if (math.abs(a.y - b.y) > tolerance)
		{
			result.y = a.y / b.y;
		}
		if (math.abs(a.z - b.z) > tolerance)
		{
			result.z = a.z / b.z;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Scale_0024BurstManaged(in float3 a, in float3 b, out float3 result)
	{
		result = new float3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Scale_0024BurstManaged(in Vector3 a, in Vector3 b, out Vector3 result)
	{
		result = new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void Orthogonal_0024BurstManaged(in float3 input, out float3 result)
	{
		if (math.abs(input.x) < math.abs(input.y) && math.abs(input.x) < math.abs(input.z))
		{
			result = math.cross(input, new float3(1f, 0f, 0f));
		}
		else if (math.abs(input.y) < math.abs(input.z))
		{
			result = math.cross(input, new float3(0f, 1f, 0f));
		}
		else
		{
			result = math.cross(input, new float3(0f, 0f, 1f));
		}
	}
}
