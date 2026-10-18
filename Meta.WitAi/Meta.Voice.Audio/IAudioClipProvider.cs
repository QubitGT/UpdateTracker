using UnityEngine;

namespace Meta.Voice.Audio;

public interface IAudioClipProvider
{
	AudioClip Clip { get; }
}
