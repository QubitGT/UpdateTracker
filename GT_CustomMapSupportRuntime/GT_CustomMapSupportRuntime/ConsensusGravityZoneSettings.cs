using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class ConsensusGravityZoneSettings : BasicGravityZoneSettings
{
	[SerializeField]
	public float weightForce;

	[SerializeField]
	public float centeringForce;

	[SerializeField]
	public float drag;

	[SerializeField]
	public float rotMin = -45f;

	[SerializeField]
	public float rotMax = 45f;
}
