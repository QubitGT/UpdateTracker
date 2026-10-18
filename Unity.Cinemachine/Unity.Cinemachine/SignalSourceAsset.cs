using UnityEngine;

namespace Unity.Cinemachine;

public abstract class SignalSourceAsset : ScriptableObject, ISignalSource6D
{
	public abstract float SignalDuration { get; }

	public abstract void GetSignal(float timeSinceSignalStart, out Vector3 pos, out Quaternion rot);
}
