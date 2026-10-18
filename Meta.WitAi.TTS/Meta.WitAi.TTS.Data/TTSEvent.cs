using System;
using Meta.WitAi.Json;

namespace Meta.WitAi.TTS.Data;

[Serializable]
public class TTSEvent<TData> : ITTSEvent
{
	[JsonProperty]
	internal string type;

	[JsonProperty]
	internal int offset;

	[JsonProperty]
	internal TData data;

	public string EventType => type;

	public int SampleOffset => offset;

	public TData Data => data;
}
