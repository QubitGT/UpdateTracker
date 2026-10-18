using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class SlingshotPellet : MonoBehaviour
{
	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	private Grabbable grabbable;

	[SerializeField]
	private HandGrabInteractable[] _handGrabInteractables;

	private HandGrabInteractor _lastHandGrabInteractor;

	private UniqueIdentifier Identifier;

	private bool _hasPendingForce;

	private Vector3 _linearVelocity;

	public HandGrabInteractor HandGrabber => _lastHandGrabInteractor;

	private void Awake()
	{
		Identifier = UniqueIdentifier.Generate(Context.Global.GetInstance(), this);
	}

	private void OnEnable()
	{
		HandGrabInteractable[] handGrabInteractables = _handGrabInteractables;
		for (int i = 0; i < handGrabInteractables.Length; i++)
		{
			handGrabInteractables[i].WhenSelectingInteractorAdded.Action += HandleSelectingHandGrabInteractorAdded;
		}
	}

	private void OnDisable()
	{
		HandGrabInteractable[] handGrabInteractables = _handGrabInteractables;
		for (int i = 0; i < handGrabInteractables.Length; i++)
		{
			handGrabInteractables[i].WhenSelectingInteractorAdded.Action -= HandleSelectingHandGrabInteractorAdded;
		}
	}

	private void HandleSelectingHandGrabInteractorAdded(HandGrabInteractor interactor)
	{
		_lastHandGrabInteractor = interactor;
	}

	public void Attach()
	{
		Pose pose = base.transform.GetPose();
		grabbable.ProcessPointerEvent(new PointerEvent(Identifier.ID, PointerEventType.Hover, pose));
		grabbable.ProcessPointerEvent(new PointerEvent(Identifier.ID, PointerEventType.Select, pose));
		grabbable.ProcessPointerEvent(new PointerEvent(Identifier.ID, PointerEventType.Move, pose));
	}

	public void Move(Transform transform)
	{
		grabbable.ProcessPointerEvent(new PointerEvent(Identifier.ID, PointerEventType.Move, transform.GetPose()));
	}

	public void Eject(Vector3 force)
	{
		grabbable.ProcessPointerEvent(new PointerEvent(Identifier.ID, PointerEventType.Cancel, base.transform.GetPose()));
		_linearVelocity = force;
		_hasPendingForce = true;
	}

	private void FixedUpdate()
	{
		if (_hasPendingForce)
		{
			_hasPendingForce = false;
			_rigidbody.AddForce(_linearVelocity, ForceMode.VelocityChange);
			_rigidbody.AddTorque(Vector3.zero, ForceMode.VelocityChange);
		}
	}
}
