using System;
using UnityEngine.Scripting;

namespace Meta.WitAi.TTS.Data;

[Serializable]
public class TTSVisemeEvent : TTSEvent<Viseme>
{
	[Preserve]
	public static Viseme GetVisemeAot(string inViseme)
	{
		Enum.TryParse<Viseme>(inViseme, out var result);
		return result;
	}
}
