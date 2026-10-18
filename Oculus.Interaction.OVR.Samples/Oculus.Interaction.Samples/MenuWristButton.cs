using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction.Samples;

public class MenuWristButton : MonoBehaviour
{
	[Header("The Toggle Button")]
	[Tooltip("Place the toggle on the wrist here")]
	[SerializeField]
	private Toggle _toggle;

	[Header("The Menu Manager")]
	[Tooltip("There should only be 1 ISDK manager in the scene loacted on the ISDKMenuManager.prefab")]
	[SerializeField]
	private ISDKSceneMenuManager _menuManager;

	protected bool _started;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			_toggle.onValueChanged.AddListener(OnToggleValueChanged);
		}
	}

	private void OnToggleValueChanged(bool value)
	{
		_menuManager.ToggleMenu();
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			_toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
		}
	}

	public void InjectAllMenuWrist(Toggle toggle, ISDKSceneMenuManager manager)
	{
		InjectToggle(toggle);
		InjectManager(manager);
	}

	public void InjectToggle(Toggle toggle)
	{
		_toggle = toggle;
	}

	public void InjectManager(ISDKSceneMenuManager manager)
	{
		_menuManager = manager;
	}
}
