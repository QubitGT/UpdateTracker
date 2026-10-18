using System;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.Composer.Handlers;

[Serializable]
public struct ComposerSpeakerData
{
	[SerializeField]
	[FormerlySerializedAs("speakerName")]
	public string SpeakerName;

	[SerializeField]
	[FormerlySerializedAs("speaker")]
	public TTSSpeaker Speaker;
}
