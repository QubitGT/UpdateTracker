using Meta.WitAi.Interfaces;
using Meta.WitAi.Utilities;
using UnityEngine;

namespace Meta.WitAi.ServiceReferences;

public class VoiceServiceAudioEventReference : AudioInputServiceReference
{
	[SerializeField]
	private VoiceServiceReference _voiceServiceReference;

	public override IAudioInputEvents AudioEvents => _voiceServiceReference.VoiceService.AudioEvents;
}
