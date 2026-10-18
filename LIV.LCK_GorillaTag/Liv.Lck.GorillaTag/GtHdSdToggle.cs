using UnityEngine;
using UnityEngine.Events;

namespace Liv.Lck.GorillaTag;

public class GtHdSdToggle : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private string _hdLabelText;

	[SerializeField]
	private string _sdLabelText;

	[SerializeField]
	private bool _isHd;

	[Space(10f)]
	[Header("Elements")]
	[SerializeField]
	private GtButton _hdSdButton;

	[Space(10f)]
	[Header("Events")]
	public UnityEvent<bool> OnHdModeChanged;

	public GtButton Button => _hdSdButton;

	public void SetIsHdNoNotify(bool value)
	{
		_isHd = value;
		UpdateUi();
	}

	private void OnEnable()
	{
		_hdSdButton.onTap.AddListener(ProcessHdSdToggle);
	}

	private void OnDisable()
	{
		_hdSdButton.onTap.RemoveListener(ProcessHdSdToggle);
	}

	private void Start()
	{
		UpdateUi();
	}

	private void UpdateUi()
	{
		string labelText = (_isHd ? _hdLabelText : _sdLabelText);
		_hdSdButton.SetLabelText(labelText);
	}

	private void ProcessHdSdToggle()
	{
		_isHd = !_isHd;
		UpdateUi();
		OnHdModeChanged.Invoke(_isHd);
	}
}
