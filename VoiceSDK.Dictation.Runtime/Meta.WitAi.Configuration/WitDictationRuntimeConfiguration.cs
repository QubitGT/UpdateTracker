using System;
using Oculus.Voice.Dictation.Configuration;
using UnityEngine;

namespace Meta.WitAi.Configuration;

[Serializable]
public class WitDictationRuntimeConfiguration : WitRuntimeConfiguration
{
	[Header("Dictation")]
	[SerializeField]
	public DictationConfiguration dictationConfiguration;

	protected override Vector2 RecordingTimeRange => new Vector2(-1f, 300f);
}
