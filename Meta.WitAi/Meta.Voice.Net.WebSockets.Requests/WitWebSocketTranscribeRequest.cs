using System.Collections.Generic;

namespace Meta.Voice.Net.WebSockets.Requests;

public class WitWebSocketTranscribeRequest : WitWebSocketSpeechRequest
{
	public bool MultipleSegments { get; }

	public WitWebSocketTranscribeRequest(string endpoint, Dictionary<string, string> parameters, string requestId = null, string clientUserId = null, string operationId = null, bool multipleSegments = true)
		: base(endpoint, parameters, requestId, clientUserId, operationId, !multipleSegments)
	{
		MultipleSegments = multipleSegments;
		if (MultipleSegments)
		{
			base.PostData["data"][endpoint]["multiple_segments"] = true.ToString();
		}
	}

	public override void CloseAudioStream()
	{
		base.CloseAudioStream();
		if (base.IsDownloading && !base.IsComplete && MultipleSegments)
		{
			HandleComplete();
		}
	}
}
