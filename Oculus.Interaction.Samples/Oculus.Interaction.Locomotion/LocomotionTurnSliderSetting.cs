using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction.Locomotion;

public class LocomotionTurnSliderSetting : MonoBehaviour
{
	[SerializeField]
	private Slider _slider;

	[SerializeField]
	private Toggle _snapTurnToggle;

	[SerializeField]
	private Toggle _smoothTurnToggle;

	[SerializeField]
	private float[] _snapTurnSteps = new float[3] { 30f, 45f, 90f };

	[SerializeField]
	private AnimationCurve[] _smoothTurnSteps;

	[SerializeField]
	private TurnerEventBroadcaster[] _controllerTurners;

	[SerializeField]
	private TurnerEventBroadcaster[] _handTurners;

	[SerializeField]
	private TurnLocomotionBroadcaster[] _locomotionTurners;

	protected bool _started;

	protected void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_slider.onValueChanged.AddListener(HandleValueChanged);
			_snapTurnToggle.onValueChanged.AddListener(HandleSnapTurnChanged);
			_smoothTurnToggle.onValueChanged.AddListener(HandleSmoothTurnChanged);
			HandleValueChanged(_slider.value);
			HandleSnapTurnChanged(_snapTurnToggle.isOn);
			HandleSmoothTurnChanged(_smoothTurnToggle.isOn);
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_slider.onValueChanged.RemoveListener(HandleValueChanged);
			_snapTurnToggle.onValueChanged.RemoveListener(HandleSnapTurnChanged);
			_smoothTurnToggle.onValueChanged.RemoveListener(HandleSmoothTurnChanged);
		}
	}

	private void HandleValueChanged(float arg0)
	{
		int num = Mathf.RoundToInt(arg0);
		float snapTurnDegrees = _snapTurnSteps[num];
		AnimationCurve smoothTurnCurve = _smoothTurnSteps[num];
		TurnerEventBroadcaster[] controllerTurners = _controllerTurners;
		foreach (TurnerEventBroadcaster obj in controllerTurners)
		{
			obj.SnapTurnDegrees = snapTurnDegrees;
			obj.SmoothTurnCurve = smoothTurnCurve;
		}
		controllerTurners = _handTurners;
		foreach (TurnerEventBroadcaster obj2 in controllerTurners)
		{
			obj2.SnapTurnDegrees = snapTurnDegrees;
			obj2.SmoothTurnCurve = smoothTurnCurve;
		}
		TurnLocomotionBroadcaster[] locomotionTurners = _locomotionTurners;
		foreach (TurnLocomotionBroadcaster obj3 in locomotionTurners)
		{
			obj3.SnapTurnDegrees = snapTurnDegrees;
			obj3.SmoothTurnCurve = smoothTurnCurve;
		}
	}

	private void HandleSnapTurnChanged(bool snapTurn)
	{
		if (snapTurn)
		{
			TurnerEventBroadcaster[] controllerTurners = _controllerTurners;
			for (int i = 0; i < controllerTurners.Length; i++)
			{
				controllerTurners[i].TurnMethod = TurnerEventBroadcaster.TurnMode.Snap;
			}
		}
	}

	private void HandleSmoothTurnChanged(bool smoothTurn)
	{
		if (smoothTurn)
		{
			TurnerEventBroadcaster[] controllerTurners = _controllerTurners;
			for (int i = 0; i < controllerTurners.Length; i++)
			{
				controllerTurners[i].TurnMethod = TurnerEventBroadcaster.TurnMode.Smooth;
			}
		}
	}
}
