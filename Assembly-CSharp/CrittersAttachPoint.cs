using UnityEngine;

[AddComponentMenu("GorillaTag/Critters/Critters Attach Point")]
public class CrittersAttachPoint : CrittersActor
{
	public enum AnchoredLocationTypes
	{
		Arm,
		Chest,
		Back
	}

	public bool fixedOrientation = true;

	public AnchoredLocationTypes anchorLocation;

	public bool isLeft;

	public override void ProcessRemote()
	{
	}
}
