using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction.Body.Samples;

public class PoseCaptureCountdown : MonoBehaviour
{
	[SerializeField]
	private UnityEvent _timerStart = new UnityEvent();

	[SerializeField]
	private UnityEvent _timerSecondTick = new UnityEvent();

	[SerializeField]
	private UnityEvent _timeUp = new UnityEvent();

	[SerializeField]
	private TextMeshProUGUI _countdownText;

	[SerializeField]
	private string _poseText = "Capture Pose";

	[SerializeField]
	private float duration = 10f;

	[SerializeField]
	[Optional]
	private Renderer _renderer;

	[SerializeField]
	[Optional]
	private Color _resetColor;

	private float _timer;

	public void Restart()
	{
		_timer = duration;
		_timerStart.Invoke();
		if (_renderer != null)
		{
			_renderer.material.color = _resetColor;
		}
	}

	private void Update()
	{
		bool num = _timer > 0f;
		if (num)
		{
			int num2 = (int)_timer;
			_timer -= Time.unscaledDeltaTime;
			if ((int)_timer < num2)
			{
				_timerSecondTick.Invoke();
			}
		}
		bool flag = _timer > 0f;
		if (num && !flag)
		{
			_timer = 0f;
			_timeUp.Invoke();
			_countdownText.text = _poseText;
		}
		else if (flag)
		{
			_countdownText.text = _timer.ToString("#0.0");
		}
	}
}
