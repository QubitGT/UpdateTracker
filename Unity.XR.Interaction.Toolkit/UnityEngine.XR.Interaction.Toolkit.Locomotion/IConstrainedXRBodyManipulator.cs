namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public interface IConstrainedXRBodyManipulator
{
	XRMovableBody linkedBody { get; }

	CollisionFlags lastCollisionFlags { get; }

	bool isGrounded { get; }

	void OnLinkedToBody(XRMovableBody body);

	void OnUnlinkedFromBody();

	CollisionFlags MoveBody(Vector3 motion);
}
