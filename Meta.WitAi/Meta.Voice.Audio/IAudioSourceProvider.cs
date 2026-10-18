using UnityEngine;

namespace Meta.Voice.Audio;

public interface IAudioSourceProvider
{
	AudioSource AudioSource { get; }
}
