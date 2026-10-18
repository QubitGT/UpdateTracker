using System;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.WitAi.Speech;

[Serializable]
public class VoiceAudioEvent : UnityEvent<AudioClip>
{
}
