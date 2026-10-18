using System.Collections.Generic;
using Meta.WitAi.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.TTS.Data;

public abstract class TTSVoiceSettings : IJsonDeserializer, IJsonSerializer
{
	[Tooltip("A unique id used for linking these voice settings to a TTS Speaker")]
	[FormerlySerializedAs("settingsID")]
	public string SettingsId;

	[Tooltip("Text that is added to the front of any TTS request using this voice setting")]
	[TextArea]
	public string PrependedText;

	[TextArea]
	[Tooltip("Text that is added to the end of any TTS request using this voice setting")]
	public string AppendedText;

	public abstract string UniqueId { get; }

	public abstract Dictionary<string, string> EncodedValues { get; }

	public abstract bool DeserializeObject(WitResponseClass jsonObject);

	public abstract bool SerializeObject(WitResponseClass jsonObject);
}
