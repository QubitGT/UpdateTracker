using System;
using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public static class HandGrabInteraction
{
	[Obsolete("Use CalculateBestGrab instead")]
	public static bool TryCalculateBestGrab(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable interactable, GrabTypeFlags grabTypes, out HandGrabTarget.GrabAnchor anchorMode, ref HandGrabResult handGrabResult)
	{
		handGrabInteractor.CalculateBestGrab(interactable, grabTypes, out var activeGrabFlags, ref handGrabResult);
		if (activeGrabFlags.HasFlag(GrabTypeFlags.Pinch))
		{
			anchorMode = HandGrabTarget.GrabAnchor.Pinch;
		}
		else if (activeGrabFlags.HasFlag(GrabTypeFlags.Palm))
		{
			anchorMode = HandGrabTarget.GrabAnchor.Palm;
		}
		else
		{
			anchorMode = HandGrabTarget.GrabAnchor.Wrist;
		}
		return true;
	}

	[Obsolete]
	public static GrabTypeFlags CurrentGrabType(this IHandGrabInteractor handGrabInteractor)
	{
		return handGrabInteractor.HandGrabTarget.Anchor;
	}

	public static void CalculateBestGrab(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable interactable, GrabTypeFlags grabFlags, out GrabTypeFlags activeGrabFlags, ref HandGrabResult result)
	{
		activeGrabFlags = grabFlags & interactable.SupportedGrabTypes;
		handGrabInteractor.GetPoseOffset(activeGrabFlags, out var pose, out var offset);
		interactable.CalculateBestPose(in pose, in offset, interactable.RelativeTo, handGrabInteractor.Hand.Scale, handGrabInteractor.Hand.Handedness, ref result);
	}

	public static IMovement GenerateMovement(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable interactable)
	{
		Pose from = handGrabInteractor.GetTargetGrabPose();
		Pose to = handGrabInteractor.GetHandGrabPose();
		return interactable.GenerateMovement(in from, in to);
	}

	public static Pose GetHandGrabPose(this IHandGrabInteractor handGrabInteractor)
	{
		handGrabInteractor.GetPoseOffset(GrabTypeFlags.None, out var pose, out var offset);
		offset = handGrabInteractor.WristToGrabPoseOffset;
		return PoseUtils.Multiply(in pose, in offset);
	}

	public static GrabPoseScore GetPoseScore(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable interactable, GrabTypeFlags grabTypes, ref HandGrabResult result)
	{
		GrabTypeFlags anchorMode = grabTypes & interactable.SupportedGrabTypes;
		handGrabInteractor.GetPoseOffset(anchorMode, out var pose, out var offset);
		interactable.CalculateBestPose(in pose, in offset, interactable.RelativeTo, handGrabInteractor.Hand.Scale, handGrabInteractor.Hand.Handedness, ref result);
		return result.Score;
	}

	public static bool CanInteractWith(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable handGrabInteractable)
	{
		if (!handGrabInteractable.SupportsHandedness(handGrabInteractor.Hand.Handedness))
		{
			return false;
		}
		return (handGrabInteractor.SupportedGrabTypes & handGrabInteractable.SupportedGrabTypes) != 0;
	}

	public static Pose GetGrabOffset(this IHandGrabInteractor handGrabInteractor)
	{
		handGrabInteractor.GetPoseOffset(handGrabInteractor.HandGrabTarget.Anchor, out var _, out var offset);
		return offset;
	}

	public static float ComputeHandGrabScore(IHandGrabInteractor handGrabInteractor, IHandGrabInteractable handGrabInteractable, out GrabTypeFlags handGrabTypes, bool includeSelecting = false)
	{
		HandGrabAPI handGrabApi = handGrabInteractor.HandGrabApi;
		handGrabTypes = GrabTypeFlags.None;
		float num = 0f;
		if (SupportsPinch(handGrabInteractor, handGrabInteractable))
		{
			GrabbingRule fingers = handGrabInteractable.PinchGrabRules;
			float handPinchScore = handGrabApi.GetHandPinchScore(in fingers, includeSelecting);
			if (handPinchScore > num)
			{
				num = handPinchScore;
				handGrabTypes = GrabTypeFlags.Pinch;
			}
		}
		if (SupportsPalm(handGrabInteractor, handGrabInteractable))
		{
			GrabbingRule fingers = handGrabInteractable.PalmGrabRules;
			float handPalmScore = handGrabApi.GetHandPalmScore(in fingers, includeSelecting);
			if (handPalmScore > num)
			{
				num = handPalmScore;
				handGrabTypes = GrabTypeFlags.Palm;
			}
		}
		return num;
	}

	public static GrabTypeFlags ComputeShouldSelect(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable handGrabInteractable)
	{
		if (handGrabInteractable == null)
		{
			return GrabTypeFlags.None;
		}
		HandGrabAPI handGrabApi = handGrabInteractor.HandGrabApi;
		GrabTypeFlags grabTypeFlags = GrabTypeFlags.None;
		if (SupportsPinch(handGrabInteractor, handGrabInteractable))
		{
			GrabbingRule fingers = handGrabInteractable.PinchGrabRules;
			if (handGrabApi.IsHandSelectPinchFingersChanged(in fingers))
			{
				grabTypeFlags |= GrabTypeFlags.Pinch;
			}
		}
		if (SupportsPalm(handGrabInteractor, handGrabInteractable))
		{
			GrabbingRule fingers = handGrabInteractable.PalmGrabRules;
			if (handGrabApi.IsHandSelectPalmFingersChanged(in fingers))
			{
				grabTypeFlags |= GrabTypeFlags.Palm;
			}
		}
		return grabTypeFlags;
	}

	public static GrabTypeFlags ComputeShouldUnselect(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable handGrabInteractable)
	{
		HandGrabAPI handGrabApi = handGrabInteractor.HandGrabApi;
		HandFingerFlags grabbingFingers = handGrabApi.HandPinchGrabbingFingers();
		HandFingerFlags grabbingFingers2 = handGrabApi.HandPalmGrabbingFingers();
		if (handGrabInteractable.SupportedGrabTypes == GrabTypeFlags.None)
		{
			GrabbingRule fingers = GrabbingRule.FullGrab;
			if (!handGrabApi.IsSustainingGrab(in fingers, grabbingFingers))
			{
				GrabbingRule fingers2 = GrabbingRule.FullGrab;
				if (!handGrabApi.IsSustainingGrab(in fingers2, grabbingFingers2))
				{
					return GrabTypeFlags.All;
				}
			}
			return GrabTypeFlags.None;
		}
		GrabTypeFlags grabTypeFlags = GrabTypeFlags.None;
		if (SupportsPinch(handGrabInteractor, handGrabInteractable.SupportedGrabTypes))
		{
			GrabbingRule fingers = handGrabInteractable.PinchGrabRules;
			if (!handGrabApi.IsSustainingGrab(in fingers, grabbingFingers))
			{
				GrabbingRule fingers2 = handGrabInteractable.PinchGrabRules;
				if (handGrabApi.IsHandUnselectPinchFingersChanged(in fingers2))
				{
					grabTypeFlags |= GrabTypeFlags.Pinch;
				}
			}
		}
		if (SupportsPalm(handGrabInteractor, handGrabInteractable.SupportedGrabTypes))
		{
			GrabbingRule fingers = handGrabInteractable.PalmGrabRules;
			if (!handGrabApi.IsSustainingGrab(in fingers, grabbingFingers2))
			{
				GrabbingRule fingers2 = handGrabInteractable.PalmGrabRules;
				if (handGrabApi.IsHandUnselectPalmFingersChanged(in fingers2))
				{
					grabTypeFlags |= GrabTypeFlags.Palm;
				}
			}
		}
		return grabTypeFlags;
	}

	public static HandFingerFlags GrabbingFingers(this IHandGrabInteractor handGrabInteractor, IHandGrabInteractable handGrabInteractable)
	{
		HandGrabAPI handGrabApi = handGrabInteractor.HandGrabApi;
		if (handGrabInteractable == null)
		{
			return HandFingerFlags.None;
		}
		HandFingerFlags handFingerFlags = HandFingerFlags.None;
		if (SupportsPinch(handGrabInteractor, handGrabInteractable))
		{
			HandFingerFlags fingerFlags = handGrabApi.HandPinchGrabbingFingers();
			handGrabInteractable.PinchGrabRules.StripIrrelevant(ref fingerFlags);
			handFingerFlags |= fingerFlags;
		}
		if (SupportsPalm(handGrabInteractor, handGrabInteractable))
		{
			HandFingerFlags fingerFlags2 = handGrabApi.HandPalmGrabbingFingers();
			handGrabInteractable.PalmGrabRules.StripIrrelevant(ref fingerFlags2);
			handFingerFlags |= fingerFlags2;
		}
		return handFingerFlags;
	}

	private static bool SupportsPinch(IHandGrabInteractor handGrabInteractor, IHandGrabInteractable handGrabInteractable)
	{
		return SupportsPinch(handGrabInteractor, handGrabInteractable.SupportedGrabTypes);
	}

	private static bool SupportsPalm(IHandGrabInteractor handGrabInteractor, IHandGrabInteractable handGrabInteractable)
	{
		return SupportsPalm(handGrabInteractor, handGrabInteractable.SupportedGrabTypes);
	}

	private static bool SupportsPinch(IHandGrabInteractor handGrabInteractor, GrabTypeFlags grabTypes)
	{
		return (handGrabInteractor.SupportedGrabTypes & grabTypes & GrabTypeFlags.Pinch) != 0;
	}

	private static bool SupportsPalm(IHandGrabInteractor handGrabInteractor, GrabTypeFlags grabTypes)
	{
		return (handGrabInteractor.SupportedGrabTypes & grabTypes & GrabTypeFlags.Palm) != 0;
	}

	public static void GetPoseOffset(this IHandGrabInteractor handGrabInteractor, GrabTypeFlags anchorMode, out Pose pose, out Pose offset)
	{
		handGrabInteractor.Hand.GetRootPose(out pose);
		offset = Pose.identity;
		if (anchorMode != 0)
		{
			if ((anchorMode & GrabTypeFlags.Pinch) != 0 && handGrabInteractor.PinchPoint != null)
			{
				Pose to = handGrabInteractor.PinchPoint.GetPose();
				offset = PoseUtils.Delta(in pose, in to);
			}
			else if ((anchorMode & GrabTypeFlags.Palm) != 0 && handGrabInteractor.PalmPoint != null)
			{
				Pose to = handGrabInteractor.PalmPoint.GetPose();
				offset = PoseUtils.Delta(in pose, in to);
			}
		}
	}
}
