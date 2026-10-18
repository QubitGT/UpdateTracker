using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction.Locomotion;

public class LocomotionSeatedSetting : MonoBehaviour
{
	[SerializeField]
	private Toggle _seated;

	[SerializeField]
	private Toggle _standing;

	[SerializeField]
	private FirstPersonLocomotor _locomotor;

	[SerializeField]
	private float _seatedHeightOffset = 0.5f;

	protected bool _started;

	public float SeatedHeightOffset
	{
		get
		{
			return _seatedHeightOffset;
		}
		set
		{
			_seatedHeightOffset = value;
		}
	}

	protected void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_seated.onValueChanged.AddListener(HandleSeatedChanged);
			_standing.onValueChanged.AddListener(HandleStandingChanged);
			if (_standing.isOn)
			{
				HandleStandingChanged(standing: true);
			}
			else
			{
				HandleSeatedChanged(seated: true);
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_seated.onValueChanged.RemoveListener(HandleSeatedChanged);
			_standing.onValueChanged.RemoveListener(HandleStandingChanged);
		}
	}

	private void HandleSeatedChanged(bool seated)
	{
		if (seated)
		{
			_locomotor.HeightOffset = _seatedHeightOffset;
		}
	}

	private void HandleStandingChanged(bool standing)
	{
		if (standing)
		{
			_locomotor.HeightOffset = 0f;
		}
	}

	public void InjectAllSeatedMode(Toggle seated, Toggle standing, FirstPersonLocomotor locomotor)
	{
		InjectSeated(seated);
		InjectStanding(standing);
		InjectLocomotor(locomotor);
	}

	public void InjectSeated(Toggle seated)
	{
		_seated = seated;
	}

	public void InjectStanding(Toggle standing)
	{
		_standing = standing;
	}

	public void InjectLocomotor(FirstPersonLocomotor locomotor)
	{
		_locomotor = locomotor;
	}
}
