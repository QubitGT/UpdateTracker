using UnityEngine;

namespace Oculus.Interaction.Input;

public static class HandMirroring
{
	public struct HandsSpace
	{
		private readonly HandSpace _leftHand;

		private readonly HandSpace _rightHand;

		public readonly HandSpace this[Handedness handedness]
		{
			get
			{
				if (handedness != 0)
				{
					return _rightHand;
				}
				return _leftHand;
			}
		}

		public HandsSpace(HandSpace leftHand, HandSpace rightHand)
		{
			_leftHand = leftHand;
			_rightHand = rightHand;
		}
	}

	public struct HandSpace
	{
		public readonly Vector3 distal;

		public readonly Vector3 dorsal;

		public readonly Vector3 thumbSide;

		public readonly Quaternion rotation;

		public HandSpace(Vector3 distal, Vector3 dorsal, Vector3 thumbSide)
		{
			this.distal = distal;
			this.dorsal = dorsal;
			this.thumbSide = thumbSide;
			rotation = Quaternion.LookRotation(distal, dorsal);
		}
	}

	public static readonly HandSpace LeftHandSpace = new HandSpace(Constants.LeftDistal, Constants.LeftDorsal, Constants.LeftThumbSide);

	public static readonly HandSpace RightHandSpace = new HandSpace(Constants.RightDistal, Constants.RightDorsal, Constants.RightThumbSide);

	public static Pose Mirror(Pose pose)
	{
		pose.position = Mirror(in pose.position);
		pose.rotation = Mirror(in pose.rotation);
		return pose;
	}

	public static Vector3 Mirror(in Vector3 position)
	{
		return TransformPosition(in position, in LeftHandSpace, in RightHandSpace);
	}

	public static Quaternion Mirror(in Quaternion rotation)
	{
		return TransformRotation(in rotation, in LeftHandSpace, in RightHandSpace);
	}

	public static Quaternion Reflect(in Quaternion rotation, Vector3 normal)
	{
		Vector3 forward = Vector3.Reflect(rotation * RightHandSpace.distal, normal);
		Vector3 upwards = Vector3.Reflect(rotation * RightHandSpace.dorsal, normal);
		return Quaternion.LookRotation(forward, upwards) * Quaternion.Inverse(LeftHandSpace.rotation);
	}

	public static Pose TransformPose(in Pose pose, in HandSpace fromHand, in HandSpace toHand)
	{
		return new Pose(TransformPosition(in pose.position, in fromHand, in toHand), TransformRotation(in pose.rotation, in fromHand, in toHand));
	}

	public static Vector3 TransformPosition(in Vector3 position, in HandSpace fromHand, in HandSpace toHand)
	{
		Vector3 vector = Vector3.Dot(position, fromHand.distal) * toHand.distal;
		Vector3 vector2 = Vector3.Dot(position, fromHand.dorsal) * toHand.dorsal;
		Vector3 vector3 = Vector3.Dot(position, fromHand.thumbSide) * toHand.thumbSide;
		return vector + vector2 + vector3;
	}

	public static Quaternion TransformRotation(in Quaternion rotation, in HandSpace fromHand, in HandSpace toHand)
	{
		Vector3 position = rotation * Vector3.forward;
		Vector3 forward = TransformPosition(in position, in fromHand, in toHand);
		position = rotation * Vector3.up;
		Vector3 upwards = TransformPosition(in position, in fromHand, in toHand);
		return Quaternion.LookRotation(forward, upwards) * Quaternion.Inverse(toHand.rotation) * fromHand.rotation;
	}
}
