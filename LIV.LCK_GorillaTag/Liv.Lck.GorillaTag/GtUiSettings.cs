using UnityEngine;

namespace Liv.Lck.GorillaTag;

[CreateAssetMenu(fileName = "LIV/GT UI Settings", menuName = "GT UI Settings", order = 0)]
public class GtUiSettings : ScriptableObject
{
	[Header("Body Materials")]
	[SerializeField]
	private Material _defaultBodyMaterial;

	[SerializeField]
	private Material _selectedBodyMaterial;

	[SerializeField]
	private Material _recordingBodyMaterial;

	[Space(10f)]
	[Header("UI")]
	[SerializeField]
	private Color _primaryColor;

	[Space(10f)]
	[Header("Text Colors")]
	[SerializeField]
	private Color _primaryTextColor;

	[SerializeField]
	private Color _secondaryTextColor;

	[SerializeField]
	private Color _disabledTextColor;

	[Space(10f)]
	[SerializeField]
	private Color _primaryCounterButtonDefaultColor;

	[SerializeField]
	private Color _primaryCounterButtonActiveColor;

	[Space(10f)]
	[Header("Icon Colors")]
	[SerializeField]
	private Color _primaryIconColor;

	[SerializeField]
	private Color _secondaryIconColor;

	[SerializeField]
	private Color _inactiveIconColor;

	[Space(10f)]
	[Header("Offsets")]
	[SerializeField]
	private float _activeButtonOffset;

	[SerializeField]
	private float _counterAngleOffset;

	[Space(10f)]
	[Header("Elements for Selector Modes")]
	[SerializeField]
	private CameraModeAsset _selfieMode;

	[SerializeField]
	private CameraModeAsset _firstPersonMode;

	[SerializeField]
	private CameraModeAsset _thirdPersonMode;

	[SerializeField]
	private CameraModeAsset _headsetMode;

	public Material DefaultBodyMaterial => _defaultBodyMaterial;

	public Material SelectedBodyMaterial => _selectedBodyMaterial;

	public Material RecordingBodyMaterial => _recordingBodyMaterial;

	public Color PrimaryColor => _primaryColor;

	public Color PrimaryTextColor => _primaryTextColor;

	public Color SecondaryTextColor => _secondaryTextColor;

	public Color DisabledTextColor => _disabledTextColor;

	public Color PrimaryCounterButtonDefaultColor => _primaryCounterButtonDefaultColor;

	public Color PrimaryCounterButtonActiveColor => _primaryCounterButtonActiveColor;

	public CameraModeAsset SelfieModeAsset => _selfieMode;

	public CameraModeAsset FirstPersonModeAsset => _firstPersonMode;

	public CameraModeAsset ThirdPersonModeAsset => _thirdPersonMode;

	public CameraModeAsset HeadsetModeAsset => _headsetMode;

	public float ActiveButtonOffset => _activeButtonOffset;

	public float CounterAngleOffset => _counterAngleOffset;

	public Color PrimaryIconColor => _primaryIconColor;

	public Color SecondaryIconColor => _secondaryIconColor;

	public Color InactiveIconColor => _inactiveIconColor;
}
