using System.Collections;
using System.Threading.Tasks;
using Meta.Voice.Audio;
using Meta.WitAi.Json;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Utilities;

namespace Meta.WitAi.TTS.Interfaces;

public interface ISpeaker
{
	bool IsSpeaking { get; }

	bool IsPaused { get; }

	TTSSpeakerEvents Events { get; }

	TTSVoiceSettings VoiceSettings { get; }

	IAudioPlayer AudioPlayer { get; }

	void Speak(string textToSpeak, TTSSpeakerClipEvents playbackEvents);

	void SpeakQueued(string textToSpeak, TTSSpeakerClipEvents playbackEvents);

	IEnumerator SpeakAsync(string textToSpeak);

	Task SpeakTask(string textToSpeak);

	Task SpeakTask(string textToSpeak, TTSSpeakerClipEvents playbackEvents);

	Task SpeakTask(WitResponseNode responseNode, TTSSpeakerClipEvents playbackEvents);

	bool Speak(WitResponseNode responseNode, TTSSpeakerClipEvents playbackEvents);

	IEnumerator SpeakQueuedAsync(WitResponseNode responseNode, TTSSpeakerClipEvents playbackEvents);

	Task SpeakQueuedTask(WitResponseNode responseNode, TTSSpeakerClipEvents playbackEvents);

	IEnumerator SpeakQueuedAsync(string[] textsToSpeak, TTSSpeakerClipEvents playbackEvents);

	Task SpeakQueuedTask(string[] textsToSpeak, TTSSpeakerClipEvents playbackEvents);

	void Stop();

	void Pause();

	void Resume();

	void PrepareToSpeak();

	void StartTextBlock();

	void EndTextBlock();
}
