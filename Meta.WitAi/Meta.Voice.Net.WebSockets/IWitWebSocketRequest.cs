using System;
using System.Threading.Tasks;
using Meta.Voice.Net.PubSub;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;

namespace Meta.Voice.Net.WebSockets;

public interface IWitWebSocketRequest
{
	string RequestId { get; }

	string OperationId { get; }

	string ClientUserId { get; }

	string TopicId { get; set; }

	PubSubResponseOptions PublishOptions { get; set; }

	int TimeoutMs { get; set; }

	bool IsUploading { get; }

	bool IsDownloading { get; }

	bool IsComplete { get; }

	TaskCompletionSource<bool> Completion { get; }

	int Code { get; }

	string Error { get; }

	VoiceErrorSimulationType SimulatedErrorType { get; }

	Action<string> OnRawResponse { get; set; }

	Action<IWitWebSocketRequest> OnFirstResponse { get; set; }

	Action<IWitWebSocketRequest> OnComplete { get; set; }

	void HandleUpload(UploadChunkDelegate uploadChunk);

	void HandleDownload(string jsonString, WitResponseNode jsonData, byte[] binaryData);

	void Cancel();
}
