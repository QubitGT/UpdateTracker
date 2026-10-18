using System.Collections.Generic;
using System.Runtime.InteropServices;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.GrabAPI;

public class FingerPalmGrabAPI : IFingerAPI
{
	[StructLayout(LayoutKind.Sequential)]
	public class HandData
	{
		private const int NumHandJoints = 24;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 168, ArraySubType = UnmanagedType.R4)]
		private float[] jointValues;

		private float _rootRotX;

		private float _rootRotY;

		private float _rootRotZ;

		private float _rootRotW;

		private float _rootPosX;

		private float _rootPosY;

		private float _rootPosZ;

		private int _handedness;

		public HandData()
		{
			jointValues = new float[168];
		}

		public void SetData(IReadOnlyList<Pose> joints, Pose root, Handedness handedness)
		{
			int num = 0;
			for (int i = 0; i < 24; i++)
			{
				Pose pose = joints[i];
				jointValues[num++] = pose.rotation.x;
				jointValues[num++] = pose.rotation.y;
				jointValues[num++] = pose.rotation.z;
				jointValues[num++] = pose.rotation.w;
				jointValues[num++] = pose.position.x;
				jointValues[num++] = pose.position.y;
				jointValues[num++] = pose.position.z;
			}
			_rootRotX = root.rotation.x;
			_rootRotY = root.rotation.y;
			_rootRotZ = root.rotation.z;
			_rootRotW = root.rotation.w;
			_rootPosX = root.position.x;
			_rootPosY = root.position.y;
			_rootPosZ = root.position.z;
			_handedness = (int)handedness;
		}
	}

	private enum ReturnValue
	{
		Success = 0,
		Failure = -1
	}

	private int apiHandle_ = -1;

	private HandData handData_;

	[DllImport("InteractionSdk")]
	private static extern int isdk_FingerPalmGrabAPI_Create();

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPalmGrabAPI_UpdateHandData(int handle, [In] HandData data);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPalmGrabAPI_GetFingerIsGrabbing(int handle, HandFinger finger, out bool grabbing);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPalmGrabAPI_GetFingerIsGrabbingChanged(int handle, HandFinger finger, bool targetGrabState, out bool changed);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPalmGrabAPI_GetFingerGrabScore(int handle, HandFinger finger, out float score);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPalmGrabAPI_GetCenterOffset(int handle, out Vector3 score);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPalmGrabAPI_GetConfigParamFloat(int handle, PalmGrabParamID paramID, out float outVal);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPalmGrabAPI_SetConfigParamFloat(int handle, PalmGrabParamID paramID, float inVal);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPalmGrabAPI_GetConfigParamVec3(int handle, PalmGrabParamID paramID, out Vector3 outVal);

	[DllImport("InteractionSdk")]
	private static extern ReturnValue isdk_FingerPalmGrabAPI_SetConfigParamVec3(int handle, PalmGrabParamID paramID, Vector3 inVal);

	public FingerPalmGrabAPI()
	{
		handData_ = new HandData();
	}

	private int GetHandle()
	{
		if (apiHandle_ == -1)
		{
			apiHandle_ = isdk_FingerPalmGrabAPI_Create();
		}
		return apiHandle_;
	}

	public bool GetFingerIsGrabbing(HandFinger finger)
	{
		isdk_FingerPalmGrabAPI_GetFingerIsGrabbing(GetHandle(), finger, out var grabbing);
		return grabbing;
	}

	public bool GetFingerIsGrabbingChanged(HandFinger finger, bool targetGrabState)
	{
		isdk_FingerPalmGrabAPI_GetFingerIsGrabbingChanged(GetHandle(), finger, targetGrabState, out var changed);
		return changed;
	}

	public float GetFingerGrabScore(HandFinger finger)
	{
		isdk_FingerPalmGrabAPI_GetFingerGrabScore(GetHandle(), finger, out var score);
		return score;
	}

	public void Update(IHand hand)
	{
		if (hand.GetRootPose(out var pose) && hand.GetJointPosesFromWrist(out var jointPosesFromWrist))
		{
			handData_.SetData(jointPosesFromWrist, pose, hand.Handedness);
			isdk_FingerPalmGrabAPI_UpdateHandData(GetHandle(), handData_);
		}
	}

	public Vector3 GetWristOffsetLocal()
	{
		isdk_FingerPalmGrabAPI_GetCenterOffset(GetHandle(), out var score);
		return score;
	}

	public void SetConfigParamFloat(PalmGrabParamID paramId, float paramVal)
	{
		isdk_FingerPalmGrabAPI_SetConfigParamFloat(GetHandle(), paramId, paramVal);
	}

	public float GetConfigParamFloat(PalmGrabParamID paramId)
	{
		isdk_FingerPalmGrabAPI_GetConfigParamFloat(GetHandle(), paramId, out var outVal);
		return outVal;
	}

	public void SetConfigParamVec3(PalmGrabParamID paramId, Vector3 paramVal)
	{
		isdk_FingerPalmGrabAPI_SetConfigParamVec3(GetHandle(), paramId, paramVal);
	}

	public Vector3 GetConfigParamVec3(PalmGrabParamID paramId)
	{
		isdk_FingerPalmGrabAPI_GetConfigParamVec3(GetHandle(), paramId, out var outVal);
		return outVal;
	}
}
