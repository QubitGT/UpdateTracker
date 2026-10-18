using UnityEngine;

namespace Meta.Voice.Audio;

public interface IAudioSystem
{
	AudioClipSettings ClipSettings { get; set; }

	void PreloadClipStreams(int total);

	IAudioClipStream GetAudioClipStream();

	IAudioPlayer GetAudioPlayer(GameObject root);
}
