using Meta.Voice;
using Meta.WitAi.Json;

namespace Meta.WitAi.Requests;

public class WitResponseDecoder : INLPRequestResponseDecoder<WitResponseNode>
{
	public WitResponseNode Decode(string rawResponse)
	{
		return JsonConvert.DeserializeToken(rawResponse);
	}

	public int GetResponseStatusCode(WitResponseNode results)
	{
		return results.GetStatusCode();
	}

	public string GetResponseError(WitResponseNode results)
	{
		return results.GetError();
	}

	public bool GetResponseHasPartial(WitResponseNode results)
	{
		return !results.GetHasTranscription();
	}

	public string GetResponseTranscription(WitResponseNode results)
	{
		return results.GetTranscription();
	}

	public bool GetResponseHasTranscription(WitResponseNode results)
	{
		return results.GetHasTranscription();
	}

	public bool GetResponseIsTranscriptionFull(WitResponseNode results)
	{
		return results.GetIsTranscriptionFinal();
	}
}
