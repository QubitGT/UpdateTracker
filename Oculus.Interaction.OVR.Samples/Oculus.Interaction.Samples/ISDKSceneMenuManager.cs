using UnityEngine;

namespace Oculus.Interaction.Samples;

public class ISDKSceneMenuManager : MonoBehaviour
{
	[Tooltip("The parent object of the menu")]
	[Header("Place the grabbable parent object here")]
	[SerializeField]
	private GameObject _menuParent;

	[Tooltip("The audio to play when showing the menu panel")]
	[Header("Place the menu open audio here")]
	[SerializeField]
	private AudioSource _showMenuAudio;

	[Tooltip("The audio to play when hiding the menu panel")]
	[Header("Place the menu hide audio here")]
	[SerializeField]
	private AudioSource _hideMenuAudio;

	[Tooltip("The location the menu should be spawning at")]
	[Header("The location the menu should be spawning at")]
	[SerializeField]
	private GameObject _spawnPoint;

	protected bool _started;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	public void ToggleMenu()
	{
		if (_menuParent.activeSelf)
		{
			_hideMenuAudio.Play();
			_menuParent.SetActive(value: false);
			return;
		}
		_showMenuAudio.Play();
		_menuParent.transform.position = _spawnPoint.transform.position;
		_menuParent.transform.rotation = _spawnPoint.transform.rotation;
		_menuParent.SetActive(value: true);
	}

	public void InjectAllMenuItems(GameObject parent, AudioSource show, AudioSource hide, GameObject spawnpoint)
	{
		InjectMenuParent(parent);
		InjectShowAudio(show);
		InjectHideAudio(hide);
		InjectSpawnPoint(spawnpoint);
	}

	public void InjectMenuParent(GameObject parent)
	{
		_menuParent = parent;
	}

	public void InjectShowAudio(AudioSource show)
	{
		_showMenuAudio = show;
	}

	public void InjectHideAudio(AudioSource hide)
	{
		_showMenuAudio = hide;
	}

	public void InjectSpawnPoint(GameObject spawnpoint)
	{
		_menuParent = spawnpoint;
	}
}
