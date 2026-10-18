using UnityEngine;

namespace Meta.XR.MRUtilityKit.BuildingBlocks;

public class PlaceWithAnchor : MonoBehaviour
{
	[Tooltip("Target transform to place")]
	public Transform Target;

	private Transform _spatialAnchorTransform;

	private OVRSpatialAnchor _spatialAnchor;

	private bool _requestMove;

	private Pose _surfacePose;

	private void Awake()
	{
		_spatialAnchorTransform = new GameObject("[" + base.gameObject.name + "] Spatial Anchor").transform;
		if ((object)Target == null)
		{
			Target = base.transform;
		}
	}

	public void RequestMove(Pose pose)
	{
		_requestMove = true;
		_surfacePose = pose;
	}

	private void Update()
	{
		if (_requestMove && _surfacePose != default(Pose))
		{
			SetTargetWithAnchor(_surfacePose);
			_requestMove = false;
		}
	}

	public void OnLocateSpace(Pose surfacePose, bool success)
	{
		if (!success)
		{
			Debug.Log("[PlaceWithAnchor] Failed to locate space.");
		}
		else
		{
			RequestMove(surfacePose);
		}
	}

	private void SetTargetWithAnchor(Pose pose)
	{
		EraseAnchor();
		Target.SetParent(null);
		Target.SetPositionAndRotation(pose.position, pose.rotation);
		SetAnchor();
	}

	private void EraseAnchor()
	{
		if (_spatialAnchorTransform.TryGetComponent<OVRSpatialAnchor>(out _spatialAnchor))
		{
			_spatialAnchor.EraseAnchorAsync();
			Object.DestroyImmediate(_spatialAnchor);
		}
	}

	private void SetAnchor()
	{
		_spatialAnchorTransform.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
		_spatialAnchor = _spatialAnchorTransform.gameObject.AddComponent<OVRSpatialAnchor>();
		Target.SetParent(_spatialAnchorTransform.transform);
	}
}
