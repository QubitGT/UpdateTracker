using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class OneGrabPhysicsJointTransformer : MonoBehaviour, ITransformer
{
	[SerializeField]
	[Optional]
	[Tooltip("Specify a custom joint to use when grabbing; should be disabled.")]
	private ConfigurableJoint _customJoint;

	[SerializeField]
	[Tooltip("Indicates if the grabbing rigidbody should be kinematic or not.")]
	private bool _isKinematicGrab = true;

	[SerializeField]
	[Optional]
	[Tooltip("Newly created rigidbodies will be appended to this transform")]
	private Transform _rigidbodiesRoot;

	private Joint _joint;

	private Rigidbody _grabbingRigidbody;

	private static List<Rigidbody> _cachedGrabbingRigidbodies = new List<Rigidbody>();

	private IGrabbable _grabbable;

	private Vector3 _targetPosition;

	private Quaternion _targetRotation;

	public bool IsKinematicGrab
	{
		get
		{
			return _isKinematicGrab;
		}
		set
		{
			_isKinematicGrab = value;
		}
	}

	private void OnValidate()
	{
		if (_customJoint != null)
		{
			if (_customJoint.gameObject == base.gameObject)
			{
				Debug.LogWarning("The OptionalCustomJoint must be placed in a disabled child GameObject. Moving it.", base.gameObject);
				GameObject destination = CreateJointHolder();
				_customJoint = CloneJoint(_customJoint, destination);
			}
			else
			{
				_customJoint.gameObject.SetActive(value: false);
			}
		}
	}

	public void Initialize(IGrabbable grabbable)
	{
		_grabbable = grabbable;
	}

	public void BeginTransform()
	{
		Vector3 position = _grabbable.GrabPoints[0].position;
		Quaternion rotation = _grabbable.GrabPoints[0].rotation;
		_grabbingRigidbody = GetGrabRigidbody();
		_grabbingRigidbody.transform.SetPositionAndRotation(position, rotation);
		_joint = AddJoint(_grabbingRigidbody);
	}

	public void UpdateTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		_targetPosition = pose.position;
		_targetRotation = pose.rotation;
		if (_isKinematicGrab)
		{
			_grabbingRigidbody.transform.SetPositionAndRotation(_targetPosition, _targetRotation);
		}
	}

	private void FixedUpdate()
	{
		if (!_isKinematicGrab && _grabbingRigidbody != null)
		{
			_grabbingRigidbody.MovePosition(_targetPosition);
			_grabbingRigidbody.MoveRotation(_targetRotation);
		}
	}

	public void EndTransform()
	{
		RemoveCurrentJoint();
		RemoveCurrentGrabRigidbody();
	}

	private Joint AddJoint(Rigidbody rigidbody)
	{
		RemoveCurrentJoint();
		Joint joint = ((!(_customJoint != null)) ? CreateDefaultJoint() : CloneJoint(_customJoint, base.gameObject));
		joint.connectedBody = rigidbody;
		joint.autoConfigureConnectedAnchor = false;
		joint.anchor = joint.transform.InverseTransformPoint(rigidbody.transform.position);
		joint.connectedAnchor = Vector3.zero;
		return joint;
	}

	private void RemoveCurrentJoint()
	{
		if (_joint != null)
		{
			Object.Destroy(_joint);
		}
	}

	private Rigidbody GetGrabRigidbody()
	{
		Rigidbody rigidbody = _cachedGrabbingRigidbodies.Find((Rigidbody rb) => rb != null && !rb.gameObject.activeSelf);
		if (rigidbody == null)
		{
			rigidbody = CreateRigidBody();
			_cachedGrabbingRigidbodies.Add(rigidbody);
		}
		rigidbody.gameObject.SetActive(value: true);
		rigidbody.isKinematic = _isKinematicGrab;
		return rigidbody;
	}

	private void RemoveCurrentGrabRigidbody()
	{
		if (_grabbingRigidbody != null)
		{
			_grabbingRigidbody.gameObject.SetActive(value: false);
			_grabbingRigidbody.isKinematic = true;
			_grabbingRigidbody = null;
		}
	}

	private Rigidbody CreateRigidBody()
	{
		GameObject obj = new GameObject();
		obj.name = "Proxy RigidBody";
		obj.SetActive(value: false);
		obj.transform.SetParent(_rigidbodiesRoot);
		Rigidbody rigidbody = obj.AddComponent<Rigidbody>();
		rigidbody.useGravity = false;
		rigidbody.isKinematic = false;
		return rigidbody;
	}

	private Joint CreateDefaultJoint()
	{
		FixedJoint fixedJoint = base.gameObject.AddComponent<FixedJoint>();
		fixedJoint.breakForce = float.PositiveInfinity;
		fixedJoint.enablePreprocessing = false;
		return fixedJoint;
	}

	protected GameObject CreateJointHolder()
	{
		GameObject obj = new GameObject();
		obj.name = "Saved Joint";
		obj.SetActive(value: false);
		obj.transform.SetParent(base.transform);
		obj.AddComponent<Rigidbody>().isKinematic = true;
		return obj;
	}

	private static ConfigurableJoint CloneJoint(ConfigurableJoint joint, GameObject destination)
	{
		ConfigurableJoint configurableJoint = destination.gameObject.AddComponent<ConfigurableJoint>();
		configurableJoint.connectedBody = joint.connectedBody;
		configurableJoint.axis = joint.axis;
		configurableJoint.anchor = joint.anchor;
		configurableJoint.connectedAnchor = joint.connectedAnchor;
		configurableJoint.autoConfigureConnectedAnchor = joint.autoConfigureConnectedAnchor;
		configurableJoint.breakForce = joint.breakForce;
		configurableJoint.breakTorque = joint.breakTorque;
		configurableJoint.enableCollision = joint.enableCollision;
		configurableJoint.enablePreprocessing = joint.enablePreprocessing;
		configurableJoint.massScale = joint.massScale;
		configurableJoint.connectedMassScale = joint.connectedMassScale;
		configurableJoint.projectionAngle = joint.projectionAngle;
		configurableJoint.projectionDistance = joint.projectionDistance;
		configurableJoint.projectionMode = joint.projectionMode;
		configurableJoint.slerpDrive = joint.slerpDrive;
		configurableJoint.angularYZDrive = joint.angularYZDrive;
		configurableJoint.angularXDrive = joint.angularXDrive;
		configurableJoint.rotationDriveMode = joint.rotationDriveMode;
		configurableJoint.targetAngularVelocity = joint.targetAngularVelocity;
		configurableJoint.targetRotation = joint.targetRotation;
		configurableJoint.zDrive = joint.zDrive;
		configurableJoint.yDrive = joint.yDrive;
		configurableJoint.xDrive = joint.xDrive;
		configurableJoint.targetVelocity = joint.targetVelocity;
		configurableJoint.targetPosition = joint.targetPosition;
		configurableJoint.angularZLimit = joint.angularZLimit;
		configurableJoint.angularYLimit = joint.angularYLimit;
		configurableJoint.highAngularXLimit = joint.highAngularXLimit;
		configurableJoint.lowAngularXLimit = joint.lowAngularXLimit;
		configurableJoint.linearLimit = joint.linearLimit;
		configurableJoint.angularYZLimitSpring = joint.angularYZLimitSpring;
		configurableJoint.angularXLimitSpring = joint.angularXLimitSpring;
		configurableJoint.linearLimitSpring = joint.linearLimitSpring;
		configurableJoint.angularZMotion = joint.angularZMotion;
		configurableJoint.angularYMotion = joint.angularYMotion;
		configurableJoint.angularXMotion = joint.angularXMotion;
		configurableJoint.zMotion = joint.zMotion;
		configurableJoint.yMotion = joint.yMotion;
		configurableJoint.xMotion = joint.xMotion;
		configurableJoint.secondaryAxis = joint.secondaryAxis;
		configurableJoint.configuredInWorldSpace = joint.configuredInWorldSpace;
		configurableJoint.swapBodies = joint.swapBodies;
		return configurableJoint;
	}

	public void InjectOptionalCustomJoint(ConfigurableJoint customJoint)
	{
		_customJoint = customJoint;
	}

	public void InjectOptionalRigidbodiesRoot(Transform rigidbodiesRoot)
	{
		_rigidbodiesRoot = rigidbodiesRoot;
	}
}
