using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.GrabAPI;

public class FingerPinchGrabAPI : IFingerAPI
{
	private enum ReturnValue
	{
		Success = 0,
		Failure = -1
	}

	private int _fingerPinchGrabApiHandle = -1;

	private HandPinchData _pinchData = new HandPinchData();

	private IHmd _hmd;

	[DllImport("InteractionSdk")]
	private static extern int isdk_FingerPinchGrabAPI_Create();

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_UpdateHandData(int handle, [In] HandPinchData data);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_UpdateHandWristHMDData(int handle, [In] HandPinchData data, in Vector3 wristForward, in Vector3 hmdForward);

	[DllImport("InteractionSdk", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
	private static extern bool isdk_FingerPinchGrabAPI_GetString(int handle, [MarshalAs(UnmanagedType.LPStr)] string name, out IntPtr val);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_GetFingerIsGrabbing(int handle, int index, out bool grabbing);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_GetFingerPinchPercent(int handle, int index, out float pinchPercent);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_GetFingerPinchDistance(int handle, int index, out float pinchDistance);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_GetFingerIsGrabbingChanged(int handle, int index, bool targetState, out bool grabbing);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_GetFingerGrabScore(int handle, HandFinger finger, out float outScore);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_GetCenterOffset(int handle, out Vector3 outCenter);

	[DllImport("InteractionSdk")]
	private static extern int isdk_Common_GetVersion(out IntPtr versionStringPtr);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_GetPinchGrabParam(int handle, PinchGrabParam paramId, out float outParam);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_SetPinchGrabParam(int handle, PinchGrabParam paramId, float param);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPinchGrabAPI_IsPinchVisibilityGood(int handle, out bool outVal);

	public FingerPinchGrabAPI(IHmd hmd = null)
	{
		_hmd = hmd;
	}

	private int GetHandle()
	{
		if (_fingerPinchGrabApiHandle == -1)
		{
			_fingerPinchGrabApiHandle = isdk_FingerPinchGrabAPI_Create();
		}
		return _fingerPinchGrabApiHandle;
	}

	public void SetPinchGrabParam(PinchGrabParam paramId, float paramVal)
	{
		isdk_FingerPinchGrabAPI_SetPinchGrabParam(GetHandle(), paramId, paramVal);
	}

	public float GetPinchGrabParam(PinchGrabParam paramId)
	{
		isdk_FingerPinchGrabAPI_GetPinchGrabParam(GetHandle(), paramId, out var outParam);
		return outParam;
	}

	public bool GetIsPinchVisibilityGood()
	{
		isdk_FingerPinchGrabAPI_IsPinchVisibilityGood(GetHandle(), out var outVal);
		return outVal;
	}

	public bool GetFingerIsGrabbing(HandFinger finger)
	{
		isdk_FingerPinchGrabAPI_GetFingerIsGrabbing(GetHandle(), (int)finger, out var grabbing);
		return grabbing;
	}

	public float GetFingerPinchPercent(HandFinger finger)
	{
		isdk_FingerPinchGrabAPI_GetFingerPinchPercent(GetHandle(), (int)finger, out var pinchPercent);
		return pinchPercent;
	}

	public float GetFingerPinchDistance(HandFinger finger)
	{
		isdk_FingerPinchGrabAPI_GetFingerPinchDistance(GetHandle(), (int)finger, out var pinchDistance);
		return pinchDistance;
	}

	public Vector3 GetWristOffsetLocal()
	{
		isdk_FingerPinchGrabAPI_GetCenterOffset(GetHandle(), out var outCenter);
		return outCenter;
	}

	public bool GetFingerIsGrabbingChanged(HandFinger finger, bool targetPinchState)
	{
		isdk_FingerPinchGrabAPI_GetFingerIsGrabbingChanged(GetHandle(), (int)finger, targetPinchState, out var grabbing);
		return grabbing;
	}

	public float GetFingerGrabScore(HandFinger finger)
	{
		isdk_FingerPinchGrabAPI_GetFingerGrabScore(GetHandle(), finger, out var outScore);
		return outScore;
	}

	public void Update(IHand hand)
	{
		hand.GetJointPosesFromWrist(out var jointPosesFromWrist);
		hand.GetJointPose(HandJointId.HandWristRoot, out var pose);
		Update(jointPosesFromWrist, hand.Handedness, pose);
	}

	internal void Update(IReadOnlyList<Pose> handPoses, Handedness handedness, Pose wristPose)
	{
		if (handPoses.Count <= 0)
		{
			return;
		}
		_pinchData.SetJoints(handPoses);
		Vector3 wristForward = Vector3.forward;
		Vector3 hmdForward = Vector3.forward;
		if (_hmd != null && _hmd.TryGetRootPose(out var pose))
		{
			wristForward = -1f * wristPose.forward;
			hmdForward = -1f * pose.forward;
			if (handedness == Handedness.Right)
			{
				wristForward = -wristForward;
			}
		}
		isdk_FingerPinchGrabAPI_UpdateHandWristHMDData(GetHandle(), _pinchData, in wristForward, in hmdForward);
	}
}
