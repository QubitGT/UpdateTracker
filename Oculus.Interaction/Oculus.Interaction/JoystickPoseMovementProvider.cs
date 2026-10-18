using System;
using System.Linq;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction;

public class JoystickPoseMovementProvider : MonoBehaviour, IMovementProvider
{
	[SerializeField]
	[Interface(typeof(IInteractableView), new Type[] { })]
	private MonoBehaviour _interactable;

	private IInteractableView _interactableView;

	[FormerlySerializedAs("moveSpeed")]
	[SerializeField]
	[Optional]
	[Tooltip("The speed at which movement occurs.")]
	private float _moveSpeed = 0.04f;

	[FormerlySerializedAs("rotationSpeed")]
	[SerializeField]
	[Optional]
	[Tooltip("The speed at which rotation occurs.")]
	private float _rotationSpeed = 1f;

	[SerializeField]
	[Optional]
	[Range(0f, 10f)]
	[Tooltip("The minimum distance along the Z-axis for the grabbed object.")]
	private float _minDistance = 0.1f;

	[SerializeField]
	[Optional]
	[Range(1f, 10f)]
	[Tooltip("The maximum distance along the Z-axis for the grabbed object.")]
	private float _maxDistance = 3f;

	private IInteractorView _latestSelectingInteractor;

	public float MoveSpeed
	{
		get
		{
			return _moveSpeed;
		}
		set
		{
			_moveSpeed = value;
		}
	}

	public float RotationSpeed
	{
		get
		{
			return _rotationSpeed;
		}
		set
		{
			_rotationSpeed = value;
		}
	}

	public float MinDistance
	{
		get
		{
			return _minDistance;
		}
		set
		{
			_minDistance = value;
		}
	}

	public float MaxDistance
	{
		get
		{
			return _maxDistance;
		}
		set
		{
			_maxDistance = value;
		}
	}

	private void Awake()
	{
		_interactableView = _interactable as IInteractableView;
	}

	private void OnEnable()
	{
		if (_interactableView != null)
		{
			_interactableView.WhenSelectingInteractorViewAdded += OnSelectingInteractorViewAdded;
			_interactableView.WhenSelectingInteractorViewRemoved += OnSelectingInteractorViewRemoved;
		}
	}

	private void OnDisable()
	{
		if (_interactableView != null)
		{
			_interactableView.WhenSelectingInteractorViewAdded -= OnSelectingInteractorViewAdded;
			_interactableView.WhenSelectingInteractorViewRemoved -= OnSelectingInteractorViewRemoved;
		}
	}

	private void OnSelectingInteractorViewAdded(IInteractorView interactor)
	{
		_latestSelectingInteractor = interactor;
	}

	private void OnSelectingInteractorViewRemoved(IInteractorView interactor)
	{
		if (_latestSelectingInteractor == interactor)
		{
			_latestSelectingInteractor = _interactableView.SelectingInteractorViews.LastOrDefault();
		}
	}

	public IMovement CreateMovement()
	{
		IController controller = null;
		if (_latestSelectingInteractor != null)
		{
			InteractorControllerDecorator.TryGetControllerForInteractor(_latestSelectingInteractor, out controller);
		}
		return new JoystickPoseMovement(controller, _moveSpeed, _rotationSpeed, _minDistance, _maxDistance);
	}
}
