using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction;

[Obsolete("Use Grabbable and/or RigidbodyKinematicLocker instead")]
public class PhysicsGrabbable : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IPointable), new Type[] { })]
	[FormerlySerializedAs("_grabbable")]
	private UnityEngine.Object _pointable;

	[SerializeField]
	private Rigidbody _rigidbody;

	[SerializeField]
	[Tooltip("If enabled, the object's mass will scale appropriately as the scale of the object changes.")]
	private bool _scaleMassWithSize = true;

	private Vector3 _initialScale;

	private bool _hasPendingForce;

	private Vector3 _linearVelocity;

	private Vector3 _angularVelocity;

	private int _selectorsCount;

	protected bool _started;

	private IPointable Pointable { get; set; }

	public event Action<Vector3, Vector3> WhenVelocitiesApplied = delegate
	{
	};

	private void Reset()
	{
		_pointable = GetComponent<IPointable>() as UnityEngine.Object;
		_rigidbody = GetComponent<Rigidbody>();
	}

	protected virtual void Awake()
	{
		Pointable = _pointable as IPointable;
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
			Pointable.WhenPointerEventRaised += HandlePointerEventRaised;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Pointable.WhenPointerEventRaised -= HandlePointerEventRaised;
			if (_selectorsCount != 0)
			{
				_selectorsCount = 0;
				ReenablePhysics();
			}
		}
	}

	private void HandlePointerEventRaised(PointerEvent evt)
	{
		switch (evt.Type)
		{
		case PointerEventType.Select:
			AddSelection();
			break;
		case PointerEventType.Unselect:
		case PointerEventType.Cancel:
			RemoveSelection();
			break;
		case PointerEventType.Move:
			break;
		}
	}

	private void AddSelection()
	{
		if (_selectorsCount++ == 0)
		{
			DisablePhysics();
		}
	}

	private void RemoveSelection()
	{
		if (--_selectorsCount == 0)
		{
			ReenablePhysics();
		}
		_selectorsCount = Mathf.Max(0, _selectorsCount);
	}

	private void DisablePhysics()
	{
		CachePhysicsState();
		_rigidbody.LockKinematic();
	}

	private void ReenablePhysics()
	{
		if (_scaleMassWithSize)
		{
			float num = _initialScale.x * _initialScale.y * _initialScale.z;
			Vector3 localScale = _rigidbody.transform.localScale;
			float num2 = localScale.x * localScale.y * localScale.z / num;
			_rigidbody.mass *= num2;
		}
		_rigidbody.UnlockKinematic();
	}

	public void ApplyVelocities(Vector3 linearVelocity, Vector3 angularVelocity)
	{
		_hasPendingForce = true;
		_linearVelocity = linearVelocity;
		_angularVelocity = angularVelocity;
	}

	private void FixedUpdate()
	{
		if (_hasPendingForce)
		{
			_hasPendingForce = false;
			_rigidbody.AddForce(_linearVelocity, ForceMode.VelocityChange);
			_rigidbody.AddTorque(_angularVelocity, ForceMode.VelocityChange);
			this.WhenVelocitiesApplied(_linearVelocity, _angularVelocity);
		}
	}

	private void CachePhysicsState()
	{
		_initialScale = _rigidbody.transform.localScale;
	}

	public void InjectAllPhysicsGrabbable(IPointable pointable, Rigidbody rigidbody)
	{
		InjectPointable(pointable);
		InjectRigidbody(rigidbody);
	}

	[Obsolete("Use InjectAllPhysicsGrabbable with IPointable instead")]
	public void InjectAllPhysicsGrabbable(Grabbable grabbable, Rigidbody rigidbody)
	{
		InjectPointable(grabbable);
		InjectRigidbody(rigidbody);
	}

	[Obsolete("Use InjectPointable instead")]
	public void InjectGrabbable(Grabbable grabbable)
	{
		InjectPointable(grabbable);
	}

	public void InjectPointable(IPointable pointable)
	{
		_pointable = pointable as UnityEngine.Object;
		Pointable = pointable;
	}

	public void InjectRigidbody(Rigidbody rigidbody)
	{
		_rigidbody = rigidbody;
	}

	public void InjectOptionalScaleMassWithSize(bool scaleMassWithSize)
	{
		_scaleMassWithSize = scaleMassWithSize;
	}
}
