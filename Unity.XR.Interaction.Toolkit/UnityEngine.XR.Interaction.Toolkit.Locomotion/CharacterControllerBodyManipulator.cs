using Unity.XR.CoreUtils;

namespace UnityEngine.XR.Interaction.Toolkit.Locomotion;

[CreateAssetMenu(fileName = "CharacterControllerBodyManipulator", menuName = "XR/Locomotion/Character Controller Body Manipulator")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Locomotion.CharacterControllerBodyManipulator.html")]
public class CharacterControllerBodyManipulator : ScriptableConstrainedBodyManipulator
{
	public override CollisionFlags lastCollisionFlags
	{
		get
		{
			if (!(characterController != null))
			{
				return CollisionFlags.None;
			}
			return characterController.collisionFlags;
		}
	}

	public override bool isGrounded
	{
		get
		{
			if (!(characterController == null))
			{
				return characterController.isGrounded;
			}
			return true;
		}
	}

	public CharacterController characterController { get; private set; }

	public override void OnLinkedToBody(XRMovableBody body)
	{
		base.OnLinkedToBody(body);
		XROrigin xrOrigin = body.xrOrigin;
		GameObject origin = xrOrigin.Origin;
		if (!origin.TryGetComponent<CharacterController>(out var component) && origin != xrOrigin.gameObject)
		{
			xrOrigin.TryGetComponent<CharacterController>(out component);
		}
		if (component != null)
		{
			characterController = component;
			return;
		}
		Debug.LogWarning("No CharacterController found. Adding one to Origin GameObject '" + origin.name + "'.", this);
		characterController = origin.AddComponent<CharacterController>();
	}

	public override void OnUnlinkedFromBody()
	{
		base.OnUnlinkedFromBody();
		characterController = null;
	}

	public override CollisionFlags MoveBody(Vector3 motion)
	{
		if (base.linkedBody == null || characterController == null)
		{
			return CollisionFlags.None;
		}
		XROrigin xrOrigin = base.linkedBody.xrOrigin;
		Vector3 bodyGroundLocalPosition = base.linkedBody.GetBodyGroundLocalPosition();
		float num = xrOrigin.CameraInOriginSpaceHeight - bodyGroundLocalPosition.y;
		characterController.height = num;
		characterController.center = new Vector3(bodyGroundLocalPosition.x, bodyGroundLocalPosition.y + num * 0.5f + characterController.skinWidth, bodyGroundLocalPosition.z);
		return characterController.Move(motion);
	}
}
