using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction.Samples;

public class CountdownTimer : MonoBehaviour
{
	[SerializeField]
	[Min(0f)]
	private float _countdownTime = 1f;

	[SerializeField]
	private bool _countdownOn;

	[SerializeField]
	private UnityEvent _callback;

	[SerializeField]
	private UnityEvent<float> _progressCallback;

	private float _countdownTimer;

	public bool CountdownOn
	{
		get
		{
			return _countdownOn;
		}
		set
		{
			if (value && !_countdownOn)
			{
				_countdownTimer = _countdownTime;
			}
			_countdownOn = value;
		}
	}

	private void Awake()
	{
	}

	private void Update()
	{
		if (!_countdownOn || _countdownTimer < 0f)
		{
			_progressCallback.Invoke(0f);
			return;
		}
		_countdownTimer -= Time.deltaTime;
		if (_countdownTimer < 0f)
		{
			_countdownTimer = -1f;
			_callback.Invoke();
			_progressCallback.Invoke(1f);
		}
		else
		{
			_progressCallback.Invoke(1f - _countdownTimer / _countdownTime);
		}
	}
}
