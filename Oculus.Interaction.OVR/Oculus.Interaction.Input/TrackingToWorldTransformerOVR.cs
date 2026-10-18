using System;
using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Feature(Feature.Interaction)]
public class TrackingToWorldTransformerOVR : MonoBehaviour, ITrackingToWorldTransformer
{
	[SerializeField]
	[Interface(typeof(IOVRCameraRigRef), new Type[] { })]
	private UnityEngine.Object _cameraRigRef;

	public IOVRCameraRigRef CameraRigRef { get; private set; }

	public Transform Transform => CameraRigRef.CameraRig.trackingSpace;

	public Quaternion WorldToTrackingWristJointFixup => FromOVRHandDataSource.WristFixupRotation;

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

	protected virtual void Awake()
	{
		CameraRigRef = _cameraRigRef as IOVRCameraRigRef;
	}

	protected virtual void Start()
	{
	}

	public void InjectAllTrackingToWorldTransformerOVR(IOVRCameraRigRef cameraRigRef)
	{
		InjectCameraRigRef(cameraRigRef);
	}

	public void InjectCameraRigRef(IOVRCameraRigRef cameraRigRef)
	{
		_cameraRigRef = cameraRigRef as UnityEngine.Object;
		CameraRigRef = cameraRigRef;
	}

	Pose ITrackingToWorldTransformer.ToTrackingPose(in Pose worldPose)
	{
		return ToTrackingPose(in worldPose);
	}
}
