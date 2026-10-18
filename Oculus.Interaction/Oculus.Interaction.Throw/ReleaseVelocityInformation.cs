using UnityEngine;

namespace Oculus.Interaction.Throw;

public struct ReleaseVelocityInformation
{
	public Vector3 LinearVelocity;

	public Vector3 AngularVelocity;

	public Vector3 Origin;

	public bool IsSelectedVelocity;

	public ReleaseVelocityInformation(Vector3 linearVelocity, Vector3 angularVelocity, Vector3 origin, bool isSelectedVelocity = false)
	{
		LinearVelocity = linearVelocity;
		AngularVelocity = angularVelocity;
		Origin = origin;
		IsSelectedVelocity = isSelectedVelocity;
	}
}
