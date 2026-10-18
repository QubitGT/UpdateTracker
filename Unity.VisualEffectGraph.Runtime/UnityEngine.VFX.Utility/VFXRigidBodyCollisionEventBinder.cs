namespace UnityEngine.VFX.Utility;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
internal class VFXRigidBodyCollisionEventBinder : VFXEventBinderBase
{
	private ExposedProperty positionParameter = "position";

	private ExposedProperty directionParameter = "velocity";

	protected override void SetEventAttribute(object[] parameters)
	{
		ContactPoint contactPoint = (ContactPoint)parameters[0];
		eventAttribute.SetVector3(positionParameter, contactPoint.point);
		eventAttribute.SetVector3(directionParameter, contactPoint.normal);
	}

	private void OnCollisionEnter(Collision collision)
	{
		ContactPoint[] contacts = collision.contacts;
		foreach (ContactPoint contactPoint in contacts)
		{
			SendEventToVisualEffect(contactPoint);
		}
	}
}
