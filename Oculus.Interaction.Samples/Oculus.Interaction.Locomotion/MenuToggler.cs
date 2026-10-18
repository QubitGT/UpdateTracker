using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction.Locomotion;

public class MenuToggler : MonoBehaviour
{
	[SerializeField]
	private GameObject _panel;

	[SerializeField]
	[Optional]
	private Button _closeButton;

	[SerializeField]
	private Transform _headAnchor;

	[SerializeField]
	private Vector3 _spawnOffset = new Vector3(0f, -0.1f, 0.3f);

	protected bool _started;

	public Transform HeadAnchor
	{
		get
		{
			return _headAnchor;
		}
		set
		{
			_headAnchor = value;
		}
	}

	public Vector3 SpawnOffset
	{
		get
		{
			return _spawnOffset;
		}
		set
		{
			_spawnOffset = value;
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			if (_closeButton != null)
			{
				_closeButton.onClick.AddListener(HidePanel);
			}
			if (!_panel.activeSelf)
			{
				HidePanel();
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (_started && _closeButton != null)
		{
			_closeButton.onClick.RemoveListener(HidePanel);
		}
	}

	public void TogglePanel()
	{
		if (_panel.activeSelf)
		{
			HidePanel();
		}
		else
		{
			ShowPanel();
		}
	}

	public void HidePanel()
	{
		_panel.SetActive(value: false);
	}

	public void ShowPanel()
	{
		Quaternion quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(_headAnchor.forward, Vector3.up).normalized);
		Vector3 position = _headAnchor.position + quaternion * _spawnOffset;
		quaternion *= Quaternion.Euler(15f, 0f, 0f);
		Pose pose = new Pose(position, quaternion);
		_panel.transform.SetPose(in pose);
		_panel.SetActive(value: true);
	}

	public void InjectAllAUIToggler(GameObject panel)
	{
		InjectPanel(panel);
	}

	public void InjectPanel(GameObject panel)
	{
		_panel = panel;
	}

	public void InjectOptionalCloseButton(Button closeButton)
	{
		_closeButton = closeButton;
	}
}
