using TMPro;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class FadeTextAfterActive : MonoBehaviour
{
	[SerializeField]
	private float _fadeOutTime;

	[SerializeField]
	private TextMeshPro _text;

	private float _timeLeft;

	protected virtual void OnEnable()
	{
		_timeLeft = _fadeOutTime;
		_text.fontMaterial.color = new Color(_text.color.r, _text.color.g, _text.color.b, 255f);
	}

	protected virtual void Update()
	{
		if (!(_timeLeft <= 0f))
		{
			float t = 1f - _timeLeft / _fadeOutTime;
			float a = Mathf.SmoothStep(1f, 0f, t);
			_text.color = new Color(_text.color.r, _text.color.g, _text.color.b, a);
			_timeLeft -= Time.deltaTime;
		}
	}
}
