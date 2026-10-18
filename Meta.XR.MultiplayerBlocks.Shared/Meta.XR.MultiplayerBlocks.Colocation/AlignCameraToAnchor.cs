using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Colocation;

[DefaultExecutionOrder(10)]
internal class AlignCameraToAnchor : MonoBehaviour
{
	public OVRSpatialAnchor CameraAlignmentAnchor { get; set; }

	private void Update()
	{
		RealignToAnchor();
	}

	public void RealignToAnchor()
	{
		Align(CameraAlignmentAnchor.transform);
	}

	private void Align(Transform anchorTransform)
	{
		Vector3 localScale = anchorTransform.localScale;
		anchorTransform.localScale = Vector3.one;
		OVRPose trackingSpacePose = anchorTransform.ToTrackingSpacePose(Camera.main);
		anchorTransform.SetPositionAndRotation(trackingSpacePose.position, trackingSpacePose.orientation);
		base.transform.position = anchorTransform.InverseTransformPoint(Vector3.zero);
		base.transform.eulerAngles = new Vector3(0f, 0f - anchorTransform.eulerAngles.y, 0f);
		OVRPose oVRPose = trackingSpacePose.ToWorldSpacePose(Camera.main);
		anchorTransform.SetPositionAndRotation(oVRPose.position, oVRPose.orientation);
		anchorTransform.localScale = localScale;
	}
}
