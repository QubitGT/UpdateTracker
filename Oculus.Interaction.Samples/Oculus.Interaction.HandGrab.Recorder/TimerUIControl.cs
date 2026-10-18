using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction.HandGrab.Recorder;

public class TimerUIControl : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _timerLabel;

	[SerializeField]
	private int _delaySeconds = 3;

	[SerializeField]
	private int _maxSeconds = 10;

	[SerializeField]
	private Button _moreButton;

	[SerializeField]
	private Button _lessButton;

	public int DelaySeconds
	{
		get
		{
			return _delaySeconds;
		}
		set
		{
			_delaySeconds = Mathf.Clamp(value, 0, _maxSeconds);
			UpdateDisplay(value);
		}
	}

	private void OnEnable()
	{
		_moreButton.onClick.AddListener(IncreaseTime);
		_lessButton.onClick.AddListener(DecreaseTime);
	}

	private void OnDisable()
	{
		_moreButton.onClick.RemoveListener(IncreaseTime);
		_lessButton.onClick.RemoveListener(DecreaseTime);
	}

	private void Start()
	{
		UpdateDisplay(DelaySeconds);
	}

	private void IncreaseTime()
	{
		DelaySeconds++;
	}

	private void DecreaseTime()
	{
		DelaySeconds--;
	}

	private void UpdateDisplay(int seconds)
	{
		_timerLabel.text = $"{seconds}\nseconds";
		_lessButton.interactable = seconds > 0;
		_moreButton.interactable = seconds < _maxSeconds;
	}
}
