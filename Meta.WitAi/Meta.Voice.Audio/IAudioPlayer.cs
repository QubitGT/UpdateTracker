using Meta.WitAi.Json;

namespace Meta.Voice.Audio;

public interface IAudioPlayer
{
	IAudioClipStream ClipStream { get; }

	bool IsPlaying { get; }

	bool CanSetElapsedSamples { get; }

	int ElapsedSamples { get; }

	void Init();

	string GetPlaybackErrors();

	void Play(IAudioClipStream clipStream, int offsetSamples, WitResponseNode speechNode);

	void Pause();

	void Resume();

	void Stop();
}
