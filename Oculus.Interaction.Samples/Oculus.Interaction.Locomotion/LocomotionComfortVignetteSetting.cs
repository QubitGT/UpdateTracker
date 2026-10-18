using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction.Locomotion;

public class LocomotionComfortVignetteSetting : MonoBehaviour
{
	public enum ComfortType
	{
		Turning,
		Accelerating,
		Moving
	}

	[SerializeField]
	private Toggle _toggle;

	[SerializeField]
	private ComfortType _comfortType;

	[SerializeField]
	private AnimationCurve _curve;

	[SerializeField]
	private LocomotionTunneling _tunneling;

	protected bool _started;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_toggle.onValueChanged.AddListener(InjectCurve);
			InjectCurve(_toggle.isOn);
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_toggle.onValueChanged.RemoveListener(InjectCurve);
		}
	}

	private void InjectCurve(bool inject)
	{
		if (inject)
		{
			switch (_comfortType)
			{
			case ComfortType.Turning:
				_tunneling.RotationStrength = _curve;
				break;
			case ComfortType.Accelerating:
				_tunneling.AccelerationStrength = _curve;
				break;
			case ComfortType.Moving:
				_tunneling.MovementStrength = _curve;
				break;
			}
		}
	}

	public void InjectAllComfortOption(ComfortType comfortType, Toggle toggle, AnimationCurve curve, LocomotionTunneling tunneling)
	{
		InjectComfortType(comfortType);
		InjectToggle(toggle);
		InjectCurve(curve);
		InjectTunneling(tunneling);
	}

	public void InjectComfortType(ComfortType comfortType)
	{
		_comfortType = comfortType;
	}

	public void InjectToggle(Toggle toggle)
	{
		_toggle = toggle;
	}

	public void InjectCurve(AnimationCurve curve)
	{
		_curve = curve;
	}

	public void InjectTunneling(LocomotionTunneling tunneling)
	{
		_tunneling = tunneling;
	}
}
