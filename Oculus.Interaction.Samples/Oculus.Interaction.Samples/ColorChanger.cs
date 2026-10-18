using UnityEngine;

namespace Oculus.Interaction.Samples;

public class ColorChanger : MonoBehaviour
{
	[SerializeField]
	private Renderer _target;

	private Material _targetMaterial;

	private Color _savedColor;

	private float _lastHue;

	public void NextColor()
	{
		_lastHue = (_lastHue + 0.3f) % 1f;
		Color color = Color.HSVToRGB(_lastHue, 0.8f, 0.8f);
		_targetMaterial.color = color;
	}

	public void Save()
	{
		_savedColor = _targetMaterial.color;
	}

	public void Revert()
	{
		_targetMaterial.color = _savedColor;
	}

	protected virtual void Start()
	{
		_targetMaterial = _target.material;
		_savedColor = _targetMaterial.color;
	}

	private void OnDestroy()
	{
		Object.Destroy(_targetMaterial);
	}
}
