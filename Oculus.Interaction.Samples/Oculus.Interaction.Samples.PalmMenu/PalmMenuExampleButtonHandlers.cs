using TMPro;
using UnityEngine;

namespace Oculus.Interaction.Samples.PalmMenu;

public class PalmMenuExampleButtonHandlers : MonoBehaviour
{
	[SerializeField]
	private GameObject _controlledObject;

	[SerializeField]
	private Color[] _colors;

	[SerializeField]
	private GameObject _rotationEnabledIcon;

	[SerializeField]
	private GameObject _rotationDisabledIcon;

	[SerializeField]
	private float _rotationLerpSpeed = 1f;

	[SerializeField]
	private TMP_Text _rotationDirectionText;

	[SerializeField]
	private string[] _rotationDirectionNames;

	[SerializeField]
	private GameObject[] _rotationDirectionIcons;

	[SerializeField]
	private Quaternion[] _rotationDirections;

	[SerializeField]
	private TMP_Text _elevationText;

	[SerializeField]
	private float _elevationChangeIncrement;

	[SerializeField]
	private float _elevationChangeLerpSpeed = 1f;

	[SerializeField]
	private TMP_Text _shapeNameText;

	[SerializeField]
	private string[] _shapeNames;

	[SerializeField]
	private Mesh[] _shapes;

	private int _currentColorIdx;

	private bool _rotationEnabled;

	private int _currentRotationDirectionIdx;

	private Vector3 _targetPosition;

	private int _currentShapeIdx;

	private void Start()
	{
		_currentColorIdx = _colors.Length;
		CycleColor();
		_rotationEnabled = false;
		ToggleRotationEnabled();
		_currentRotationDirectionIdx = _rotationDirections.Length;
		CycleRotationDirection();
		_targetPosition = _controlledObject.transform.position;
		IncrementElevation(up: true);
		IncrementElevation(up: false);
		_currentShapeIdx = _shapes.Length;
		CycleShape(cycleForward: true);
	}

	private void Update()
	{
		if (_rotationEnabled)
		{
			Quaternion quaternion = Quaternion.Slerp(Quaternion.identity, _rotationDirections[_currentRotationDirectionIdx], _rotationLerpSpeed * Time.deltaTime);
			_controlledObject.transform.rotation = quaternion * _controlledObject.transform.rotation;
		}
		_controlledObject.transform.position = Vector3.Lerp(_controlledObject.transform.position, _targetPosition, _elevationChangeLerpSpeed * Time.deltaTime);
	}

	public void CycleColor()
	{
		_currentColorIdx++;
		if (_currentColorIdx >= _colors.Length)
		{
			_currentColorIdx = 0;
		}
		_controlledObject.GetComponent<Renderer>().material.color = _colors[_currentColorIdx];
	}

	public void ToggleRotationEnabled()
	{
		_rotationEnabled = !_rotationEnabled;
		_rotationEnabledIcon.SetActive(!_rotationEnabled);
		_rotationDisabledIcon.SetActive(_rotationEnabled);
	}

	public void CycleRotationDirection()
	{
		_currentRotationDirectionIdx++;
		if (_currentRotationDirectionIdx >= _rotationDirections.Length)
		{
			_currentRotationDirectionIdx = 0;
		}
		int num = _currentRotationDirectionIdx + 1;
		if (num >= _rotationDirections.Length)
		{
			num = 0;
		}
		_rotationDirectionText.text = _rotationDirectionNames[num];
		for (int i = 0; i < _rotationDirections.Length; i++)
		{
			_rotationDirectionIcons[i].SetActive(i == num);
		}
	}

	public void IncrementElevation(bool up)
	{
		float num = _elevationChangeIncrement;
		if (!up)
		{
			num *= -1f;
		}
		_targetPosition = new Vector3(_targetPosition.x, Mathf.Clamp(_targetPosition.y + num, 0.2f, 2f), _targetPosition.z);
		_elevationText.text = "Elevation: " + _targetPosition.y.ToString("0.0");
	}

	public void CycleShape(bool cycleForward)
	{
		_currentShapeIdx += (cycleForward ? 1 : (-1));
		if (_currentShapeIdx >= _shapes.Length)
		{
			_currentShapeIdx = 0;
		}
		else if (_currentShapeIdx < 0)
		{
			_currentShapeIdx = _shapes.Length - 1;
		}
		_shapeNameText.text = _shapeNames[_currentShapeIdx];
		_controlledObject.GetComponent<MeshFilter>().mesh = _shapes[_currentShapeIdx];
	}
}
