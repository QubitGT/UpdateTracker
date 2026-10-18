using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class TurnerEventBroadcaster : MonoBehaviour, ILocomotionEventBroadcaster
{
	public enum TurnMode
	{
		Snap,
		Smooth
	}

	[SerializeField]
	[Interface(typeof(IInteractor), new Type[] { })]
	[Tooltip("The interactor defines when the Locomotion events are sent based on its Select state.")]
	private UnityEngine.Object _interactor;

	[SerializeField]
	[Interface(typeof(IAxis1D), new Type[] { })]
	[Tooltip("Axis from -1 to 1 indicating the turning direction and velocity.")]
	private UnityEngine.Object _axis;

	[SerializeField]
	[Tooltip("Snap turn fires once during Select, while Smooth fires continuously during Select.")]
	private TurnMode _turnMethod;

	[SerializeField]
	[Tooltip("Degrees to instantly turn when in Snap turn mode. Note the direction is provided by the axis")]
	private float _snapTurnDegrees = 45f;

	[SerializeField]
	[Tooltip("Degrees to continuously rotate during selection when in Smooth turn mode, it is remapped from the Axis value")]
	private AnimationCurve _smoothTurnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 100f);

	[SerializeField]
	[Tooltip("When enabled, snap turn happens on unselect. If false it happens on select")]
	private bool _fireSnapOnUnselect = true;

	private UniqueIdentifier _identifier;

	private bool _wasSelecting;

	protected bool _started;

	private Action<LocomotionEvent> _whenLocomotionEventRaised = delegate
	{
	};

	private IInteractor Interactor { get; set; }

	private IAxis1D Axis { get; set; }

	public TurnMode TurnMethod
	{
		get
		{
			return _turnMethod;
		}
		set
		{
			_turnMethod = value;
		}
	}

	public float SnapTurnDegrees
	{
		get
		{
			return _snapTurnDegrees;
		}
		set
		{
			_snapTurnDegrees = value;
		}
	}

	public AnimationCurve SmoothTurnCurve
	{
		get
		{
			return _smoothTurnCurve;
		}
		set
		{
			_smoothTurnCurve = value;
		}
	}

	public bool FireSnapOnUnselect
	{
		get
		{
			return _fireSnapOnUnselect;
		}
		set
		{
			_fireSnapOnUnselect = value;
		}
	}

	public int Identifier => _identifier.ID;

	public event Action<LocomotionEvent> WhenLocomotionPerformed
	{
		add
		{
			_whenLocomotionEventRaised = (Action<LocomotionEvent>)Delegate.Combine(_whenLocomotionEventRaised, value);
		}
		remove
		{
			_whenLocomotionEventRaised = (Action<LocomotionEvent>)Delegate.Remove(_whenLocomotionEventRaised, value);
		}
	}

	protected virtual void Awake()
	{
		_identifier = UniqueIdentifier.Generate(Context.Global.GetInstance(), this);
		Interactor = _interactor as IInteractor;
		Axis = _axis as IAxis1D;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Interactor.WhenStateChanged += HandleStateChanged;
			Interactor.WhenPostprocessed += HandlePostprocessed;
			_wasSelecting = false;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Interactor.WhenStateChanged -= HandleStateChanged;
			Interactor.WhenPostprocessed -= HandlePostprocessed;
		}
	}

	private void HandleStateChanged(InteractorStateChangeArgs obj)
	{
		if (obj.PreviousState == InteractorState.Select)
		{
			_wasSelecting = _fireSnapOnUnselect;
		}
	}

	private void HandlePostprocessed()
	{
		if (_wasSelecting && _fireSnapOnUnselect)
		{
			_wasSelecting = false;
			if ((Interactor.State == InteractorState.Hover || Interactor.State == InteractorState.Normal) && _turnMethod == TurnMode.Snap)
			{
				SnapTurn(Axis.Value());
			}
		}
		if (Interactor.State == InteractorState.Select)
		{
			if (_turnMethod == TurnMode.Smooth)
			{
				SmoothTurn(Axis.Value());
			}
			else if (_turnMethod == TurnMode.Snap && !_fireSnapOnUnselect && !_wasSelecting)
			{
				_wasSelecting = true;
				SnapTurn(Axis.Value());
			}
		}
	}

	public void SnapTurn(float direction)
	{
		float num = Mathf.Sign(direction);
		Quaternion rotation = Quaternion.Euler(0f, num * _snapTurnDegrees, 0f);
		LocomotionEvent obj = new LocomotionEvent(Identifier, rotation, LocomotionEvent.RotationType.Relative);
		_whenLocomotionEventRaised(obj);
	}

	public void SmoothTurn(float direction)
	{
		float num = Mathf.Sign(direction);
		float num2 = _smoothTurnCurve.Evaluate(Mathf.Abs(direction));
		Quaternion rotation = Quaternion.Euler(0f, num * num2, 0f);
		LocomotionEvent obj = new LocomotionEvent(Identifier, rotation, LocomotionEvent.RotationType.Velocity);
		_whenLocomotionEventRaised(obj);
	}

	public void InjectAllTurnerEventBroadcaster(IInteractor interactor, IAxis1D axis)
	{
		InjectInteractor(interactor);
		InjectAxis(axis);
	}

	public void InjectInteractor(IInteractor interactor)
	{
		_interactor = interactor as UnityEngine.Object;
		Interactor = interactor;
	}

	public void InjectAxis(IAxis1D axis)
	{
		_axis = axis as UnityEngine.Object;
		Axis = axis;
	}
}
