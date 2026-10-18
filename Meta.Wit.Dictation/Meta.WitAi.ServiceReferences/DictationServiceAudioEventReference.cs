using Meta.WitAi.Interfaces;
using Meta.WitAi.Utilities;
using UnityEngine;

namespace Meta.WitAi.ServiceReferences;

public class DictationServiceAudioEventReference : AudioInputServiceReference
{
	[SerializeField]
	private DictationServiceReference _dictationServiceReference;

	public override IAudioInputEvents AudioEvents => _dictationServiceReference.DictationService.AudioEvents;
}
