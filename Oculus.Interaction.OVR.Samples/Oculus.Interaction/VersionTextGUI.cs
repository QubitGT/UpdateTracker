using System;
using TMPro;
using UnityEngine;

namespace Oculus.Interaction;

[Obsolete("Use VersionTextVisual instead")]
public class VersionTextGUI : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI _text;

	protected bool _started;

	protected virtual void Reset()
	{
		_text = GetComponent<TextMeshProUGUI>();
	}

	protected virtual void Start()
	{
		_text.text = "Version: " + Application.version;
	}

	public void InjectAllVersionTextGuiVisual(TextMeshProUGUI text)
	{
		InjectTextUI(text);
	}

	public void InjectTextUI(TextMeshProUGUI text)
	{
		_text = text;
	}
}
