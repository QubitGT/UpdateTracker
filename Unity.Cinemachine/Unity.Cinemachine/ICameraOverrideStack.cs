using UnityEngine;

namespace Unity.Cinemachine;

public interface ICameraOverrideStack
{
	Vector3 DefaultWorldUp { get; }

	int SetCameraOverride(int overrideId, int priority, ICinemachineCamera camA, ICinemachineCamera camB, float weightB, float deltaTime);

	void ReleaseCameraOverride(int overrideId);
}
