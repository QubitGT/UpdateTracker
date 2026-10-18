using UnityEngine;

namespace Meta.XR.BuildingBlocks;

public class SpatialAnchorSpawnerBuildingBlock : MonoBehaviour
{
	[Tooltip("A placeholder object to place in the anchor's position.")]
	[SerializeField]
	private GameObject _anchorPrefab;

	[Tooltip("Anchor prefab GameObject will follow the user's right hand.")]
	[SerializeField]
	private bool _followHand = true;

	private SpatialAnchorCoreBuildingBlock _spatialAnchorCore;

	private OVRCameraRig _cameraRig;

	private Transform _anchorPrefabTransform;

	private Vector3 _initialPosition;

	private Quaternion _initialRotation;

	public GameObject AnchorPrefab
	{
		get
		{
			return _anchorPrefab;
		}
		set
		{
			_anchorPrefab = value;
			if ((bool)_anchorPrefabTransform)
			{
				Object.Destroy(_anchorPrefabTransform.gameObject);
			}
			_anchorPrefabTransform = Object.Instantiate(AnchorPrefab).transform;
			FollowHand = _followHand;
		}
	}

	public bool FollowHand
	{
		get
		{
			return _followHand;
		}
		set
		{
			_followHand = value;
			if (_followHand)
			{
				_initialPosition = _anchorPrefabTransform.position;
				_initialRotation = _anchorPrefabTransform.rotation;
				_anchorPrefabTransform.parent = _cameraRig.rightControllerAnchor;
				_anchorPrefabTransform.localPosition = Vector3.zero;
				_anchorPrefabTransform.localRotation = Quaternion.identity;
			}
			else
			{
				_anchorPrefabTransform.parent = null;
				_anchorPrefabTransform.SetPositionAndRotation(_initialPosition, _initialRotation);
			}
		}
	}

	private void Awake()
	{
		_spatialAnchorCore = SpatialAnchorCoreBuildingBlock.GetFirstInstance();
		_cameraRig = Object.FindAnyObjectByType<OVRCameraRig>();
		AnchorPrefab = _anchorPrefab;
		FollowHand = _followHand;
	}

	public void SpawnSpatialAnchor(Vector3 position, Quaternion rotation)
	{
		_spatialAnchorCore.InstantiateSpatialAnchor(AnchorPrefab, position, rotation);
	}

	internal void SpawnSpatialAnchor()
	{
		if (!FollowHand)
		{
			SpawnSpatialAnchor(AnchorPrefab.transform.position, AnchorPrefab.transform.rotation);
		}
		else
		{
			SpawnSpatialAnchor(_anchorPrefabTransform.position, _anchorPrefabTransform.rotation);
		}
	}
}
