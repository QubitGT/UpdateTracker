using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Transformers;

[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Transformers.XRSocketGrabTransformer.html")]
[BurstCompile]
public class XRSocketGrabTransformer : IXRGrabTransformer
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate float FastCalculateRadiusOffset_0000092E_0024PostfixBurstDelegate(in float3 initialScale, in float3 targetScale, in float3 initialBoundsSize, float innerRadius);

	internal static class FastCalculateRadiusOffset_0000092E_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FastCalculateRadiusOffset_0000092E_0024PostfixBurstDelegate>(FastCalculateRadiusOffset).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static float Invoke(in float3 initialScale, in float3 targetScale, in float3 initialBoundsSize, float innerRadius)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, float, float>)functionPointer)(ref initialScale, ref targetScale, ref initialBoundsSize, innerRadius);
				}
			}
			return FastCalculateRadiusOffset_0024BurstManaged(in initialScale, in targetScale, in initialBoundsSize, innerRadius);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void FastComputeNewTrackedPose_0000092F_0024PostfixBurstDelegate(in float3 interactorAttachPos, in quaternion interactorAttachRot, in float3 positionOffset, in quaternion interactableRot, in quaternion interactableAttachRot, out float3 targetPos, out quaternion targetRot);

	internal static class FastComputeNewTrackedPose_0000092F_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FastComputeNewTrackedPose_0000092F_0024PostfixBurstDelegate>(FastComputeNewTrackedPose).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 interactorAttachPos, in quaternion interactorAttachRot, in float3 positionOffset, in quaternion interactableRot, in quaternion interactableAttachRot, out float3 targetPos, out quaternion targetRot)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref quaternion, ref float3, ref quaternion, ref quaternion, ref float3, ref quaternion, void>)functionPointer)(ref interactorAttachPos, ref interactorAttachRot, ref positionOffset, ref interactableRot, ref interactableAttachRot, ref targetPos, ref targetRot);
					return;
				}
			}
			FastComputeNewTrackedPose_0024BurstManaged(in interactorAttachPos, in interactorAttachRot, in positionOffset, in interactableRot, in interactableAttachRot, out targetPos, out targetRot);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool IsWithinRadius_00000930_0024PostfixBurstDelegate(in float3 a, in float3 b, float radius);

	internal static class IsWithinRadius_00000930_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<IsWithinRadius_00000930_0024PostfixBurstDelegate>(IsWithinRadius).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static bool Invoke(in float3 a, in float3 b, float radius)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref float3, ref float3, float, bool>)functionPointer)(ref a, ref b, radius);
				}
			}
			return IsWithinRadius_0024BurstManaged(in a, in b, radius);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CalculateScaleToFit_00000931_0024PostfixBurstDelegate(in float3 boundsSize, in float3 fixedSize, in float3 initialScale, float epsilon, out float3 newScale);

	internal static class CalculateScaleToFit_00000931_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CalculateScaleToFit_00000931_0024PostfixBurstDelegate>(CalculateScaleToFit).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in float3 boundsSize, in float3 fixedSize, in float3 initialScale, float epsilon, out float3 newScale)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref float3, ref float3, ref float3, float, ref float3, void>)functionPointer)(ref boundsSize, ref fixedSize, ref initialScale, epsilon, ref newScale);
					return;
				}
			}
			CalculateScaleToFit_0024BurstManaged(in boundsSize, in fixedSize, in initialScale, epsilon, out newScale);
		}
	}

	private const float k_SocketSnappingAxisTolerance = 0.01f;

	private readonly Dictionary<IXRInteractable, float3> m_InitialScale = new Dictionary<IXRInteractable, float3>();

	private readonly Dictionary<IXRInteractable, float3> m_InteractableBoundsSize = new Dictionary<IXRInteractable, float3>();

	public bool canProcess { get; set; } = true;


	public float socketSnappingRadius { get; set; }

	public SocketScaleMode scaleMode { get; set; }

	internal bool scaleOnlyMode { get; set; }

	public float3 fixedScale { get; set; } = new float3(1f, 1f, 1f);


	public float3 targetBoundsSize { get; set; } = new float3(1f, 1f, 1f);


	public IXRInteractor socketInteractor { get; set; }

	public void OnLink(XRGrabInteractable grabInteractable)
	{
	}

	public void OnGrab(XRGrabInteractable grabInteractable)
	{
	}

	public void OnGrabCountChanged(XRGrabInteractable grabInteractable, Pose targetPose, Vector3 localScale)
	{
		RegisterInteractableScale(grabInteractable, localScale);
	}

	public void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale)
	{
		if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic && updatePhase != XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender)
		{
			return;
		}
		if (scaleMode == SocketScaleMode.None)
		{
			if (!scaleOnlyMode)
			{
				UpdateTargetWithoutScale(grabInteractable, socketInteractor, socketSnappingRadius, ref targetPose);
			}
			return;
		}
		float3 initialInteractableScale = m_InitialScale[grabInteractable];
		float3 targetScale = ComputeSocketTargetScale(grabInteractable, in initialInteractableScale);
		if (!scaleOnlyMode)
		{
			float3 initialBounds = m_InteractableBoundsSize[grabInteractable];
			UpdateTargetWithScale(grabInteractable, socketInteractor, socketSnappingRadius, in initialInteractableScale, in initialBounds, in targetScale, ref targetPose, ref localScale);
		}
		else
		{
			localScale = targetScale;
		}
	}

	private static void UpdateTargetWithoutScale(XRGrabInteractable grabInteractable, IXRInteractor interactor, float snappingRadius, ref Pose targetPose)
	{
		if (!GetTargetPoseForInteractable(grabInteractable, interactor, out var targetPose2))
		{
			return;
		}
		if (!(interactor is IXRSelectInteractor iXRSelectInteractor) || !iXRSelectInteractor.IsSelecting(grabInteractable))
		{
			float3 a = targetPose.position;
			float3 b = targetPose2.position;
			if (!IsWithinRadius(in a, in b, snappingRadius))
			{
				return;
			}
		}
		targetPose = targetPose2;
	}

	private static void UpdateTargetWithScale(XRGrabInteractable grabInteractable, IXRInteractor interactor, float innerRadius, in float3 initialScale, in float3 initialBounds, in float3 targetScale, ref Pose targetPose, ref Vector3 localScale)
	{
		if (!GetTargetPoseForInteractable(grabInteractable, interactor, out var targetPose2))
		{
			return;
		}
		Vector3 a = grabInteractable.transform.position;
		bool flag = BurstMathUtility.FastVectorEquals(in a, in targetPose2.position, 0.01f);
		float num = FastCalculateRadiusOffset(in initialScale, in targetScale, in initialBounds, innerRadius);
		float radius = (flag ? num : innerRadius);
		if (!(interactor is IXRSelectInteractor iXRSelectInteractor) || !iXRSelectInteractor.IsSelecting(grabInteractable))
		{
			float3 a2 = targetPose.position;
			float3 b = targetPose2.position;
			if (!IsWithinRadius(in a2, in b, radius))
			{
				localScale = initialScale;
				return;
			}
		}
		targetPose = targetPose2;
		if (flag)
		{
			localScale = targetScale;
		}
	}

	public void OnUnlink(XRGrabInteractable grabInteractable)
	{
		if (m_InitialScale.TryGetValue(grabInteractable, out var value))
		{
			grabInteractable.SetTargetLocalScale(value);
			m_InitialScale.Remove(grabInteractable);
			m_InteractableBoundsSize.Remove(grabInteractable);
		}
	}

	private bool RegisterInteractableScale(IXRInteractable targetInteractable, Vector3 scale)
	{
		if (!m_InitialScale.TryAdd(targetInteractable, scale))
		{
			return false;
		}
		Transform transform = targetInteractable.transform;
		Pose worldPose = transform.GetWorldPose();
		transform.SetWorldPose(Pose.identity);
		m_InteractableBoundsSize[targetInteractable] = BoundsUtils.GetBounds(targetInteractable.transform).size;
		transform.SetWorldPose(worldPose);
		return true;
	}

	private float3 ComputeSocketTargetScale(IXRInteractable interactable, in float3 initialInteractableScale)
	{
		switch (scaleMode)
		{
		case SocketScaleMode.Fixed:
		{
			float3 fixedSize = fixedScale;
			BurstMathUtility.Scale(in initialInteractableScale, in fixedSize, out var result);
			return result;
		}
		case SocketScaleMode.StretchedToFitSize:
		{
			if (!m_InteractableBoundsSize.TryGetValue(interactable, out var value))
			{
				return initialInteractableScale;
			}
			float3 fixedSize = targetBoundsSize;
			CalculateScaleToFit(in value, in fixedSize, in initialInteractableScale, Mathf.Epsilon, out var newScale);
			return newScale;
		}
		default:
			return initialInteractableScale;
		}
	}

	private static bool GetTargetPoseForInteractable(IXRInteractable interactable, IXRInteractor interactor, out Pose targetPose)
	{
		targetPose = Pose.identity;
		XRGrabInteractable xRGrabInteractable = interactable as XRGrabInteractable;
		if (xRGrabInteractable == null)
		{
			return false;
		}
		Transform attachTransform = interactor.GetAttachTransform(xRGrabInteractable);
		Transform transform = xRGrabInteractable.transform;
		Transform attachTransform2 = xRGrabInteractable.GetAttachTransform(interactor);
		Vector3 vector = transform.position - attachTransform2.position;
		if (xRGrabInteractable.trackRotation)
		{
			Vector3 vector2 = attachTransform2.InverseTransformDirection(vector);
			float3 interactorAttachPos = attachTransform.position;
			quaternion interactorAttachRot = attachTransform.rotation;
			float3 positionOffset = vector2;
			quaternion interactableRot = transform.rotation;
			quaternion interactableAttachRot = attachTransform2.rotation;
			FastComputeNewTrackedPose(in interactorAttachPos, in interactorAttachRot, in positionOffset, in interactableRot, in interactableAttachRot, out var targetPos, out var targetRot);
			targetPose.position = targetPos;
			targetPose.rotation = targetRot;
		}
		else
		{
			targetPose.position = vector + attachTransform.position;
		}
		return true;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002ETransformers_002EFastCalculateRadiusOffset_0000092E_0024PostfixBurstDelegate))]
	private static float FastCalculateRadiusOffset(in float3 initialScale, in float3 targetScale, in float3 initialBoundsSize, float innerRadius)
	{
		return FastCalculateRadiusOffset_0000092E_0024BurstDirectCall.Invoke(in initialScale, in targetScale, in initialBoundsSize, innerRadius);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002ETransformers_002EFastComputeNewTrackedPose_0000092F_0024PostfixBurstDelegate))]
	private static void FastComputeNewTrackedPose(in float3 interactorAttachPos, in quaternion interactorAttachRot, in float3 positionOffset, in quaternion interactableRot, in quaternion interactableAttachRot, out float3 targetPos, out quaternion targetRot)
	{
		FastComputeNewTrackedPose_0000092F_0024BurstDirectCall.Invoke(in interactorAttachPos, in interactorAttachRot, in positionOffset, in interactableRot, in interactableAttachRot, out targetPos, out targetRot);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002ETransformers_002EIsWithinRadius_00000930_0024PostfixBurstDelegate))]
	private static bool IsWithinRadius(in float3 a, in float3 b, float radius)
	{
		return IsWithinRadius_00000930_0024BurstDirectCall.Invoke(in a, in b, radius);
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002ETransformers_002ECalculateScaleToFit_00000931_0024PostfixBurstDelegate))]
	private static void CalculateScaleToFit(in float3 boundsSize, in float3 fixedSize, in float3 initialScale, float epsilon, out float3 newScale)
	{
		CalculateScaleToFit_00000931_0024BurstDirectCall.Invoke(in boundsSize, in fixedSize, in initialScale, epsilon, out newScale);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static float FastCalculateRadiusOffset_0024BurstManaged(in float3 initialScale, in float3 targetScale, in float3 initialBoundsSize, float innerRadius)
	{
		float x = math.max(math.max(initialBoundsSize.x, initialBoundsSize.y), initialBoundsSize.z);
		BurstMathUtility.FastSafeDivide(in targetScale, in initialScale, out var result);
		float x2 = result.x * initialBoundsSize.x;
		float y = result.y * initialBoundsSize.y;
		float y3 = math.max(y: result.z * initialBoundsSize.z, x: math.max(x2, y));
		float num = math.max(x, y3);
		return innerRadius + num / 2f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void FastComputeNewTrackedPose_0024BurstManaged(in float3 interactorAttachPos, in quaternion interactorAttachRot, in float3 positionOffset, in quaternion interactableRot, in quaternion interactableAttachRot, out float3 targetPos, out quaternion targetRot)
	{
		quaternion b = math.inverse(math.mul(math.inverse(interactableRot), interactableAttachRot));
		targetPos = math.mul(interactorAttachRot, positionOffset) + interactorAttachPos;
		targetRot = math.mul(interactorAttachRot, b);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static bool IsWithinRadius_0024BurstManaged(in float3 a, in float3 b, float radius)
	{
		return math.lengthsq(a - b) < radius * radius;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void CalculateScaleToFit_0024BurstManaged(in float3 boundsSize, in float3 fixedSize, in float3 initialScale, float epsilon, out float3 newScale)
	{
		float x = boundsSize.x / (fixedSize.x + epsilon);
		float y = boundsSize.y / (fixedSize.y + epsilon);
		float num = math.max(y: boundsSize.z / (fixedSize.z + epsilon), x: math.max(x, y));
		newScale = initialScale / num;
	}
}
