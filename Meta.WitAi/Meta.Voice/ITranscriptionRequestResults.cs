namespace Meta.Voice;

public interface ITranscriptionRequestResults : IVoiceRequestResults
{
	string Transcription { get; }

	string[] FinalTranscriptions { get; }

	void SetTranscription(string transcription, bool full);
}
