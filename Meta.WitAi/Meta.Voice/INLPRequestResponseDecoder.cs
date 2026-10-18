namespace Meta.Voice;

public interface INLPRequestResponseDecoder<TResults>
{
	TResults Decode(string rawResponse);

	int GetResponseStatusCode(TResults results);

	string GetResponseError(TResults results);

	bool GetResponseHasPartial(TResults results);

	string GetResponseTranscription(TResults results);

	bool GetResponseHasTranscription(TResults results);

	bool GetResponseIsTranscriptionFull(TResults results);
}
