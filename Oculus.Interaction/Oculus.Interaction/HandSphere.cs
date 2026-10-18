using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public struct HandSphere
{
	public Vector3 Position { get; }

	public float Radius { get; }

	public HandJointId Joint { get; }

	public HandSphere(Vector3 position, float radius, HandJointId joint)
	{
		Position = position;
		Radius = radius;
		Joint = joint;
	}
}
