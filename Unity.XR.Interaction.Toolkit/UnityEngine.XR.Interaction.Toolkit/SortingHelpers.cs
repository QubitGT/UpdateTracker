using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit;

internal static class SortingHelpers
{
	private class InteractableBasedEvaluator : IInteractorDistanceEvaluator
	{
		public float EvaluateDistance(IXRInteractor interactor, IXRInteractable interactable)
		{
			return interactable.GetDistanceSqrToInteractor(interactor);
		}
	}

	private class ClosestPointOnColliderEvaluator : IInteractorDistanceEvaluator
	{
		public float EvaluateDistance(IXRInteractor interactor, IXRInteractable interactable)
		{
			float3 @float = interactor.GetAttachTransform(interactable).position;
			XRInteractableUtility.TryGetClosestPointOnCollider(interactable, @float, out var distanceInfo);
			return distanceInfo.distanceSqr;
		}
	}

	[BurstCompile]
	private class SquareDistanceAttachPointEvaluator : IInteractorDistanceEvaluator
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		internal delegate float SqDistanceToInteractable_000017C3_0024PostfixBurstDelegate(in float3 attachPosition, in float3 interactablePosition);

		internal static class SqDistanceToInteractable_000017C3_0024BurstDirectCall
		{
			private static IntPtr Pointer;

			[BurstDiscard]
			private static void GetFunctionPointerDiscard(ref IntPtr P_0)
			{
				if (Pointer == (IntPtr)0)
				{
					Pointer = BurstCompiler.CompileFunctionPointer<SqDistanceToInteractable_000017C3_0024PostfixBurstDelegate>(SqDistanceToInteractable).Value;
				}
				P_0 = Pointer;
			}

			private static IntPtr GetFunctionPointer()
			{
				nint result = 0;
				GetFunctionPointerDiscard(ref result);
				return result;
			}

			public unsafe static float Invoke(in float3 attachPosition, in float3 interactablePosition)
			{
				if (BurstCompiler.IsEnabled)
				{
					IntPtr functionPointer = GetFunctionPointer();
					if (functionPointer != (IntPtr)0)
					{
						return ((delegate* unmanaged[Cdecl]<ref float3, ref float3, float>)functionPointer)(ref attachPosition, ref interactablePosition);
					}
				}
				return SqDistanceToInteractable_0024BurstManaged(in attachPosition, in interactablePosition);
			}
		}

		public float EvaluateDistance(IXRInteractor interactor, IXRInteractable interactable)
		{
			float3 attachPosition = interactor.GetAttachTransform(interactable).position;
			float3 interactablePosition = interactable.GetAttachTransform(interactor).position;
			return SqDistanceToInteractable(in attachPosition, in interactablePosition);
		}

		[BurstCompile]
		[MonoPInvokeCallback(typeof(SqDistanceToInteractable_000017C3_0024PostfixBurstDelegate))]
		private static float SqDistanceToInteractable(in float3 attachPosition, in float3 interactablePosition)
		{
			return SqDistanceToInteractable_000017C3_0024BurstDirectCall.Invoke(in attachPosition, in interactablePosition);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[BurstCompile]
		internal static float SqDistanceToInteractable_0024BurstManaged(in float3 attachPosition, in float3 interactablePosition)
		{
			return math.lengthsq(attachPosition - interactablePosition);
		}
	}

	private static readonly Dictionary<IXRInteractable, float> s_InteractableDistanceSqrMap = new Dictionary<IXRInteractable, float>();

	private static readonly Comparison<IXRInteractable> s_InteractableDistanceComparison = InteractableDistanceComparison;

	public static readonly IInteractorDistanceEvaluator squareDistanceAttachPointEvaluator = new SquareDistanceAttachPointEvaluator();

	public static readonly IInteractorDistanceEvaluator interactableBasedEvaluator = new InteractableBasedEvaluator();

	public static readonly IInteractorDistanceEvaluator closestPointOnColliderEvaluator = new ClosestPointOnColliderEvaluator();

	public static void Sort<T>(IList<T> hits, IComparer<T> comparer) where T : struct
	{
		Sort(hits, comparer, hits.Count);
	}

	public static void Sort<T>(IList<T> hits, IComparer<T> comparer, int count) where T : struct
	{
		if (count <= 1)
		{
			return;
		}
		int num = count - 1;
		while (num > 0)
		{
			bool flag = false;
			for (int i = 1; i <= num; i++)
			{
				if (comparer.Compare(hits[i - 1], hits[i]) > 0)
				{
					int index = i - 1;
					int index2 = i;
					T val = hits[i];
					T val2 = hits[i - 1];
					T val4 = (hits[index] = val);
					val4 = (hits[index2] = val2);
					flag = true;
				}
			}
			if (flag)
			{
				num--;
				continue;
			}
			break;
		}
	}

	public static void SortByDistanceToInteractor(IXRInteractor interactor, List<IXRInteractable> unsortedTargets, List<IXRInteractable> results)
	{
		SortByDistanceToInteractor(interactor, unsortedTargets, results, interactableBasedEvaluator);
	}

	public static void SortByDistanceToInteractor(IXRInteractor interactor, List<IXRInteractable> unsortedTargets, List<IXRInteractable> results, IInteractorDistanceEvaluator distanceEvaluator)
	{
		results.Clear();
		if (unsortedTargets.Count == 0)
		{
			return;
		}
		if (unsortedTargets.Count == 1)
		{
			results.Add(unsortedTargets[0]);
			return;
		}
		results.AddRange(unsortedTargets);
		s_InteractableDistanceSqrMap.Clear();
		foreach (IXRInteractable unsortedTarget in unsortedTargets)
		{
			s_InteractableDistanceSqrMap[unsortedTarget] = distanceEvaluator.EvaluateDistance(interactor, unsortedTarget);
		}
		results.Sort(s_InteractableDistanceComparison);
	}

	public static void SortByDistanceToInteractor(IXRInteractor interactor, List<IXRInteractable> interactablesToSort)
	{
		SortByDistanceToInteractor(interactor, interactablesToSort, interactableBasedEvaluator);
	}

	public static void SortByDistanceToInteractor(IXRInteractor interactor, List<IXRInteractable> interactablesToSort, IInteractorDistanceEvaluator distanceEvaluator)
	{
		if (interactablesToSort.Count <= 1)
		{
			return;
		}
		s_InteractableDistanceSqrMap.Clear();
		foreach (IXRInteractable item in interactablesToSort)
		{
			s_InteractableDistanceSqrMap[item] = distanceEvaluator.EvaluateDistance(interactor, item);
		}
		interactablesToSort.Sort(s_InteractableDistanceComparison);
	}

	private static int InteractableDistanceComparison(IXRInteractable x, IXRInteractable y)
	{
		float num = s_InteractableDistanceSqrMap[x];
		float value = s_InteractableDistanceSqrMap[y];
		return num.CompareTo(value);
	}
}
