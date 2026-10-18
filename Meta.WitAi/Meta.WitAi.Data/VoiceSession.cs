using System;
using Meta.WitAi.Json;

namespace Meta.WitAi.Data;

[Serializable]
public class VoiceSession
{
	public VoiceService service;

	public WitResponseNode response;

	public bool validResponse;
}
