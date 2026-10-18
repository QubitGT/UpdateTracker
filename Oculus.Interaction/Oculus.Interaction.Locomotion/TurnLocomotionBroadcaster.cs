using System;
using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class TurnLocomotionBroadcaster : MonoBehaviour, ILocomotionEventBroadcaster
{
	[SerializeField]
	[Tooltip("Degrees to instantly turn when in Snap turn mode. Note the direction is provided by the axis")]
	private float _snapTurnDegrees = 45f;

	[SerializeField]
	[Tooltip("Degrees to continuously rotate during selection when in Smooth turn mode, it is remapped from the Axis value")]
	private AnimationCurve _smoothTurnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 100f);

	private UniqueIdentifier _identifier;

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

	public int Identifier => _identifier.ID;

	public event Action<LocomotionEvent> WhenLocomotionPerformed = delegate
	{
	};

	protected virtual void Awake()
	{
		_identifier = UniqueIdentifier.Generate(Context.Global.GetInstance(), this);
	}

	public void SnapTurnLeft()
	{
		SnapTurn(-1f);
	}

	public void SnapTurnRight()
	{
		SnapTurn(1f);
	}

	public void SnapTurn(float direction)
	{
		float num = Mathf.Sign(direction);
		Quaternion rotation = Quaternion.Euler(0f, num * _snapTurnDegrees, 0f);
		LocomotionEvent obj = new LocomotionEvent(Identifier, rotation, LocomotionEvent.RotationType.Relative);
		this.WhenLocomotionPerformed(obj);
	}

	public void SmoothTurn(float direction)
	{
		float num = Mathf.Sign(direction);
		float num2 = _smoothTurnCurve.Evaluate(Mathf.Abs(direction));
		Quaternion rotation = Quaternion.Euler(0f, num * num2, 0f);
		LocomotionEvent obj = new LocomotionEvent(Identifier, rotation, LocomotionEvent.RotationType.Velocity);
		this.WhenLocomotionPerformed(obj);
	}
}
