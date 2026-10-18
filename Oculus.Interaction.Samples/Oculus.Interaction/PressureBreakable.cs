using System.Collections;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Oculus.Interaction;

public class PressureBreakable : MonoBehaviour, IHandGrabUseDelegate
{
	[SerializeField]
	[Range(0f, 1f)]
	private float _breakThreshold = 0.9f;

	[SerializeField]
	private GameObject _unbrokenObject;

	[SerializeField]
	private GameObject _brokenObject;

	[SerializeField]
	private Rigidbody[] _brokenBodies;

	[SerializeField]
	private HandGrabInteractable[] _grabInteractables;

	[Header("Break Effects")]
	[SerializeField]
	private float _explosionForce = 3f;

	[SerializeField]
	private float _explosionRadius = 0.5f;

	[SerializeField]
	private float _unbreakDelay = 3f;

	private float _useStrength;

	private bool _isBroken;

	private Pose[] _brokenBodiesInitialPoses;

	protected virtual void Awake()
	{
		_unbrokenObject.SetActive(!_isBroken);
		_brokenObject.SetActive(_isBroken);
	}

	protected virtual void Start()
	{
		_brokenBodiesInitialPoses = new Pose[_brokenBodies.Length];
		for (int i = 0; i < _brokenBodies.Length; i++)
		{
			Rigidbody rigidbody = _brokenBodies[i];
			_brokenBodiesInitialPoses[i] = new Pose(rigidbody.transform.localPosition, rigidbody.transform.localRotation);
		}
	}

	protected virtual void Update()
	{
		if (_useStrength >= _breakThreshold)
		{
			Break();
		}
	}

	public void BeginUse()
	{
	}

	public void EndUse()
	{
		_useStrength = 0f;
	}

	public float ComputeUseStrength(float strength)
	{
		_useStrength = strength;
		return _useStrength;
	}

	private void Break()
	{
		if (!_isBroken)
		{
			_isBroken = true;
			_unbrokenObject.SetActive(!_isBroken);
			HandGrabInteractable[] grabInteractables = _grabInteractables;
			for (int i = 0; i < grabInteractables.Length; i++)
			{
				grabInteractables[i].Disable();
			}
			_brokenObject.SetActive(_isBroken);
			Rigidbody[] brokenBodies = _brokenBodies;
			foreach (Rigidbody obj in brokenBodies)
			{
				obj.mass = 1f / (float)_brokenBodies.Length;
				obj.AddExplosionForce(_explosionForce, base.transform.position, _explosionRadius);
			}
			StartCoroutine(Unbreak());
		}
	}

	private IEnumerator Unbreak()
	{
		if (_isBroken)
		{
			yield return new WaitForSeconds(_unbreakDelay);
			_isBroken = false;
			_brokenObject.SetActive(_isBroken);
			for (int i = 0; i < _brokenBodies.Length; i++)
			{
				Rigidbody obj = _brokenBodies[i];
				Pose pose = _brokenBodiesInitialPoses[i];
				obj.velocity = Vector3.zero;
				obj.angularVelocity = Vector3.zero;
				obj.transform.localPosition = pose.position;
				obj.transform.localRotation = pose.rotation;
			}
			HandGrabInteractable[] grabInteractables = _grabInteractables;
			for (int j = 0; j < grabInteractables.Length; j++)
			{
				grabInteractables[j].Enable();
			}
			_unbrokenObject.SetActive(!_isBroken);
		}
	}
}
