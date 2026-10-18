using TMPro;
using UnityEngine;

namespace Oculus.Interaction;

public class VersionTextVisual : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private string _format = "Version: {0}";

	protected virtual void Reset()
	{
		_text = GetComponent<TMP_Text>();
	}

	protected virtual void Start()
	{
		_text.text = string.Format(_format, Application.version);
	}

	public void InjectAllVersionTextVisual(TMP_Text text, string format)
	{
		InjectText(text);
		InjectFormat(format);
	}

	public void InjectText(TMP_Text text)
	{
		_text = text;
	}

	public void InjectFormat(string format)
	{
		_format = format;
	}
}
