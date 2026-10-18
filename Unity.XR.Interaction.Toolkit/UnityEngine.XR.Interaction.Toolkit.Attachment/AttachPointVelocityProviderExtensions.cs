using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Attachment;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit.Interaction")]
public static class AttachPointVelocityProviderExtensions
{
	public static Vector3 GetAttachPointVelocity(this IAttachPointVelocityProvider provider, Transform xrOriginTransform)
	{
		return xrOriginTransform.TransformDirection(provider.GetAttachPointVelocity());
	}

	public static Vector3 GetAttachPointAngularVelocity(this IAttachPointVelocityProvider provider, Transform xrOriginTransform)
	{
		return xrOriginTransform.TransformDirection(provider.GetAttachPointAngularVelocity());
	}
}
