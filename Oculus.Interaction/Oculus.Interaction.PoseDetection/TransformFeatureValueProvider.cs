using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class TransformFeatureValueProvider
{
	public struct TransformProperties
	{
		public readonly Pose CenterEyePose;

		public readonly Pose WristPose;

		public readonly Handedness Handedness;

		public readonly Vector3 TrackingSystemUp;

		public readonly Vector3 TrackingSystemForward;

		public TransformProperties(Pose centerEyePos, Pose wristPose, Handedness handedness, Vector3 trackingSystemUp, Vector3 trackingSystemForward)
		{
			CenterEyePose = centerEyePos;
			WristPose = wristPose;
			Handedness = handedness;
			TrackingSystemUp = trackingSystemUp;
			TrackingSystemForward = trackingSystemForward;
		}
	}

	public static float GetValue(TransformFeature transformFeature, TransformJointData transformJointData, TransformConfig transformConfig)
	{
		TransformProperties transformProps = new TransformProperties(transformJointData.CenterEyePose, transformJointData.WristPose, transformJointData.Handedness, transformJointData.TrackingSystemUp, transformJointData.TrackingSystemForward);
		return transformFeature switch
		{
			TransformFeature.WristDown => GetWristDownValue(in transformProps, in transformConfig), 
			TransformFeature.WristUp => GetWristUpValue(in transformProps, in transformConfig), 
			TransformFeature.PalmDown => GetPalmDownValue(in transformProps, in transformConfig), 
			TransformFeature.PalmUp => GetPalmUpValue(in transformProps, in transformConfig), 
			TransformFeature.PalmTowardsFace => GetPalmTowardsFaceValue(in transformProps, in transformConfig), 
			TransformFeature.PalmAwayFromFace => GetPalmAwayFromFaceValue(in transformProps, in transformConfig), 
			TransformFeature.FingersUp => GetFingersUpValue(in transformProps, in transformConfig), 
			TransformFeature.FingersDown => GetFingersDownValue(in transformProps, in transformConfig), 
			_ => GetPinchClearValue(in transformProps, in transformConfig), 
		};
	}

	[Obsolete("The TransformConfig parameter is obsolete")]
	public static Vector3 GetHandVectorForFeature(TransformFeature transformFeature, in TransformJointData transformJointData, in TransformConfig transformConfig)
	{
		return GetHandVectorForFeature(transformFeature, in transformJointData);
	}

	public static Vector3 GetHandVectorForFeature(TransformFeature transformFeature, in TransformJointData transformJointData)
	{
		TransformProperties transformProps = new TransformProperties(transformJointData.CenterEyePose, transformJointData.WristPose, transformJointData.Handedness, transformJointData.TrackingSystemUp, transformJointData.TrackingSystemForward);
		return GetHandVectorForFeature(transformFeature, in transformProps);
	}

	private static Vector3 GetHandVectorForFeature(TransformFeature transformFeature, in TransformProperties transformProps)
	{
		Quaternion rotation = transformProps.WristPose.rotation;
		bool flag = transformProps.Handedness == Handedness.Left;
		return transformFeature switch
		{
			TransformFeature.WristDown => rotation * (flag ? Constants.LeftPinkySide : Constants.RightPinkySide), 
			TransformFeature.WristUp => rotation * (flag ? Constants.LeftThumbSide : Constants.RightThumbSide), 
			TransformFeature.PalmDown => rotation * (flag ? Constants.LeftDorsal : Constants.RightDorsal), 
			TransformFeature.PalmUp => rotation * (flag ? Constants.LeftPalmar : Constants.RightPalmar), 
			TransformFeature.PalmTowardsFace => rotation * (flag ? Constants.LeftDorsal : Constants.RightDorsal), 
			TransformFeature.PalmAwayFromFace => rotation * (flag ? Constants.LeftPalmar : Constants.RightPalmar), 
			TransformFeature.FingersUp => rotation * (flag ? Constants.LeftDistal : Constants.RightDistal), 
			TransformFeature.FingersDown => rotation * (flag ? Constants.LeftProximal : Constants.RightProximal), 
			TransformFeature.PinchClear => rotation * (flag ? Constants.LeftPinkySide : Constants.RightPinkySide), 
			_ => rotation * (flag ? Constants.LeftPinkySide : Constants.RightPinkySide), 
		};
	}

	public static Vector3 GetTargetVectorForFeature(TransformFeature transformFeature, in TransformJointData transformJointData, in TransformConfig transformConfig)
	{
		TransformProperties transformProps = new TransformProperties(transformJointData.CenterEyePose, transformJointData.WristPose, transformJointData.Handedness, transformJointData.TrackingSystemUp, transformJointData.TrackingSystemForward);
		return GetTargetVectorForFeature(transformFeature, in transformProps, in transformConfig);
	}

	private static Vector3 GetTargetVectorForFeature(TransformFeature transformFeature, in TransformProperties transformProps, in TransformConfig transformConfig)
	{
		Vector3 result = Vector3.zero;
		switch (transformFeature)
		{
		case TransformFeature.WristUp:
		case TransformFeature.WristDown:
		case TransformFeature.PalmDown:
		case TransformFeature.PalmUp:
		case TransformFeature.FingersUp:
		case TransformFeature.FingersDown:
		{
			Vector3 originalVector = GetVerticalVector(in transformProps.CenterEyePose, in transformProps.TrackingSystemUp, in transformConfig);
			result = OffsetVectorWithRotation(in transformProps, in originalVector, in transformConfig);
			break;
		}
		case TransformFeature.PalmTowardsFace:
		case TransformFeature.PalmAwayFromFace:
		case TransformFeature.PinchClear:
		{
			Vector3 originalVector = transformProps.CenterEyePose.forward;
			result = OffsetVectorWithRotation(in transformProps, in originalVector, in transformConfig);
			break;
		}
		}
		return result;
	}

	private static float GetWristDownValue(in TransformProperties transformProps, in TransformConfig transformConfig)
	{
		Vector3 handVectorForFeature = GetHandVectorForFeature(TransformFeature.WristDown, in transformProps);
		Vector3 targetVectorForFeature = GetTargetVectorForFeature(TransformFeature.WristDown, in transformProps, in transformConfig);
		return Vector3.Angle(handVectorForFeature, targetVectorForFeature);
	}

	private static float GetWristUpValue(in TransformProperties transformProps, in TransformConfig transformConfig)
	{
		Vector3 handVectorForFeature = GetHandVectorForFeature(TransformFeature.WristUp, in transformProps);
		Vector3 targetVectorForFeature = GetTargetVectorForFeature(TransformFeature.WristUp, in transformProps, in transformConfig);
		return Vector3.Angle(handVectorForFeature, targetVectorForFeature);
	}

	private static float GetPalmDownValue(in TransformProperties transformProps, in TransformConfig transformConfig)
	{
		Vector3 handVectorForFeature = GetHandVectorForFeature(TransformFeature.PalmDown, in transformProps);
		Vector3 targetVectorForFeature = GetTargetVectorForFeature(TransformFeature.PalmDown, in transformProps, in transformConfig);
		return Vector3.Angle(handVectorForFeature, targetVectorForFeature);
	}

	private static float GetPalmUpValue(in TransformProperties transformProps, in TransformConfig transformConfig)
	{
		Vector3 handVectorForFeature = GetHandVectorForFeature(TransformFeature.PalmUp, in transformProps);
		Vector3 targetVectorForFeature = GetTargetVectorForFeature(TransformFeature.PalmUp, in transformProps, in transformConfig);
		return Vector3.Angle(handVectorForFeature, targetVectorForFeature);
	}

	private static float GetPalmTowardsFaceValue(in TransformProperties transformProps, in TransformConfig transformConfig)
	{
		Vector3 handVectorForFeature = GetHandVectorForFeature(TransformFeature.PalmTowardsFace, in transformProps);
		Vector3 targetVectorForFeature = GetTargetVectorForFeature(TransformFeature.PalmTowardsFace, in transformProps, in transformConfig);
		return Vector3.Angle(handVectorForFeature, targetVectorForFeature);
	}

	private static float GetPalmAwayFromFaceValue(in TransformProperties transformProps, in TransformConfig transformConfig)
	{
		Vector3 handVectorForFeature = GetHandVectorForFeature(TransformFeature.PalmAwayFromFace, in transformProps);
		Vector3 targetVectorForFeature = GetTargetVectorForFeature(TransformFeature.PalmAwayFromFace, in transformProps, in transformConfig);
		return Vector3.Angle(handVectorForFeature, targetVectorForFeature);
	}

	private static float GetFingersUpValue(in TransformProperties transformProps, in TransformConfig transformConfig)
	{
		Vector3 handVectorForFeature = GetHandVectorForFeature(TransformFeature.FingersUp, in transformProps);
		Vector3 targetVectorForFeature = GetTargetVectorForFeature(TransformFeature.FingersUp, in transformProps, in transformConfig);
		return Vector3.Angle(handVectorForFeature, targetVectorForFeature);
	}

	private static float GetFingersDownValue(in TransformProperties transformProps, in TransformConfig transformConfig)
	{
		Vector3 handVectorForFeature = GetHandVectorForFeature(TransformFeature.FingersDown, in transformProps);
		Vector3 targetVectorForFeature = GetTargetVectorForFeature(TransformFeature.FingersDown, in transformProps, in transformConfig);
		return Vector3.Angle(handVectorForFeature, targetVectorForFeature);
	}

	private static float GetPinchClearValue(in TransformProperties transformProps, in TransformConfig transformConfig)
	{
		Vector3 handVectorForFeature = GetHandVectorForFeature(TransformFeature.PinchClear, in transformProps);
		Vector3 targetVectorForFeature = GetTargetVectorForFeature(TransformFeature.PinchClear, in transformProps, in transformConfig);
		return Vector3.Angle(handVectorForFeature, targetVectorForFeature);
	}

	private static Vector3 GetVerticalVector(in Pose centerEyePose, in Vector3 trackingSystemUp, in TransformConfig transformConfig)
	{
		return transformConfig.UpVectorType switch
		{
			UpVectorType.Head => centerEyePose.up, 
			UpVectorType.Tracking => trackingSystemUp, 
			UpVectorType.World => Vector3.up, 
			_ => Vector3.up, 
		};
	}

	private static Vector3 OffsetVectorWithRotation(in TransformProperties transformProps, in Vector3 originalVector, in TransformConfig transformConfig)
	{
		Quaternion quaternion = transformConfig.UpVectorType switch
		{
			UpVectorType.Head => transformProps.CenterEyePose.rotation, 
			UpVectorType.Tracking => Quaternion.LookRotation(transformProps.TrackingSystemForward, transformProps.TrackingSystemUp), 
			_ => Quaternion.identity, 
		};
		Quaternion quaternion2 = Quaternion.Euler(transformConfig.RotationOffset);
		return quaternion * quaternion2 * Quaternion.Inverse(quaternion) * originalVector;
	}
}
