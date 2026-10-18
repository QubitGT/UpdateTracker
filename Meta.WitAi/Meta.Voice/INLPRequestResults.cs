namespace Meta.Voice;

public interface INLPRequestResults<TResponseData> : ITranscriptionRequestResults, IVoiceRequestResults
{
	TResponseData ResponseData { get; }

	void SetResponseData(TResponseData responseData);
}
