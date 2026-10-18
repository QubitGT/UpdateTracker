using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Attachment;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit.Interaction")]
public interface IAttachPointVelocityTracker : IAttachPointVelocityProvider
{
	void UpdateAttachPointVelocityData(Transform attachTransform);

	void UpdateAttachPointVelocityData(Transform attachTransform, Transform xrOriginTransform);
}
