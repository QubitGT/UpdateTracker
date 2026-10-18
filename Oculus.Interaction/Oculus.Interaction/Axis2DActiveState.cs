using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class Axis2DActiveState : MonoBehaviour, IActiveState
{
	public enum CheckComponent
	{
		Any,
		X,
		Y,
		All
	}

	public enum ComparisonMode
	{
		GreaterThan,
		LessThan
	}

	[SerializeField]
	[Interface(typeof(IAxis2D), new Type[] { })]
	private UnityEngine.Object _inputAxis;

	[SerializeField]
	private CheckComponent _checkAxis = CheckComponent.Y;

	[SerializeField]
	private ComparisonMode _comparison;

	[SerializeField]
	private bool _absoluteValues;

	[SerializeField]
	private Vector2 _thresold = new Vector2(0f, 0.5f);

	protected bool _started;

	private IAxis2D InputAxis { get; set; }

	public CheckComponent CheckAxis
	{
		get
		{
			return _checkAxis;
		}
		set
		{
			_checkAxis = value;
		}
	}

	public ComparisonMode Comparison
	{
		get
		{
			return _comparison;
		}
		set
		{
			_comparison = value;
		}
	}

	public bool AbsoluteValues
	{
		get
		{
			return _absoluteValues;
		}
		set
		{
			_absoluteValues = value;
		}
	}

	public bool Active { get; private set; }

	protected virtual void Awake()
	{
		InputAxis = _inputAxis as IAxis2D;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Active = false;
		}
	}

	protected virtual void Update()
	{
		HandleValueUpdated(InputAxis.Value());
	}

	private void HandleValueUpdated(Vector2 value)
	{
		if (AbsoluteValues)
		{
			value.x = Mathf.Abs(value.x);
			value.y = Mathf.Abs(value.y);
		}
		Active = ((Comparison == ComparisonMode.GreaterThan) ? CheckGreaterThan(value) : CheckLessThan(value));
	}

	private bool CheckGreaterThan(Vector2 value)
	{
		if (CheckAxis != CheckComponent.X)
		{
			if (CheckAxis != CheckComponent.Y)
			{
				if (CheckAxis != 0)
				{
					if (CheckAxis != CheckComponent.All)
					{
						return false;
					}
					if (value.y > _thresold.y)
					{
						return value.x > _thresold.x;
					}
					return false;
				}
				if (!(value.y > _thresold.y))
				{
					return value.x > _thresold.x;
				}
				return true;
			}
			return value.y > _thresold.y;
		}
		return value.x > _thresold.x;
	}

	private bool CheckLessThan(Vector2 value)
	{
		if (CheckAxis != CheckComponent.X)
		{
			if (CheckAxis != CheckComponent.Y)
			{
				if (CheckAxis != 0)
				{
					if (CheckAxis != CheckComponent.All)
					{
						return false;
					}
					if (value.y < _thresold.y)
					{
						return value.x < _thresold.x;
					}
					return false;
				}
				if (!(value.y < _thresold.y))
				{
					return value.x < _thresold.x;
				}
				return true;
			}
			return value.y < _thresold.y;
		}
		return value.x < _thresold.x;
	}
}
