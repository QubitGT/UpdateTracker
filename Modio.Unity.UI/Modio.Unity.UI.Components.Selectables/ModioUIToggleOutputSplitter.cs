using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.Selectables;

public class ModioUIToggleOutputSplitter : MonoBehaviour
{
	public Toggle.ToggleEvent onToggleOn = new Toggle.ToggleEvent();

	public Toggle.ToggleEvent onToggleOff = new Toggle.ToggleEvent();

	private Toggle _toggle;

	private bool _hasFiredEvent;

	private void Awake()
	{
		_toggle = GetComponent<Toggle>();
		_toggle.onValueChanged.AddListener(ToggleValueChanged);
	}

	private void Start()
	{
		if (_toggle.isOn && !_hasFiredEvent)
		{
			ToggleValueChanged(isOn: true);
		}
	}

	private void ToggleValueChanged(bool isOn)
	{
		_hasFiredEvent = true;
		if (isOn)
		{
			onToggleOn.Invoke(arg0: true);
		}
		else
		{
			onToggleOff.Invoke(arg0: false);
		}
	}
}
