using System;
using UnityEngine;

namespace Technie.PhysicsCreator.Skinned;

[Serializable]
public class BoneData
{
	public string targetBoneName;

	public bool addRigidbody;

	public float mass = 1f;

	public float linearDrag;

	public float angularDrag = 0.05f;

	public bool isKinematic;

	public bool addJoint;

	public BoneJointType jointType;

	public Vector3 primaryAxis = Vector3.forward;

	public Vector3 secondaryAxis = Vector3.up;

	public float primaryLowerAngularLimit;

	public float primaryUpperAngularLimit;

	public float secondaryAngularLimit;

	public float tertiaryAngularLimit;

	public float translationLimit;

	public float linearDamping;

	public float angularDamping;

	public BoneData(Transform src)
	{
		targetBoneName = src.name;
	}

	public Vector3 GetThirdAxis()
	{
		return Vector3.Cross(primaryAxis, secondaryAxis);
	}
}
