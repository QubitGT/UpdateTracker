using UnityEngine.Playables;
using UnityEngine.VFX.Utility;

namespace UnityEngine.VFX;

internal class VisualEffectControlPlayableBehaviour : PlayableBehaviour
{
	public double clipStart { get; set; }

	public double clipEnd { get; set; }

	public bool scrubbing { get; set; }

	public bool reinitEnter { get; set; }

	public bool reinitExit { get; set; }

	public uint startSeed { get; set; }

	public VisualEffectPlayableSerializedEvent[] events { get; set; }

	public uint clipEventsCount { get; set; }

	public uint prewarmStepCount { get; set; }

	public float prewarmDeltaTime { get; set; }

	public ExposedProperty prewarmEvent { get; set; }
}
