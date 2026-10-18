namespace Oculus.Voice.Dictation.Listeners;

public interface DictationListener
{
	void OnStart(DictationListener listener);

	void OnMicAudioLevel(float micLevel);

	void OnPartialTranscription(string transcription);

	void OnFinalTranscription(string transcription);

	void OnError(string errorType, string errorMessage);

	void OnStopped(DictationListener listener);
}
