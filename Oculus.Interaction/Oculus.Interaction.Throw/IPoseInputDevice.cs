using UnityEngine;

namespace Oculus.Interaction.Throw;

public interface IPoseInputDevice
{
	bool IsInputValid { get; }

	bool IsHighConfidence { get; }

	bool GetRootPose(out Pose pose);

	(Vector3, Vector3) GetExternalVelocities();
}
