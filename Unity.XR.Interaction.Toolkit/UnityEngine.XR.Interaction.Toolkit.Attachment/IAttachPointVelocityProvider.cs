using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Attachment;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit.Interaction")]
public interface IAttachPointVelocityProvider
{
	Vector3 GetAttachPointVelocity();

	Vector3 GetAttachPointAngularVelocity();
}
