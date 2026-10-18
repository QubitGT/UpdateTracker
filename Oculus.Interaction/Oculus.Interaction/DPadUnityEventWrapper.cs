using System;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction;

public class DPadUnityEventWrapper : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IAxis2D), new Type[] { })]
	private UnityEngine.Object _axis;

	[SerializeField]
	private float _positiveDeadZone = 0.9f;

	[SerializeField]
	private float _negativeDeadZone = 0.5f;

	[SerializeField]
	private UnityEvent _whenPressLeft;

	[SerializeField]
	private UnityEvent _whenPressRight;

	[SerializeField]
	private UnityEvent _whenPressUp;

	[SerializeField]
	private UnityEvent _whenPressDown;

	protected bool _started;

	private Vector2Int _lastDirection = Vector2Int.zero;

	private IAxis2D Axis { get; set; }

	public float PositiveDeadZone
	{
		get
		{
			return _positiveDeadZone;
		}
		set
		{
			_positiveDeadZone = value;
		}
	}

	public float NegativeDeadZone
	{
		get
		{
			return _negativeDeadZone;
		}
		set
		{
			_negativeDeadZone = value;
		}
	}

	public UnityEvent WhenPressLeft => _whenPressLeft;

	public UnityEvent WhenPressRight => _whenPressRight;

	public UnityEvent WhenPressUp => _whenPressUp;

	public UnityEvent WhenPressDown => _whenPressDown;

	protected virtual void Awake()
	{
		Axis = _axis as IAxis2D;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		_ = _started;
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_lastDirection = Vector2Int.zero;
		}
	}

	protected virtual void Update()
	{
		Vector2Int vector2Int = AxisToDPadDirection(Axis.Value());
		if (_lastDirection != vector2Int)
		{
			_lastDirection = vector2Int;
			if (vector2Int.x < 0)
			{
				_whenPressLeft.Invoke();
			}
			if (vector2Int.x > 0)
			{
				_whenPressRight.Invoke();
			}
			if (vector2Int.y < 0)
			{
				_whenPressDown.Invoke();
			}
			if (vector2Int.y > 0)
			{
				_whenPressUp.Invoke();
			}
		}
	}

	private Vector2Int AxisToDPadDirection(Vector2 axisValue)
	{
		Vector2Int zero = Vector2Int.zero;
		if (Mathf.Abs(axisValue.x) > _positiveDeadZone && Mathf.Abs(axisValue.y) < _negativeDeadZone)
		{
			zero.x = ((axisValue.x >= 0f) ? 1 : (-1));
		}
		if (Mathf.Abs(axisValue.y) > _positiveDeadZone && Mathf.Abs(axisValue.x) < _negativeDeadZone)
		{
			zero.y = ((axisValue.y >= 0f) ? 1 : (-1));
		}
		return zero;
	}

	public void InjectAllDPadUnityEventWrapper(IAxis2D axis)
	{
		InjectAxis(axis);
	}

	public void InjectAxis(IAxis2D axis)
	{
		Axis = axis;
		_axis = axis as UnityEngine.Object;
	}
}
