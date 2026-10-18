using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction;

public class TogglerActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	private Toggle _toggle;

	protected bool _started;

	public bool Active => _toggle.isOn;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	public void InjectAllTogglerActiveState(Toggle toggle)
	{
		InjectAllToggle(toggle);
	}

	public void InjectAllToggle(Toggle toggle)
	{
		_toggle = toggle;
	}
}
