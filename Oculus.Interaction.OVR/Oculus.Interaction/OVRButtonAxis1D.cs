using Meta.XR.Util;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

[Feature(Feature.Interaction)]
public class OVRButtonAxis1D : MonoBehaviour, IAxis1D
{
	[SerializeField]
	private OVRInput.Controller _controller;

	[SerializeField]
	private OVRInput.Button _near;

	[SerializeField]
	private OVRInput.Button _touch;

	[SerializeField]
	private OVRInput.Button _button;

	[SerializeField]
	private float _nearValue = 0.1f;

	[SerializeField]
	private float _touchValue = 0.5f;

	[SerializeField]
	private float _buttonValue = 1f;

	[SerializeField]
	private ProgressCurve _curve = new ProgressCurve(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f), 0.1f);

	private float _baseValue;

	private float _value;

	private float _currentTarget;

	public float NearValue
	{
		get
		{
			return _nearValue;
		}
		set
		{
			_nearValue = value;
		}
	}

	public float TouchValue
	{
		get
		{
			return _touchValue;
		}
		set
		{
			_touchValue = value;
		}
	}

	public float ButtonValue
	{
		get
		{
			return _buttonValue;
		}
		set
		{
			_buttonValue = value;
		}
	}

	private float Target
	{
		get
		{
			if (OVRInput.Get(_button, _controller))
			{
				return _buttonValue;
			}
			if (OVRInput.Get(_touch, _controller))
			{
				return _touchValue;
			}
			if (OVRInput.Get(_near, _controller))
			{
				return _nearValue;
			}
			return 0f;
		}
	}

	public float Value()
	{
		return _value;
	}

	protected virtual void Update()
	{
		float target = Target;
		if (_currentTarget != target)
		{
			_baseValue = _value;
			_currentTarget = target;
			_curve.Start();
		}
		_value = _curve.Progress() * (_currentTarget - _baseValue);
	}

	public void InjectAllOVRButtonAxis1D(OVRInput.Controller controller, OVRInput.Button near, OVRInput.Button touch, OVRInput.Button button)
	{
		_controller = controller;
		_near = near;
		_touch = touch;
		_button = button;
	}

	public void InjectOptionalCurve(ProgressCurve progressCurve)
	{
		_curve = progressCurve;
	}
}
