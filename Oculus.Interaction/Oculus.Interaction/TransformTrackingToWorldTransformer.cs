using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class TransformTrackingToWorldTransformer : MonoBehaviour, ITrackingToWorldTransformer
{
	[SerializeField]
	private Transform TrackingSpace;

	public Transform Transform => TrackingSpace;

	public Quaternion WorldToTrackingWristJointFixup { get; } = new Quaternion(0f, 1f, 0f, 0f);


	public Pose ToWorldPose(Pose pose)
	{
		Transform transform = Transform;
		pose.position = transform.TransformPoint(pose.position);
		pose.rotation = transform.rotation * pose.rotation;
		return pose;
	}

	public Pose ToTrackingPose(in Pose worldPose)
	{
		Transform obj = Transform;
		Vector3 position = obj.InverseTransformPoint(worldPose.position);
		Quaternion rotation = Quaternion.Inverse(obj.rotation) * worldPose.rotation;
		return new Pose(position, rotation);
	}

	Pose ITrackingToWorldTransformer.ToTrackingPose(in Pose worldPose)
	{
		return ToTrackingPose(in worldPose);
	}
}
