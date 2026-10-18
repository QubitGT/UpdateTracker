namespace Meta.Voice;

public interface INLPRequestOptions : ITranscriptionRequestOptions, IVoiceRequestOptions
{
	NLPRequestInputType InputType { get; set; }

	string Text { get; set; }
}
