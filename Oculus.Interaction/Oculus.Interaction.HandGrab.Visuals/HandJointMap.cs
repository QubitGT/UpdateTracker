using System;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab.Visuals;

[Serializable]
public class HandJointMap
{
	public HandJointId id;

	public Transform transform;

	public Vector3 rotationOffset;

	public Quaternion RotationOffset => Quaternion.Euler(rotationOffset);

	public Quaternion TrackedRotation => Quaternion.Inverse(RotationOffset) * transform.localRotation;
}
