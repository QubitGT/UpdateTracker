namespace Meta.Voice;

public interface ITranscriptionRequestOptions : IVoiceRequestOptions
{
	float AudioThreshold { get; }
}
