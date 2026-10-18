using UnityEngine;

namespace Unity.Cinemachine;

public interface ISignalSource6D
{
	float SignalDuration { get; }

	void GetSignal(float timeSinceSignalStart, out Vector3 pos, out Quaternion rot);
}
