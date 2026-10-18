namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

public abstract class ScriptableConstrainedBodyManipulator : ScriptableObject, IConstrainedXRBodyManipulator
{
	public XRMovableBody linkedBody { get; private set; }

	public abstract CollisionFlags lastCollisionFlags { get; }

	public abstract bool isGrounded { get; }

	public virtual void OnLinkedToBody(XRMovableBody body)
	{
		linkedBody = body;
	}

	public virtual void OnUnlinkedFromBody()
	{
		linkedBody = null;
	}

	public abstract CollisionFlags MoveBody(Vector3 motion);
}
