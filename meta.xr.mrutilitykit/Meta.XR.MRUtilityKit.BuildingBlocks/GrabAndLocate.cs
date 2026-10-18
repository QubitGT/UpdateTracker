using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.BuildingBlocks;

public class GrabAndLocate : SpaceLocator
{
	private HandGrabInteractable _handGrabInteractable;

	private GrabInteractable _grabInteractable;

	private PlaceWithAnchor _placeWithAnchor;

	private OVRCameraRig _cameraRig;

	private bool _requestMove;

	protected override Transform RaycastOrigin => base.transform;

	protected override float MaxRaycastDistance => 3f;

	public void Awake()
	{
		_handGrabInteractable = GetComponentInChildren<HandGrabInteractable>();
		_grabInteractable = GetComponentInChildren<GrabInteractable>();
		_placeWithAnchor = GetComponent<PlaceWithAnchor>();
		_cameraRig = Object.FindFirstObjectByType<OVRCameraRig>();
	}

	private void OnEnable()
	{
		_handGrabInteractable.WhenStateChanged += OnInteractableStateChanged;
		_grabInteractable.WhenStateChanged += OnInteractableStateChanged;
	}

	private void OnDisable()
	{
		_handGrabInteractable.WhenStateChanged -= OnInteractableStateChanged;
		_grabInteractable.WhenStateChanged -= OnInteractableStateChanged;
	}

	private void OnInteractableStateChanged(InteractableStateChangeArgs stateChange)
	{
		if (stateChange.PreviousState == InteractableState.Select)
		{
			TryLocateSpace(out var _);
		}
	}

	protected internal override Ray GetRaycastRay()
	{
		Vector3 origin = base.transform.position + base.transform.up * 0.5f;
		Vector3 direction = -base.transform.up;
		return new Ray(origin, direction);
	}
}
