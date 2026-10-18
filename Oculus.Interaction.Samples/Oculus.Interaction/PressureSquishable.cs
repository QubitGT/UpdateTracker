using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Oculus.Interaction;

public class PressureSquishable : MonoBehaviour, IHandGrabUseDelegate
{
	[SerializeField]
	private GameObject _squishableObject;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float _maxSquish = 0.25f;

	[SerializeField]
	[Range(0.01f, 1f)]
	private float _maxStretch = 0.15f;

	protected bool _started;

	private Vector3 _initialScale;

	protected virtual void Start()
	{
		_initialScale = _squishableObject.transform.localScale;
	}

	public void BeginUse()
	{
	}

	public void EndUse()
	{
		_squishableObject.transform.localScale = _initialScale;
	}

	public float ComputeUseStrength(float strength)
	{
		float num = Mathf.Lerp(1f, 1f - _maxSquish, strength);
		float num2 = Mathf.Lerp(1f, 1f + _maxStretch, strength);
		_squishableObject.transform.localScale = new Vector3(_initialScale.x * num2, _initialScale.y * num, _initialScale.z * num2);
		return strength;
	}
}
