using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Gaze;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRAimAssist
{
	Vector3 GetAssistedVelocity(in Vector3 source, in Vector3 velocity, float gravity);

	Vector3 GetAssistedVelocity(in Vector3 source, in Vector3 velocity, float gravity, float maxAngle);
}
