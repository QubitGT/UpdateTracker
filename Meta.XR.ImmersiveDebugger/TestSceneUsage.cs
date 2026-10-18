using System;
using Meta.XR.ImmersiveDebugger;
using Meta.XR.ImmersiveDebugger.Gizmo;
using UnityEngine;

[RequireComponent(typeof(OVREyeGaze))]
internal class TestSceneUsage : MonoBehaviour
{
	private OVREyeGaze _eyeGazeComponent;

	[DebugMember(DebugColor.Gray, GizmoType = DebugGizmoType.Axis)]
	private Pose _eyeGazePose;

	[DebugMember(DebugColor.Gray, GizmoType = DebugGizmoType.Point)]
	private Vector3 _eyeGazePosition;

	[DebugMember(DebugColor.Gray, GizmoType = DebugGizmoType.Line)]
	private Tuple<Vector3, Vector3> _eyeGazeDirection;

	[DebugMember(DebugColor.Gray)]
	private float _confidence;

	[DebugMember(DebugColor.Gray, Tweakable = true, Min = 0.1f, Max = 1f)]
	private float drawingLineWidth = 0.01f;

	[DebugMember(DebugColor.Gray, Tweakable = true)]
	private bool passthroughEnabled = true;

	private bool previousPassthroughEnabled = true;

	private void Start()
	{
		_eyeGazeComponent = GetComponent<OVREyeGaze>();
	}

	private void Update()
	{
		_eyeGazePose.position = base.transform.position;
		_eyeGazePose.position.z += 0.15f;
		_eyeGazePose.rotation = base.transform.rotation;
		_eyeGazePosition = _eyeGazePose.position;
		Vector3 eyeGazePosition = _eyeGazePosition;
		eyeGazePosition += _eyeGazePose.rotation * Vector3.forward * 2f;
		_eyeGazeDirection = Tuple.Create(_eyeGazePosition, eyeGazePosition);
		_confidence = _eyeGazeComponent.Confidence;
		DebugGizmos.LineWidth = drawingLineWidth;
		if (passthroughEnabled != previousPassthroughEnabled)
		{
			UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>().GetComponent<OVRManager>().isInsightPassthroughEnabled = passthroughEnabled;
			previousPassthroughEnabled = passthroughEnabled;
		}
	}

	[DebugMember(DebugColor.Gray)]
	private void TogglePassthrough()
	{
		TogglePassthroughStatic();
	}

	[DebugMember(DebugColor.Gray)]
	private static void TogglePassthroughStatic()
	{
		OVRManager component = UnityEngine.Object.FindAnyObjectByType<OVRCameraRig>().GetComponent<OVRManager>();
		component.isInsightPassthroughEnabled = !component.isInsightPassthroughEnabled;
	}
}
