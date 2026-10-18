using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.ServiceReferences;

public abstract class AudioInputServiceReference : MonoBehaviour, IAudioEventProvider
{
	public abstract IAudioInputEvents AudioEvents { get; }
}
